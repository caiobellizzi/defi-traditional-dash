/**
 * Alert Types
 */

export interface Alert {
  id: string;
  alertType: string;
  severity: 'Low' | 'Medium' | 'High' | 'Critical';
  title: string;
  description: string;
  metadata: Record<string, unknown>;
  status: 'New' | 'Acknowledged' | 'Resolved';
  createdAt: string;
  acknowledgedAt?: string;
  resolvedAt?: string;
}

export interface AlertFilters {
  alertType?: string;
  severity?: string;
  status?: string;
}
