/**
 * Transaction Types
 */

export interface Transaction {
  id: string;
  transactionType: string; // 'Wallet' or 'Account'
  assetId: string;
  transactionHash: string | null;
  externalId: string | null;
  chain: string | null;
  direction: string; // 'IN', 'OUT', 'INTERNAL'
  fromAddress: string | null;
  toAddress: string | null;
  tokenSymbol: string | null;
  amount: number;
  amountUsd: number | null;
  fee: number | null;
  feeUsd: number | null;
  description: string | null;
  category: string | null;
  transactionDate: string;
  isManualEntry: boolean;
  status: string;
  createdAt: string;
}

export interface TransactionPagedResult {
  items: Transaction[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface TransactionFilters {
  pageNumber?: number;
  pageSize?: number;
  transactionType?: string;
  assetId?: string;
  direction?: string;
  fromDate?: string;
  toDate?: string;
  tokenSymbol?: string;
  status?: string;
}
