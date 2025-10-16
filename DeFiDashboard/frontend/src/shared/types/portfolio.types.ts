/**
 * Portfolio-related TypeScript types
 */

export interface TokenBalanceDto {
  chain: string;
  tokenSymbol: string;
  balance: number;
  balanceUsd?: number;
}

export interface PortfolioAssetDto {
  assetType: string; // "Wallet" | "Account"
  assetId: string;
  assetIdentifier: string;
  allocationType: string; // "Percentage" | "FixedAmount"
  allocationValue: number;
  totalAssetValueUsd: number;
  clientAllocatedValueUsd: number;
  tokens: TokenBalanceDto[];
}

export interface ClientPortfolioDto {
  clientId: string;
  clientName: string | null;
  totalValueUsd: number;
  cryptoValueUsd: number;
  traditionalValueUsd: number;
  assets: PortfolioAssetDto[];
  calculatedAt: string;
}

export interface PortfolioOverviewDto {
  totalAUM: number;
  totalClients: number;
  totalCryptoValue: number;
  totalTraditionalValue: number;
  topClientsByValue: Array<{
    clientId: string;
    clientName: string;
    totalValue: number;
  }>;
  assetDistribution: Array<{
    assetType: string;
    value: number;
    percentage: number;
  }>;
}
