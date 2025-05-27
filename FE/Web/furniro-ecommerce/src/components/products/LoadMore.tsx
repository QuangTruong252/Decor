'use client';

import React, { useState } from 'react';

interface LoadMoreProps {
  onLoadMore: () => Promise<void>;
  hasMore: boolean;
  isLoading?: boolean;
  className?: string;
  loadingText?: string;
  loadMoreText?: string;
  noMoreText?: string;
}

const LoadMore: React.FC<LoadMoreProps> = ({
  onLoadMore,
  hasMore,
  isLoading = false,
  className = "",
  loadingText = "Loading more products...",
  loadMoreText = "Load More Products",
  noMoreText = "No more products to load"
}) => {
  const [isLoadingMore, setIsLoadingMore] = useState(false);

  const handleLoadMore = async () => {
    if (isLoading || isLoadingMore || !hasMore) return;

    try {
      setIsLoadingMore(true);
      await onLoadMore();
    } catch (error) {
      console.error('Error loading more products:', error);
    } finally {
      setIsLoadingMore(false);
    }
  };

  if (!hasMore && !isLoading && !isLoadingMore) {
    return (
      <div className={`text-center py-8 ${className}`}>
        <p className="text-gray-500">{noMoreText}</p>
      </div>
    );
  }

  return (
    <div className={`text-center py-8 ${className}`}>
      {(isLoading || isLoadingMore) ? (
        <div className="flex flex-col items-center space-y-4">
          <div className="animate-spin w-8 h-8 border-4 border-gray-300 border-t-primary rounded-full"></div>
          <p className="text-gray-600">{loadingText}</p>
        </div>
      ) : hasMore ? (
        <button
          onClick={handleLoadMore}
          className="bg-primary text-white px-8 py-3 rounded-lg hover:bg-primary-dark transition-colors font-medium"
        >
          {loadMoreText}
        </button>
      ) : null}
    </div>
  );
};

// Infinite scroll hook
export const useInfiniteScroll = (
  callback: () => Promise<void>,
  hasMore: boolean,
  threshold: number = 100
) => {
  React.useEffect(() => {
    const handleScroll = () => {
      if (
        window.innerHeight + document.documentElement.scrollTop + threshold >=
        document.documentElement.offsetHeight &&
        hasMore
      ) {
        callback();
      }
    };

    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, [callback, hasMore, threshold]);
};

// Infinite scroll component
interface InfiniteScrollProps {
  onLoadMore: () => Promise<void>;
  hasMore: boolean;
  isLoading?: boolean;
  threshold?: number;
  children: React.ReactNode;
  className?: string;
  loadingComponent?: React.ReactNode;
}

export const InfiniteScroll: React.FC<InfiniteScrollProps> = ({
  onLoadMore,
  hasMore,
  isLoading = false,
  threshold = 100,
  children,
  className = "",
  loadingComponent
}) => {
  useInfiniteScroll(onLoadMore, hasMore && !isLoading, threshold);

  const defaultLoadingComponent = (
    <div className="flex justify-center py-8">
      <div className="animate-spin w-8 h-8 border-4 border-gray-300 border-t-primary rounded-full"></div>
    </div>
  );

  return (
    <div className={className}>
      {children}
      {isLoading && (loadingComponent || defaultLoadingComponent)}
      {!hasMore && !isLoading && (
        <div className="text-center py-8">
          <p className="text-gray-500">No more products to load</p>
        </div>
      )}
    </div>
  );
};

export default LoadMore;
