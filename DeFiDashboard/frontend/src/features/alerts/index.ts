/**
 * Alerts Feature - Barrel Export
 */

// Types
export * from './types/alert.types';

// API
export { alertsApi } from './api/alerts.api';

// Hooks
export {
  useAlerts,
  useAlert,
  useAlertsSummary,
  useAcknowledgeAlert,
  useResolveAlert,
  useDismissAlert,
} from './hooks/useAlerts';

// Components
export { AlertBadge } from './components/AlertBadge';
export { AlertList } from './components/AlertList';
export { AlertActions } from './components/AlertActions';

// Pages
export { default as AlertsPage } from './pages/AlertsPage';
