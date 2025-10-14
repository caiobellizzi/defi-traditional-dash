/**
 * API Types - Matches backend DTOs
 */

// Client Types
export interface ClientDto {
  id: string;
  name: string | null;
  email: string | null;
  document: string | null;
  phoneNumber: string | null;
  status: string | null;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface ClientDtoPagedResult {
  items: ClientDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateClientCommand {
  name: string;
  email: string;
  document?: string;
  phoneNumber?: string;
  notes?: string;
}

export interface UpdateClientCommand {
  name?: string;
  email?: string;
  document?: string;
  phoneNumber?: string;
  status?: string;
  notes?: string;
}

// Wallet Types
export interface WalletDto {
  id: string;
  walletAddress: string;
  label: string;
  supportedChains: string[];
  status: string;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface WalletDtoPagedResult {
  items: WalletDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface AddWalletCommand {
  walletAddress: string;
  label: string;
  supportedChains: string[];
  notes?: string;
}

// Allocation Types
export interface AllocationDto {
  id: string;
  clientId: string;
  clientName: string;
  assetType: string;
  assetId: string;
  assetName: string;
  allocationType: string;
  allocationValue: number;
  startDate: string;
  endDate?: string;
  notes: string | null;
  createdAt: string;
}

export interface CreateAllocationCommand {
  clientId: string;
  assetType: string;
  assetId: string;
  allocationType: string;
  allocationValue: number;
  startDate: string;
  notes?: string;
}

// Portfolio Types
export interface ClientPortfolioDto {
  clientId: string;
  clientName: string;
  totalValueUsd: number;
  allocations: AllocationDto[];
  lastUpdated: string;
}

// API Error Response
export interface ApiErrorResponse {
  message: string;
  errors?: Record<string, string[]>;
}
