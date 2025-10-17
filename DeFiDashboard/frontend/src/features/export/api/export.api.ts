import { apiClient } from '@/shared/lib/api-client';
import type {
  ExportJobDto,
  ExportPortfolioPdfRequest,
  ExportTransactionsExcelRequest,
  ExportPerformanceExcelRequest,
} from '../types/export.types';

/**
 * Export API Service
 * Connects to backend Export endpoints
 */
export const exportApi = {
  /**
   * Export client portfolio as PDF
   * POST /api/export/portfolio/{clientId}/pdf
   */
  exportPortfolioPdf: async (
    request: ExportPortfolioPdfRequest
  ): Promise<ExportJobDto> => {
    const response = await apiClient.post<ExportJobDto>(
      `/api/export/portfolio/${request.clientId}/pdf`,
      {
        includeTransactions: request.includeTransactions ?? false,
      }
    );
    return response.data;
  },

  /**
   * Export transactions as Excel
   * POST /api/export/transactions/excel
   */
  exportTransactionsExcel: async (
    request: ExportTransactionsExcelRequest
  ): Promise<ExportJobDto> => {
    const response = await apiClient.post<ExportJobDto>(
      '/api/export/transactions/excel',
      {
        clientId: request.clientId ?? null,
        fromDate: request.fromDate ?? null,
        toDate: request.toDate ?? null,
        transactionType: request.transactionType ?? null,
      }
    );
    return response.data;
  },

  /**
   * Export performance data as Excel
   * POST /api/export/performance/excel
   */
  exportPerformanceExcel: async (
    request: ExportPerformanceExcelRequest
  ): Promise<ExportJobDto> => {
    const response = await apiClient.post<ExportJobDto>(
      '/api/export/performance/excel',
      {
        clientId: request.clientId ?? null,
        fromDate: request.fromDate,
        toDate: request.toDate,
      }
    );
    return response.data;
  },

  /**
   * Get export job status
   * GET /api/export/jobs/{jobId}
   */
  getExportJobStatus: async (jobId: string): Promise<ExportJobDto> => {
    const response = await apiClient.get<ExportJobDto>(
      `/api/export/jobs/${jobId}`
    );
    return response.data;
  },

  /**
   * Download completed export file
   * GET /api/export/download/{jobId}
   * Returns a Blob that can be used to trigger file download
   */
  downloadExport: async (jobId: string): Promise<Blob> => {
    const response = await apiClient.get(`/api/export/download/${jobId}`, {
      responseType: 'blob',
    });
    return response.data;
  },

  /**
   * Helper function to trigger file download from blob
   */
  triggerFileDownload: (blob: Blob, filename: string): void => {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  },
};
