import { useState } from 'react';
import Dialog from '@/shared/components/ui/Dialog';
import Button from '@/shared/components/ui/Button';
import Input from '@/shared/components/ui/Input';
import { ExportJobStatus } from './ExportJobStatus';
import {
  useExportPortfolioPdf,
  useExportTransactionsExcel,
  useExportPerformanceExcel,
} from '../hooks/useExportJobs';
import type { ExportType } from '../types/export.types';

/**
 * Export dialog props
 */
interface ExportDialogProps {
  isOpen: boolean;
  onClose: () => void;
  type: ExportType;
  clientId?: string;
  defaultFromDate?: string;
  defaultToDate?: string;
}

/**
 * Helper to generate filename
 */
const generateFilename = (type: ExportType, clientId?: string): string => {
  const timestamp = new Date().toISOString().split('T')[0];
  switch (type) {
    case 'portfolio-pdf':
      return `portfolio-${clientId || 'all'}-${timestamp}.pdf`;
    case 'transactions-excel':
      return `transactions-${clientId || 'all'}-${timestamp}.xlsx`;
    case 'performance-excel':
      return `performance-${clientId || 'all'}-${timestamp}.xlsx`;
    default:
      return `export-${timestamp}.file`;
  }
};

/**
 * Export dialog component with customizable filters
 * Provides a form to configure export parameters before triggering
 */
export const ExportDialog = ({
  isOpen,
  onClose,
  type,
  clientId: initialClientId,
  defaultFromDate,
  defaultToDate,
}: ExportDialogProps) => {
  const [clientId, setClientId] = useState(initialClientId || '');
  const [includeTransactions, setIncludeTransactions] = useState(false);
  const [fromDate, setFromDate] = useState(
    defaultFromDate || new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
  );
  const [toDate, setToDate] = useState(
    defaultToDate || new Date().toISOString().split('T')[0]
  );
  const [transactionType, setTransactionType] = useState('');
  const [activeJobId, setActiveJobId] = useState<string | null>(null);

  const exportPortfolio = useExportPortfolioPdf();
  const exportTransactions = useExportTransactionsExcel();
  const exportPerformance = useExportPerformanceExcel();

  const handleExport = async () => {
    try {
      let job;

      switch (type) {
        case 'portfolio-pdf':
          if (!clientId) {
            alert('Client ID is required for portfolio export');
            return;
          }
          job = await exportPortfolio.mutateAsync({
            clientId,
            includeTransactions,
          });
          break;

        case 'transactions-excel':
          job = await exportTransactions.mutateAsync({
            clientId: clientId || null,
            fromDate: fromDate || null,
            toDate: toDate || null,
            transactionType: transactionType || null,
          });
          break;

        case 'performance-excel':
          job = await exportPerformance.mutateAsync({
            clientId: clientId || null,
            fromDate,
            toDate,
          });
          break;

        default:
          alert('Invalid export type');
          return;
      }

      setActiveJobId(job.jobId);
    } catch (error) {
      console.error('Export failed:', error);
    }
  };

  const handleJobComplete = () => {
    setActiveJobId(null);
    onClose();
  };

  const isLoading =
    exportPortfolio.isPending ||
    exportTransactions.isPending ||
    exportPerformance.isPending;

  const getTitle = () => {
    switch (type) {
      case 'portfolio-pdf':
        return 'Export Portfolio to PDF';
      case 'transactions-excel':
        return 'Export Transactions to Excel';
      case 'performance-excel':
        return 'Export Performance to Excel';
      default:
        return 'Export Data';
    }
  };

  return (
    <Dialog isOpen={isOpen} onClose={onClose} title={getTitle()} size="md">
      <div className="space-y-4">
        {/* Client ID field (optional for transactions/performance, required for portfolio) */}
        {(type !== 'portfolio-pdf' || !initialClientId) && (
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Client ID {type === 'portfolio-pdf' && '(Required)'}
            </label>
            <Input
              type="text"
              value={clientId}
              onChange={(e) => setClientId(e.target.value)}
              placeholder="Leave empty for all clients"
              disabled={!!initialClientId}
            />
            <p className="mt-1 text-xs text-gray-500">
              {type === 'portfolio-pdf'
                ? 'Specific client portfolio'
                : 'Optional: Filter by specific client'}
            </p>
          </div>
        )}

        {/* Portfolio-specific options */}
        {type === 'portfolio-pdf' && (
          <div className="flex items-center">
            <input
              type="checkbox"
              id="includeTransactions"
              checked={includeTransactions}
              onChange={(e) => setIncludeTransactions(e.target.checked)}
              className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
            />
            <label
              htmlFor="includeTransactions"
              className="ml-2 block text-sm text-gray-700 dark:text-gray-300"
            >
              Include transaction history
            </label>
          </div>
        )}

        {/* Date range fields (for transactions and performance) */}
        {(type === 'transactions-excel' || type === 'performance-excel') && (
          <>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                From Date {type === 'performance-excel' && '(Required)'}
              </label>
              <Input
                type="date"
                value={fromDate}
                onChange={(e) => setFromDate(e.target.value)}
                max={toDate}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                To Date {type === 'performance-excel' && '(Required)'}
              </label>
              <Input
                type="date"
                value={toDate}
                onChange={(e) => setToDate(e.target.value)}
                min={fromDate}
                max={new Date().toISOString().split('T')[0]}
              />
            </div>
          </>
        )}

        {/* Transaction type filter (transactions only) */}
        {type === 'transactions-excel' && (
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Transaction Type (Optional)
            </label>
            <select
              value={transactionType}
              onChange={(e) => setTransactionType(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
            >
              <option value="">All Types</option>
              <option value="Deposit">Deposit</option>
              <option value="Withdrawal">Withdrawal</option>
              <option value="Transfer">Transfer</option>
              <option value="Trade">Trade</option>
            </select>
          </div>
        )}

        {/* Active job status */}
        {activeJobId && (
          <ExportJobStatus
            jobId={activeJobId}
            filename={generateFilename(type, clientId)}
            onComplete={handleJobComplete}
            autoDownload={true}
          />
        )}

        {/* Action buttons */}
        <div className="flex justify-end gap-2 pt-4 border-t border-gray-200 dark:border-gray-700">
          <Button variant="secondary" onClick={onClose} disabled={isLoading}>
            Cancel
          </Button>
          <Button
            variant="primary"
            onClick={handleExport}
            isLoading={isLoading}
            disabled={isLoading || !!activeJobId}
          >
            {isLoading ? 'Creating Export...' : 'Export'}
          </Button>
        </div>
      </div>
    </Dialog>
  );
};
