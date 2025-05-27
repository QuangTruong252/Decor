import { BaseEntity } from './common';

// Review related types
export interface ReviewDTO extends BaseEntity {
  userId: number;
  userName: string | null;
  productId: number;
  rating: number;
  comment: string | null;
}

export interface CreateReviewDTO {
  userId: number;
  productId: number;
  rating: number;
  comment?: string;
}

export interface UpdateReviewDTO {
  rating: number;
  comment?: string;
}

// Extended review types
export interface Review extends ReviewDTO {
  productName: string;
  productImage: string;
  userAvatar?: string;
  isVerifiedPurchase: boolean;
  helpfulCount: number;
  isHelpful?: boolean;
  canEdit: boolean;
  canDelete: boolean;
  images?: ReviewImage[];
}

export interface ReviewImage {
  id: number;
  reviewId: number;
  url: string;
  filename: string;
  size: number;
}

// Review statistics
export interface ReviewStats {
  totalReviews: number;
  averageRating: number;
  ratingDistribution: {
    1: number;
    2: number;
    3: number;
    4: number;
    5: number;
  };
  verifiedPurchaseCount: number;
  withImagesCount: number;
}

// Review form data
export interface ReviewFormData {
  rating: number;
  comment: string;
  images?: File[];
  isAnonymous?: boolean;
}

// Review search and filter
export interface ReviewSearchParams {
  productId?: number;
  userId?: number;
  rating?: number;
  minRating?: number;
  maxRating?: number;
  hasComment?: boolean;
  hasImages?: boolean;
  verifiedPurchase?: boolean;
  sortBy?: 'createdAt' | 'rating' | 'helpfulCount';
  sortOrder?: 'asc' | 'desc';
  page?: number;
  limit?: number;
}

export interface ReviewFilters {
  ratings: number[];
  hasComment: boolean;
  hasImages: boolean;
  verifiedPurchase: boolean;
  dateRange: {
    from: string;
    to: string;
  };
}

// Review moderation
export interface ReviewModeration {
  id: number;
  reviewId: number;
  status: 'pending' | 'approved' | 'rejected' | 'flagged';
  reason?: string;
  moderatedBy?: number;
  moderatedAt?: string;
  notes?: string;
}

export interface ReviewReport {
  id: number;
  reviewId: number;
  reportedBy: number;
  reason: 'spam' | 'inappropriate' | 'fake' | 'offensive' | 'other';
  description?: string;
  status: 'pending' | 'resolved' | 'dismissed';
  createdAt: string;
}

// Review analytics
export interface ReviewAnalytics {
  totalReviews: number;
  averageRating: number;
  reviewsThisMonth: number;
  ratingTrend: Array<{
    date: string;
    averageRating: number;
    count: number;
  }>;
  topReviewedProducts: Array<{
    productId: number;
    productName: string;
    reviewCount: number;
    averageRating: number;
  }>;
  reviewsByRating: Record<number, number>;
}

// Review helpful/unhelpful
export interface ReviewHelpful {
  id: number;
  reviewId: number;
  userId: number;
  isHelpful: boolean;
  createdAt: string;
}

// Review response (from business)
export interface ReviewResponse {
  id: number;
  reviewId: number;
  response: string;
  respondedBy: number;
  respondedAt: string;
  isPublic: boolean;
}

// Hooks return types
export interface UseReviewsReturn {
  reviews: Review[];
  stats: ReviewStats;
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
  hasMore: boolean;
  loadMore: () => void;
}

export interface UseReviewReturn {
  review: Review | null;
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
  updateReview: (data: UpdateReviewDTO) => Promise<void>;
  deleteReview: () => Promise<void>;
  markHelpful: (isHelpful: boolean) => Promise<void>;
}

export interface UseCreateReviewReturn {
  createReview: (data: CreateReviewDTO) => Promise<void>;
  isCreating: boolean;
  error: string | null;
  clearError: () => void;
}

export interface UseProductReviewsReturn {
  reviews: Review[];
  stats: ReviewStats;
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
  canReview: boolean;
  userReview?: Review;
  submitReview: (data: ReviewFormData) => Promise<void>;
  updateReview: (data: UpdateReviewDTO) => Promise<void>;
  deleteReview: () => Promise<void>;
}
