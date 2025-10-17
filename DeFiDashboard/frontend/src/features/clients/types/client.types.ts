/**
 * Client Types
 * Organized in the clients feature module
 */

export interface Client {
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

export interface ClientPagedResult {
  items: Client[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateClientDto {
  name: string;
  email: string;
  document?: string;
  phoneNumber?: string;
  notes?: string;
}

export interface UpdateClientDto {
  name?: string;
  email?: string;
  document?: string;
  phoneNumber?: string;
  status?: string;
  notes?: string;
}

export interface ClientPortfolio {
  clientId: string;
  clientName: string;
  totalValueUsd: number;
  allocations: ClientAllocation[];
  lastUpdated: string;
}

export interface ClientAllocation {
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
