import { useState, useCallback } from 'react';

/**
 * Pagination hook for managing paginated data
 */
export interface PaginationState {
  pageNumber: number;
  pageSize: number;
}

export interface UsePaginationResult {
  pageNumber: number;
  pageSize: number;
  setPageNumber: (page: number) => void;
  setPageSize: (size: number) => void;
  nextPage: () => void;
  previousPage: () => void;
  reset: () => void;
}

export function usePagination(
  initialPageNumber = 1,
  initialPageSize = 10
): UsePaginationResult {
  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const [pageSize, setPageSize] = useState(initialPageSize);

  const nextPage = useCallback(() => {
    setPageNumber((prev) => prev + 1);
  }, []);

  const previousPage = useCallback(() => {
    setPageNumber((prev) => Math.max(1, prev - 1));
  }, []);

  const reset = useCallback(() => {
    setPageNumber(initialPageNumber);
    setPageSize(initialPageSize);
  }, [initialPageNumber, initialPageSize]);

  return {
    pageNumber,
    pageSize,
    setPageNumber,
    setPageSize,
    nextPage,
    previousPage,
    reset,
  };
}
