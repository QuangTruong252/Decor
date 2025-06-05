"use client";

import { API_URL, fetchWithAuth } from "@/lib/api-utils";

/**
 * Review DTO from API
 */
export interface ReviewDTO {
  id: number;
  productId: number;
  userId: number;
  userFullName: string;
  rating: number;
  comment: string | null;
  isVerifiedPurchase: boolean;
  createdAt: string;
  updatedAt: string;
}

/**
 * Product rating summary
 */
export interface ProductRatingDTO {
  productId: number;
  averageRating: number;
  totalReviews: number;
  ratingDistribution: {
    [rating: number]: number;
  };
}

/**
 * Create review payload
 */
export interface CreateReviewPayload {
  productId: number;
  rating: number;
  comment?: string;
}

/**
 * Update review payload
 */
export interface UpdateReviewPayload {
  rating?: number;
  comment?: string;
}

/**
 * Get product reviews
 * @param productId Product ID
 * @param pageNumber Page number (optional)
 * @param pageSize Page size (optional)
 * @returns List of reviews for the product
 * @endpoint GET /api/Review/product/{productId}
 */
export async function getProductReviews(
  productId: number,
  pageNumber?: number,
  pageSize?: number
): Promise<ReviewDTO[]> {
  try {
    let url = `${API_URL}/api/Review/product/${productId}`;
    const params = [];
    
    if (pageNumber) {
      params.push(`pageNumber=${pageNumber}`);
    }
    
    if (pageSize) {
      params.push(`pageSize=${pageSize}`);
    }
    
    if (params.length > 0) {
      url += `?${params.join('&')}`;
    }

    const response = await fetchWithAuth(url);

    if (!response.ok) {
      throw new Error("Unable to fetch product reviews");
    }

    return response.json();
  } catch (error) {
    console.error(`Get product reviews for ${productId} error:`, error);
    throw new Error("Unable to fetch product reviews. Please try again later.");
  }
}

/**
 * Get review by ID
 * @param id Review ID
 * @returns Review details
 * @endpoint GET /api/Review/{id}
 */
export async function getReviewById(id: number): Promise<ReviewDTO> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Review/${id}`);

    if (!response.ok) {
      throw new Error("Unable to fetch review details");
    }

    return response.json();
  } catch (error) {
    console.error(`Get review by id ${id} error:`, error);
    throw new Error("Unable to fetch review details. Please try again later.");
  }
}

/**
 * Create a new review
 * @param review Review data
 * @returns Created review
 * @endpoint POST /api/Review
 */
export async function createReview(review: CreateReviewPayload): Promise<ReviewDTO> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Review`, {
      method: "POST",
      body: JSON.stringify(review),
    });

    if (!response.ok) {
      throw new Error("Unable to create review");
    }

    return response.json();
  } catch (error) {
    console.error("Create review error:", error);
    throw new Error("Unable to create review. Please try again later.");
  }
}

/**
 * Update a review
 * @param id Review ID
 * @param review Review data
 * @returns void
 * @endpoint PUT /api/Review/{id}
 */
export async function updateReview(id: number, review: UpdateReviewPayload): Promise<void> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Review/${id}`, {
      method: "PUT",
      body: JSON.stringify(review),
    });

    if (!response.ok) {
      throw new Error("Unable to update review");
    }
  } catch (error) {
    console.error(`Update review ${id} error:`, error);
    throw new Error("Unable to update review. Please try again later.");
  }
}

/**
 * Delete a review
 * @param id Review ID
 * @returns void
 * @endpoint DELETE /api/Review/{id}
 */
export async function deleteReview(id: number): Promise<void> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Review/${id}`, {
      method: "DELETE",
    });

    if (!response.ok) {
      throw new Error("Unable to delete review");
    }
  } catch (error) {
    console.error(`Delete review ${id} error:`, error);
    throw new Error("Unable to delete review. Please try again later.");
  }
}

/**
 * Get product rating
 * @param productId Product ID
 * @returns Product rating summary
 * @endpoint GET /api/Review/product/{productId}/rating
 */
export async function getProductRating(productId: number): Promise<ProductRatingDTO> {
  try {
    const response = await fetchWithAuth(`${API_URL}/api/Review/product/${productId}/rating`);

    if (!response.ok) {
      throw new Error("Unable to fetch product rating");
    }

    return response.json();
  } catch (error) {
    console.error(`Get product rating for ${productId} error:`, error);
    throw new Error("Unable to fetch product rating. Please try again later.");
  }
}
