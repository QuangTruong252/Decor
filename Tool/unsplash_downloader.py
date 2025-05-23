#!/usr/bin/env python3
"""
Unsplash Image Downloader

This script downloads images from Unsplash based on a list of search keywords.
It respects Unsplash's rate limits and provides progress tracking.
It handles similar keywords by ensuring different images are downloaded for each.
"""

import os
import time
import re
import random
import argparse
import json
import collections
from urllib.parse import quote, urlencode
from pathlib import Path

import requests
from bs4 import BeautifulSoup
from tqdm import tqdm

# Constants
USER_AGENTS = [
    'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
    'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.1 Safari/605.1.15',
    'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0',
    'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36'
]

# Using a public Unsplash API access key for demo purposes
# For production use, you should register your own application at https://unsplash.com/developers
# This is a demo key with limited usage
UNSPLASH_ACCESS_KEY = 'ab3411e4ac868c2646c0ed488dfd919ef612b04c264f3374c97fff98ed253dc9'

DELAY_BETWEEN_REQUESTS = 3  # seconds
OUTPUT_DIR = 'Products'
MAX_IMAGES_PER_KEYWORD = 30  # Maximum number of images to fetch per base keyword
RESULTS_PER_PAGE = 10  # Number of results per page in Unsplash API


def extract_base_keyword(keyword):
    """
    Extract the base keyword from a numbered keyword.

    Args:
        keyword (str): The keyword, possibly with a number suffix

    Returns:
        tuple: (base_keyword, index)

    Examples:
        "chair-1" -> ("chair", 1)
        "office chair 2" -> ("office chair", 2)
        "dining table" -> ("dining table", None)
    """
    # Pattern to match a keyword ending with a number (with optional separator)
    pattern = r'^(.*?)[-_ ]?(\d+)$'
    match = re.match(pattern, keyword)

    if match:
        base_keyword = match.group(1).strip()
        index = int(match.group(2))
        return base_keyword, index

    return keyword, None


def sanitize_filename(filename):
    """
    Convert a string to a valid filename by removing invalid characters.

    Args:
        filename (str): The string to convert to a valid filename

    Returns:
        str: A valid filename
    """
    # Replace spaces with underscores and remove invalid filename characters
    valid_filename = re.sub(r'[\\/*?:"<>|]', "", filename)
    valid_filename = valid_filename.replace(' ', '_')
    return valid_filename


def get_unsplash_image_url(keyword, page=1, used_image_ids=None):
    """
    Use the Unsplash API to get the URL of an image for a keyword.

    Args:
        keyword (str): The search keyword
        page (int): The page number to fetch (for pagination)
        used_image_ids (set): Set of already used image IDs to avoid duplicates

    Returns:
        tuple: (image_url, image_id) or (None, None) if no image found
    """
    if used_image_ids is None:
        used_image_ids = set()

    print(f"DEBUG: Starting search for keyword: {keyword}, page: {page}")

    # Use the Unsplash API to search for photos
    api_url = "https://api.unsplash.com/search/photos"
    params = {
        "query": keyword,
        "per_page": RESULTS_PER_PAGE,  # Get multiple results per page
        "page": page,
        "orientation": "landscape"  # Prefer landscape images for decoration
    }

    headers = {
        "Authorization": f"Client-ID {UNSPLASH_ACCESS_KEY}",
        "Accept-Version": "v1",
        "User-Agent": random.choice(USER_AGENTS)
    }

    try:
        print(f"DEBUG: Sending API request to Unsplash: {api_url}?{urlencode(params)}")
        response = requests.get(api_url, headers=headers, params=params, timeout=10)
        response.raise_for_status()
        print(f"DEBUG: Response status code: {response.status_code}")

        data = response.json()
        print(f"DEBUG: Received JSON response with {len(data.get('results', []))} results")

        # Check if we got any results
        if data.get('results') and len(data['results']) > 0:
            # Try to find an image that hasn't been used yet
            for result in data['results']:
                image_id = result['id']

                # Skip if we've already used this image
                if image_id in used_image_ids:
                    print(f"DEBUG: Skipping already used image ID: {image_id}")
                    continue

                # Get the image URL - we'll use the regular size
                image_url = result['urls']['regular']
                print(f"DEBUG: Found image URL: {image_url}")

                # Also print some info about the image
                print(f"DEBUG: Image ID: {image_id}")
                print(f"DEBUG: Image description: {result.get('description') or result.get('alt_description') or 'No description'}")
                print(f"DEBUG: Image dimensions: {result.get('width')}x{result.get('height')}")

                return image_url, image_id

            # If we've gone through all results and all are used, return None
            print(f"DEBUG: All images on page {page} have been used already")
            return None, None
        else:
            print(f"DEBUG: No results found for '{keyword}' on page {page}")
            return None, None

    except requests.RequestException as e:
        print(f"Error fetching search results for '{keyword}': {e}")
        return None, None
    except (ValueError, KeyError, json.JSONDecodeError) as e:
        print(f"Error parsing API response for '{keyword}': {e}")
        return None, None


def download_image(url, output_path):
    """
    Download an image from a URL and save it to the specified path.

    Args:
        url (str): URL of the image to download
        output_path (str): Path where the image will be saved

    Returns:
        bool: True if download was successful, False otherwise
    """
    print(f"DEBUG: Downloading image from URL: {url}")
    print(f"DEBUG: Output path: {output_path}")

    headers = {
        'User-Agent': random.choice(USER_AGENTS),
        'Accept': 'image/webp,image/apng,image/*,*/*;q=0.8',
        'Accept-Language': 'en-US,en;q=0.9',
        'Referer': 'https://unsplash.com/'
    }

    try:
        print("DEBUG: Sending request to download image...")
        response = requests.get(url, headers=headers, stream=True, timeout=15)
        response.raise_for_status()
        print(f"DEBUG: Response status code: {response.status_code}")

        # Ensure the output directory exists
        output_dir = os.path.dirname(output_path)
        if output_dir:  # Only create directory if path has a directory component
            print(f"DEBUG: Creating directory if needed: {output_dir}")
            os.makedirs(output_dir, exist_ok=True)

        # Save the image
        print("DEBUG: Saving image to file...")
        total_size = 0
        with open(output_path, 'wb') as f:
            for chunk in response.iter_content(chunk_size=8192):
                if chunk:
                    f.write(chunk)
                    total_size += len(chunk)

        print(f"DEBUG: Image saved successfully to {output_path}")
        print(f"DEBUG: Total size: {total_size / 1024:.2f} KB")
        return True

    except requests.RequestException as e:
        print(f"Error downloading image: {e}")
        return False


def group_similar_keywords(keywords):
    """
    Group keywords that share the same base keyword.

    Args:
        keywords (list): List of keywords

    Returns:
        dict: Dictionary mapping base keywords to lists of (keyword, index) tuples
    """
    keyword_groups = collections.defaultdict(list)

    for keyword in keywords:
        base_keyword, index = extract_base_keyword(keyword)
        keyword_groups[base_keyword].append((keyword, index))

    # Sort each group by index
    for base_keyword in keyword_groups:
        keyword_groups[base_keyword].sort(key=lambda x: x[1] if x[1] is not None else 0)

    return keyword_groups


def main():
    """
    Main function to orchestrate the image downloading process.
    """
    parser = argparse.ArgumentParser(description='Download images from Unsplash based on keywords.')
    parser.add_argument('--keywords', nargs='+', help='List of keywords to search for')
    parser.add_argument('--output-dir', default=OUTPUT_DIR, help=f'Output directory (default: {OUTPUT_DIR})')
    parser.add_argument('--delay', type=int, default=DELAY_BETWEEN_REQUESTS,
                        help=f'Delay between requests in seconds (default: {DELAY_BETWEEN_REQUESTS})')

    args = parser.parse_args()

    # Use default keywords if none provided
    keywords = args.keywords or [
  "outdoor-rug-1",
  "outdoor-rug-2",
  "outdoor-rug-3",
  "solar-garden-lights-1",
  "solar-garden-lights-2",
  "outdoor-lantern-set-1",
  "outdoor-lantern-set-2",
  "garden-fountain-1",
  "garden-fountain-2",
  "garden-fountain-3",
  "garden-planter-box-1",
  "garden-planter-box-2",
  "garden-planter-box-3",
  "folding-deck-chair-1",
  "folding-deck-chair-2",
  "hanging-egg-chair-1",
  "hanging-egg-chair-2",
  "hanging-egg-chair-3",
  "stackable-outdoor-chairs-1",
  "stackable-outdoor-chairs-2",
  "stackable-outdoor-chairs-3",
  "outdoor-rocking-chair-1",
  "outdoor-rocking-chair-2",
  "outdoor-rocking-chair-3",
  "garden-lounge-chair-1",
  "garden-lounge-chair-2",
  "adirondack-chair-set-1",
  "adirondack-chair-set-2",
  "adirondack-chair-set-3",
  "fire-pit-table-set-1",
  "fire-pit-table-set-2",
  "fire-pit-table-set-3",
  "bistro-set-1",
  "bistro-set-2",
  "outdoor-sofa-set-1",
  "outdoor-sofa-set-2",
  "outdoor-sofa-set-3",
  "patio-dining-set-1",
  "patio-dining-set-2",
  "patio-dining-set-3",
  "wooden-file-cabinet-1",
  "wooden-file-cabinet-2",
  "wooden-file-cabinet-3",
  "fireproof-file-cabinet-1",
  "fireproof-file-cabinet-2",
  "mobile-pedestal-1",
  "mobile-pedestal-2",
  "mobile-pedestal-3",
  "lateral-file-cabinet-1",
  "lateral-file-cabinet-2",
  "lateral-file-cabinet-3",
  "3-drawer-filing-cabinet-1",
  "3-drawer-filing-cabinet-2",
  "3-drawer-filing-cabinet-3",
  "gaming-office-chair-1",
  "gaming-office-chair-2",
  "gaming-office-chair-3",
  "task-chair-1",
  "task-chair-2",
  "task-chair-3",
  "kneeling-chair-1",
  "kneeling-chair-2",
  "ergonomic-mesh-chair-1",
  "ergonomic-mesh-chair-2",
  "executive-office-chair-1",
  "executive-office-chair-2",
  "standing-desk-1",
  "standing-desk-2",
  "compact-writing-desk-1",
  "compact-writing-desk-2",
  "compact-writing-desk-3",
  "industrial-desk-1",
  "industrial-desk-2",
  "industrial-desk-3",
  "l-shaped-corner-desk-1",
  "l-shaped-corner-desk-2",
  "ergonomic-desk-1",
  "ergonomic-desk-2",
  "compact-credenza-1",
  "compact-credenza-2",
  "compact-credenza-3",
  "industrial-buffet-1",
  "industrial-buffet-2",
  "industrial-buffet-3",
  "wine-cabinet-1",
  "wine-cabinet-2",
  "buffet-cabinet-with-hutch-1",
  "buffet-cabinet-with-hutch-2",
  "modern-sideboard-1",
  "modern-sideboard-2",
  "modern-sideboard-3",
  "industrial-dining-chairs-1",
  "industrial-dining-chairs-2",
  "industrial-dining-chairs-3"
]

    # Create output directory if it doesn't exist
    output_dir = Path(args.output_dir)
    output_dir.mkdir(exist_ok=True)

    print(f"Starting download of {len(keywords)} images from Unsplash...")

    # Group similar keywords
    keyword_groups = group_similar_keywords(keywords)
    print(f"Grouped {len(keywords)} keywords into {len(keyword_groups)} base keyword groups")

    # Dictionary to track used image IDs for each base keyword
    used_image_ids = {}

    # Process each keyword with progress tracking
    with tqdm(total=len(keywords), desc="Processing keywords") as pbar:
        for base_keyword, keyword_tuples in keyword_groups.items():
            print(f"\nProcessing base keyword: {base_keyword} with {len(keyword_tuples)} variations")

            # Initialize set of used image IDs for this base keyword
            if base_keyword not in used_image_ids:
                used_image_ids[base_keyword] = set()

            # Process each keyword in the group
            for keyword, index in keyword_tuples:
                print(f"\nProcessing: {keyword}")

                # Calculate page number based on index
                # If index is None, use page 1
                # Otherwise, use ceiling division to determine page
                # This ensures we get different images for each index
                page = 1
                if index is not None:
                    page = (index - 1) // RESULTS_PER_PAGE + 1

                # Try to get an image that hasn't been used yet
                max_attempts = 3  # Try a few pages if needed
                image_url = None
                image_id = None

                for attempt in range(1, max_attempts + 1):
                    current_page = page + attempt - 1
                    image_url, image_id = get_unsplash_image_url(
                        base_keyword,
                        page=current_page,
                        used_image_ids=used_image_ids[base_keyword]
                    )

                    if image_url:
                        break

                    print(f"Attempt {attempt} failed, trying next page...")

                if not image_url:
                    print(f"No suitable image found for '{keyword}' after {max_attempts} attempts. Skipping.")
                    pbar.update(1)
                    continue

                # Add the image ID to the used set
                used_image_ids[base_keyword].add(image_id)

                # Determine file extension (default to .jpg if not found)
                file_ext = os.path.splitext(image_url.split('?')[0])[1]
                if not file_ext:
                    file_ext = '.jpg'

                # Create output path
                filename = sanitize_filename(keyword) + file_ext
                output_path = output_dir / filename

                # Download the image
                print(f"Downloading image for '{keyword}' to {output_path}")
                success = download_image(image_url, output_path)

                if success:
                    print(f"Successfully downloaded image for '{keyword}'")
                else:
                    print(f"Failed to download image for '{keyword}'")

                # Respect rate limits
                time.sleep(args.delay)

                # Update progress bar
                pbar.update(1)

    print(f"\nDownload complete! Images saved to {output_dir.absolute()}")


if __name__ == "__main__":
    main()
