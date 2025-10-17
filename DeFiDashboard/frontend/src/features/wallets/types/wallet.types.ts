/**
 * Wallet Types
 */

export interface Wallet {
  id: string;
  walletAddress: string;
  label: string;
  supportedChains: string[];
  status: string;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface WalletPagedResult {
  items: Wallet[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface AddWalletDto {
  walletAddress: string;
  label: string;
  supportedChains: string[];
  notes?: string;
}

export interface WalletBalance {
  id: string;
  walletId: string;
  chain: string;
  tokenSymbol: string;
  tokenName: string | null;
  tokenAddress: string | null;
  balance: number;
  balanceUsd: number | null;
  lastSyncedAt: string;
}
