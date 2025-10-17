/**
 * Portfolio Types
 */

export interface PortfolioOverview {
  totalValueUsd: number;
  walletValueUsd: number;
  accountValueUsd: number;
  clientCount: number;
  walletCount: number;
  accountCount: number;
  lastUpdated: string;
}

export interface PortfolioAsset {
  assetType: string; // 'Wallet' or 'Account'
  assetId: string;
  assetName: string;
  valueUsd: number;
  allocations: PortfolioAllocation[];
}

export interface PortfolioAllocation {
  clientId: string;
  clientName: string;
  allocationType: string;
  allocationValue: number;
  allocatedValueUsd: number;
}

export interface ConsolidatedPortfolio {
  overview: PortfolioOverview;
  assets: PortfolioAsset[];
}
