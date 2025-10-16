import { QueryClient } from '@tanstack/react-query';

/**
 * Configure React Query client with default options
 */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
      staleTime: 30000, // 30 seconds
    },
    mutations: {
      retry: 0,
    },
  },
});
