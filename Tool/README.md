# Unsplash Image Downloader

A Python script that automatically downloads images from Unsplash based on a list of search keywords.

## Features

- Downloads images from Unsplash based on search keywords
- Respects Unsplash's rate limits with configurable delays
- Provides progress tracking during downloads
- Handles errors gracefully
- Sanitizes filenames for compatibility across operating systems
- Customizable output directory

## Requirements

- Python 3.6+
- Required Python packages:
  - requests
  - beautifulsoup4
  - tqdm

## Installation

1. Clone this repository or download the script.

2. Install the required dependencies:

```bash
pip install requests beautifulsoup4 tqdm
```

## Usage

### Basic Usage

Run the script with default keywords:

```bash
python unsplash_downloader.py
```

This will download images for the default keywords: 'bed', 'chair', 'living room', 'dining table'.

### Custom Keywords

Specify your own keywords:

```bash
python unsplash_downloader.py --keywords "modern sofa" "kitchen table" "office chair"
```

### Custom Output Directory

Change the output directory:

```bash
python unsplash_downloader.py --output-dir "MyImages"
```

### Adjust Rate Limiting

Change the delay between requests (in seconds):

```bash
python unsplash_downloader.py --delay 5
```

### Full Options

```bash
python unsplash_downloader.py --keywords "modern sofa" "kitchen table" --output-dir "FurnitureImages" --delay 4
```

## How It Works

1. For each keyword, the script:
   - Converts the keyword to a valid URL format
   - Navigates to the Unsplash search page
   - Locates and selects the first image from the search results
   - Extracts the full-resolution download URL
   - Downloads the image while respecting rate limits

2. Images are saved in the specified directory with filenames based on the search keywords.

## Notes

- This script is for educational purposes only.
- Please respect Unsplash's [terms of service](https://unsplash.com/terms) and [API guidelines](https://unsplash.com/documentation).
- Consider using the official [Unsplash API](https://unsplash.com/developers) for production applications.
- The script may need updates if Unsplash changes their website structure.

## License

MIT
