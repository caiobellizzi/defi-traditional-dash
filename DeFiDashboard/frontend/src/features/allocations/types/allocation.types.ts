/**
 * Allocation Types
 */

export interface Allocation {
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

export interface CreateAllocationDto {
  clientId: string;
  assetType: string;
  assetId: string;
  allocationType: string;
  allocationValue: number;
  startDate: string;
  notes?: string;
}

export interface UpdateAllocationDto {
  allocationType?: string;
  allocationValue?: number;
  notes?: string;
}
