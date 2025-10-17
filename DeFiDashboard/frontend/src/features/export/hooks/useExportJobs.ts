import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { exportApi } from '../api/export.api';
import toast from 'react-hot-toast';
import type {
  ExportPortfolioPdfRequest,
  ExportTransactionsExcelRequest,
  ExportPerformanceExcelRequest,
  ExportJobDto,
} from '../types/export.types';

/**
 * React Query hooks for export operations
 */

/**
 * Export portfolio as PDF mutation
 */
export const useExportPortfolioPdf = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: ExportPortfolioPdfRequest) =>
      exportApi.exportPortfolioPdf(request),
    onSuccess: (job) => {
      toast.success(`Export job created: ${job.jobId}`);
      // Cache the job so it can be polled
      queryClient.setQueryData(['export-job', job.jobId], job);
    },
    onError: (error) => {
      toast.error(
        error instanceof Error
          ? error.message
          : 'Failed to create portfolio export'
      );
    },
  });
};

/**
 * Export transactions as Excel mutation
 */
export const useExportTransactionsExcel = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: ExportTransactionsExcelRequest) =>
      exportApi.exportTransactionsExcel(request),
    onSuccess: (job) => {
      toast.success(`Export job created: ${job.jobId}`);
      queryClient.setQueryData(['export-job', job.jobId], job);
    },
    onError: (error) => {
      toast.error(
        error instanceof Error
          ? error.message
          : 'Failed to create transactions export'
      );
    },
  });
};

/**
 * Export performance as Excel mutation
 */
export const useExportPerformanceExcel = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: ExportPerformanceExcelRequest) =>
      exportApi.exportPerformanceExcel(request),
    onSuccess: (job) => {
      toast.success(`Export job created: ${job.jobId}`);
      queryClient.setQueryData(['export-job', job.jobId], job);
    },
    onError: (error) => {
      toast.error(
        error instanceof Error
          ? error.message
          : 'Failed to create performance export'
      );
    },
  });
};

/**
 * Query export job status with polling
 * Polls every 2 seconds until job is completed or failed
 */
export const useExportJob = (jobId: string | null, enabled = true) => {
  return useQuery({
    queryKey: ['export-job', jobId],
    queryFn: () => exportApi.getExportJobStatus(jobId!),
    enabled: enabled && !!jobId,
    refetchInterval: (query) => {
      const data = query.state.data as ExportJobDto | undefined;
      // Poll every 2 seconds if job is pending or processing
      if (data?.status === 'Pending' || data?.status === 'Processing') {
        return 2000;
      }
      // Stop polling if completed or failed
      return false;
    },
    staleTime: 1000, // Consider data stale after 1 second to enable polling
  });
};

/**
 * Download export file mutation
 * Triggers file download when export is completed
 */
export const useDownloadExport = () => {
  return useMutation({
    mutationFn: async ({
      jobId,
      filename,
    }: {
      jobId: string;
      filename: string;
    }) => {
      const blob = await exportApi.downloadExport(jobId);
      exportApi.triggerFileDownload(blob, filename);
      return { jobId, filename };
    },
    onSuccess: (_, variables) => {
      toast.success(`Downloaded: ${variables.filename}`);
    },
    onError: (error) => {
      toast.error(
        error instanceof Error ? error.message : 'Failed to download export'
      );
    },
  });
};

/**
 * Hook to manage export job lifecycle
 * Combines job creation, polling, and download
 */
export const useExportJobLifecycle = () => {
  const exportPortfolio = useExportPortfolioPdf();
  const exportTransactions = useExportTransactionsExcel();
  const exportPerformance = useExportPerformanceExcel();
  const downloadExport = useDownloadExport();

  return {
    exportPortfolio,
    exportTransactions,
    exportPerformance,
    downloadExport,
    isExporting:
      exportPortfolio.isPending ||
      exportTransactions.isPending ||
      exportPerformance.isPending,
    isDownloading: downloadExport.isPending,
  };
};
