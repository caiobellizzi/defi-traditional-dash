import { useState } from 'react';
import Button from '@/shared/components/ui/Button';
import { ExportJobStatus } from './ExportJobStatus';
import {
  useExportPortfolioPdf,
  useExportTransactionsExcel,
  useExportPerformanceExcel,
} from '../hooks/useExportJobs';
import type { ExportType } from '../types/export.types';

/**
 * Export button props
 */
interface ExportButtonProps {
  type: ExportType;
  clientId?: string;
  includeTransactions?: boolean;
  fromDate?: string;
  toDate?: string;
  transactionType?: string;
  variant?: 'primary' | 'secondary' | 'success';
  size?: 'sm' | 'md' | 'lg';
  className?: string;
  children?: React.ReactNode;
}

/**
 * Helper to generate filename based on export type
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
 * Helper to get button label based on export type
 */
const getButtonLabel = (type: ExportType): string => {
  switch (type) {
    case 'portfolio-pdf':
      return 'Export Portfolio PDF';
    case 'transactions-excel':
      return 'Export Transactions';
    case 'performance-excel':
      return 'Export Performance';
    default:
      return 'Export';
  }
};

/**
 * Export button component with integrated job status tracking
 * Handles the complete export workflow: trigger -> poll -> download
 */
export const ExportButton = ({
  type,
  clientId,
  includeTransactions = false,
  fromDate,
  toDate,
  transactionType,
  variant = 'primary',
  size = 'md',
  className = '',
  children,
}: ExportButtonProps) => {
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
            throw new Error('Client ID is required for portfolio export');
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
          if (!fromDate || !toDate) {
            throw new Error('Date range is required for performance export');
          }
          job = await exportPerformance.mutateAsync({
            clientId: clientId || null,
            fromDate,
            toDate,
          });
          break;

        default:
          throw new Error('Invalid export type');
      }

      setActiveJobId(job.jobId);
    } catch (error) {
      console.error('Export failed:', error);
    }
  };

  const handleJobComplete = () => {
    setActiveJobId(null);
  };

  const isLoading =
    exportPortfolio.isPending ||
    exportTransactions.isPending ||
    exportPerformance.isPending;

  return (
    <div className="space-y-3">
      <Button
        variant={variant}
        size={size}
        onClick={handleExport}
        isLoading={isLoading}
        disabled={isLoading || !!activeJobId}
        className={className}
      >
        {children || getButtonLabel(type)}
      </Button>

      {activeJobId && (
        <ExportJobStatus
          jobId={activeJobId}
          filename={generateFilename(type, clientId)}
          onComplete={handleJobComplete}
          autoDownload={true}
        />
      )}
    </div>
  );
};
