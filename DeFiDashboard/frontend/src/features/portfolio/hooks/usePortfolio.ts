import { useQuery } from '@tanstack/react-query';
import { portfolioApi } from '@/features/portfolio/api/portfolio.api';

/**
 * React Query hooks for portfolio operations
 */

/**
 * Fetch portfolio for a specific client
 */
export const useClientPortfolio = (clientId: string) => {
  return useQuery({
    queryKey: ['portfolio', 'client', clientId],
    queryFn: () => portfolioApi.getClientPortfolio(clientId),
    enabled: !!clientId,
    staleTime: 30000, // 30 seconds
  });
};

/**
 * Fetch portfolio overview (all clients aggregated)
 */
export const usePortfolioOverview = () => {
  return useQuery({
    queryKey: ['portfolio', 'overview'],
    queryFn: () => portfolioApi.getPortfolioOverview(),
    staleTime: 30000,
  });
};
