import { apiClient } from '@/shared/lib/api-client';
import type { ClientPortfolioDto } from '@/shared/types/portfolio.types';

/**
 * Portfolio API Service
 */
export const portfolioApi = {
  /**
   * Get portfolio for a specific client
   */
  getClientPortfolio: async (clientId: string): Promise<ClientPortfolioDto> => {
    const response = await apiClient.get<ClientPortfolioDto>(
      `/api/clients/${clientId}/portfolio`
    );
    return response.data;
  },

  /**
   * Get aggregated portfolio overview (all clients)
   * Note: Backend doesn't have this endpoint yet, so we'll aggregate client-side
   */
  getPortfolioOverview: async (): Promise<ClientPortfolioDto[]> => {
    // For now, return empty array
    // TODO: Implement backend endpoint for portfolio overview
    return [];
  },
};
