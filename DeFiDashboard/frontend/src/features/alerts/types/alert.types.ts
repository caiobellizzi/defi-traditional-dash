/**
 * Alert Types
 */

/**
 * Alert severity levels
 */
export type AlertSeverity = 'Low' | 'Medium' | 'High' | 'Critical';

/**
 * Alert types
 */
export type AlertType =
  | 'AllocationDrift'
  | 'LargeTransaction'
  | 'PriceChange'
  | 'BalanceLow'
  | 'SyncFailure'
  | 'Other';

/**
 * Alert status
 */
export type AlertStatus = 'Active' | 'Acknowledged' | 'Resolved' | 'Dismissed';

/**
 * Alert DTO
 */
export interface AlertDto {
  id: string;
  alertType: AlertType;
  severity: AlertSeverity;
  status: AlertStatus;
  title: string;
  message: string;
  relatedEntityType?: string;
  relatedEntityId?: string;
  metadata?: Record<string, any>;
  createdAt: string;
  acknowledgedAt?: string;
  acknowledgedBy?: string;
  resolvedAt?: string;
  resolvedBy?: string;
}

/**
 * Alert summary for dashboard
 */
export interface AlertsSummary {
  totalActive: number;
  criticalCount: number;
  highCount: number;
  mediumCount: number;
  lowCount: number;
  recentAlerts: AlertDto[];
}

/**
 * Alert filters
 */
export interface AlertFilters {
  status?: AlertStatus;
  severity?: AlertSeverity;
  alertType?: AlertType;
  fromDate?: string;
  toDate?: string;
}
