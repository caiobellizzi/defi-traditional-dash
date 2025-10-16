/**
 * Account Types (Traditional Finance via Pluggy)
 */

export interface Account {
  id: string;
  itemId: string; // Pluggy item ID
  accountType: string;
  accountName: string;
  institutionName: string;
  balance: number;
  currency: string;
  lastSyncedAt: string;
  status: string;
  createdAt: string;
  updatedAt: string;
}

export interface AccountBalance {
  id: string;
  accountId: string;
  balance: number;
  currency: string;
  balanceUsd: number | null;
  recordedAt: string;
}

export interface PluggyConnectToken {
  accessToken: string;
}
