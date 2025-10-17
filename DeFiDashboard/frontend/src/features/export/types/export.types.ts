/**
 * Export Feature Types
 * Matches backend DTOs from ApiService.Features.Export
 */

/**
 * Export job status enum
 */
export type ExportJobStatus = 'Pending' | 'Processing' | 'Completed' | 'Failed';

/**
 * Export job DTO
 * Matches: ApiService.Features.Export.ExportJobDto
 */
export interface ExportJobDto {
  jobId: string;
  status: ExportJobStatus;
  createdAt: string;
  completedAt: string | null;
  fileUrl: string | null;
}

/**
 * Export portfolio to PDF command
 * Matches: ApiService.Features.Export.ExportPortfolioPdf.ExportPortfolioPdfCommand
 */
export interface ExportPortfolioPdfRequest {
  clientId: string;
  includeTransactions?: boolean;
}

/**
 * Export transactions to Excel command
 * Matches: ApiService.Features.Export.ExportTransactionsExcel.ExportTransactionsExcelCommand
 */
export interface ExportTransactionsExcelRequest {
  clientId?: string | null;
  fromDate?: string | null;
  toDate?: string | null;
  transactionType?: string | null;
}

/**
 * Export performance to Excel command
 * Matches: ApiService.Features.Export.ExportPerformanceExcel.ExportPerformanceExcelCommand
 */
export interface ExportPerformanceExcelRequest {
  clientId: string | null;
  fromDate: string;
  toDate: string;
}

/**
 * Export type discriminator
 */
export type ExportType = 'portfolio-pdf' | 'transactions-excel' | 'performance-excel';

/**
 * Export options for UI components
 */
export interface ExportOptions {
  type: ExportType;
  clientId?: string;
  includeTransactions?: boolean;
  fromDate?: Date;
  toDate?: Date;
  transactionType?: string;
}
