import { api } from '../client';
import { REVIEWS } from '../endpoints';
import type {
  ReviewDTO,
  CreateReviewDTO,
  UpdateReviewDTO,
} from '../types';

export class ReviewService {
  /**
   * Get review by ID
   */
  static async getReviewById(id: number): Promise<ReviewDTO> {
    const response = await api.get<ReviewDTO>(REVIEWS.BY_ID(id));
    return response.data;
  }

  /**
   * Get reviews for a product
   */
  static async getProductReviews(productId: number): Promise<ReviewDTO[]> {
    const response = await api.get<ReviewDTO[]>(REVIEWS.BY_PRODUCT(productId));
    return response.data;
  }

  /**
   * Get average rating for a product
   */
  static async getProductRating(productId: number): Promise<number> {
    const response = await api.get<number>(REVIEWS.RATING_BY_PRODUCT(productId));
    return response.data;
  }

  /**
   * Create new review
   */
  static async createReview(reviewData: CreateReviewDTO): Promise<ReviewDTO> {
    const response = await api.post<ReviewDTO>(REVIEWS.BASE, reviewData);
    return response.data;
  }

  /**
   * Update review
   */
  static async updateReview(id: number, reviewData: UpdateReviewDTO): Promise<void> {
    await api.put(REVIEWS.BY_ID(id), reviewData);
  }

  /**
   * Delete review
   */
  static async deleteReview(id: number): Promise<void> {
    await api.delete(REVIEWS.BY_ID(id));
  }

  /**
   * Get reviews by user (current user's reviews)
   */
  static async getUserReviews(): Promise<ReviewDTO[]> {
    // This would typically be a separate endpoint like /Review/user
    // For now, we'll return empty array as this endpoint doesn't exist in the API
    return [];
  }

  /**
   * Check if user can review a product
   */
  static async canUserReviewProduct(productId: number): Promise<boolean> {
    try {
      // Check if user has purchased the product and hasn't reviewed it yet
      // This would typically be a separate API endpoint
      const userReviews = await this.getUserReviews();
      const hasReviewed = userReviews.some(review => review.productId === productId);

      // For now, we'll assume user can review if they haven't reviewed yet
      return !hasReviewed;
    } catch {
      return false;
    }
  }

  /**
   * Get user's review for a specific product
   */
  static async getUserReviewForProduct(productId: number): Promise<ReviewDTO | null> {
    try {
      const userReviews = await this.getUserReviews();
      return userReviews.find(review => review.productId === productId) || null;
    } catch {
      return null;
    }
  }

  /**
   * Calculate review statistics for a product
   */
  static async getProductReviewStats(productId: number): Promise<{
    totalReviews: number;
    averageRating: number;
    ratingDistribution: Record<number, number>;
  }> {
    try {
      const [reviews, averageRating] = await Promise.all([
        this.getProductReviews(productId),
        this.getProductRating(productId),
      ]);

      const totalReviews = reviews.length;
      const ratingDistribution = reviews.reduce((acc, review) => {
        acc[review.rating] = (acc[review.rating] || 0) + 1;
        return acc;
      }, {} as Record<number, number>);

      // Ensure all ratings 1-5 are represented
      for (let i = 1; i <= 5; i++) {
        if (!ratingDistribution[i]) {
          ratingDistribution[i] = 0;
        }
      }

      return {
        totalReviews,
        averageRating,
        ratingDistribution,
      };
    } catch {
      return {
        totalReviews: 0,
        averageRating: 0,
        ratingDistribution: { 1: 0, 2: 0, 3: 0, 4: 0, 5: 0 },
      };
    }
  }

  /**
   * Get recent reviews
   */
  static async getRecentReviews(limit: number = 10): Promise<ReviewDTO[]> {
    try {
      // This would typically be a separate endpoint
      // For now, we'll return empty array
      return [];
    } catch {
      return [];
    }
  }

  /**
   * Search reviews by content
   */
  static async searchReviews(query: string, productId?: number): Promise<ReviewDTO[]> {
    try {
      const reviews = productId
        ? await this.getProductReviews(productId)
        : await this.getRecentReviews(100); // Get more reviews for search

      const searchTerm = query.toLowerCase();
      return reviews.filter(review =>
        (review.comment?.toLowerCase().includes(searchTerm)) ||
        (review.userName?.toLowerCase().includes(searchTerm))
      );
    } catch {
      return [];
    }
  }

  /**
   * Get reviews by rating
   */
  static async getReviewsByRating(rating: number, productId?: number): Promise<ReviewDTO[]> {
    try {
      const reviews = productId
        ? await this.getProductReviews(productId)
        : await this.getRecentReviews(100);

      return reviews.filter(review => review.rating === rating);
    } catch {
      return [];
    }
  }

  /**
   * Format review date for display
   */
  static formatReviewDate(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffInDays = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24));

    if (diffInDays === 0) return 'Today';
    if (diffInDays === 1) return 'Yesterday';
    if (diffInDays < 7) return `${diffInDays} days ago`;
    if (diffInDays < 30) return `${Math.floor(diffInDays / 7)} weeks ago`;
    if (diffInDays < 365) return `${Math.floor(diffInDays / 30)} months ago`;

    return date.toLocaleDateString();
  }

  /**
   * Generate star rating display
   */
  static generateStarRating(rating: number): string {
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 >= 0.5;
    const emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

    return '★'.repeat(fullStars) +
           (hasHalfStar ? '☆' : '') +
           '☆'.repeat(emptyStars);
  }

  /**
   * Validate review data
   */
  static validateReviewData(reviewData: CreateReviewDTO | UpdateReviewDTO): {
    isValid: boolean;
    errors: string[];
  } {
    const errors: string[] = [];

    if ('rating' in reviewData) {
      if (reviewData.rating < 1 || reviewData.rating > 5) {
        errors.push('Rating must be between 1 and 5');
      }
    }

    if (reviewData.comment && reviewData.comment.length > 500) {
      errors.push('Comment must be 500 characters or less');
    }

    return {
      isValid: errors.length === 0,
      errors,
    };
  }

  /**
   * Get review summary text
   */
  static getReviewSummary(stats: { totalReviews: number; averageRating: number }): string {
    if (stats.totalReviews === 0) {
      return 'No reviews yet';
    }

    const rating = stats.averageRating.toFixed(1);
    const reviewText = stats.totalReviews === 1 ? 'review' : 'reviews';

    return `${rating} out of 5 stars (${stats.totalReviews} ${reviewText})`;
  }
}

export default ReviewService;
