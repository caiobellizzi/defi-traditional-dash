import { useExportJob, useDownloadExport } from '../hooks/useExportJobs';
import type { ExportJobStatus as JobStatus } from '../types/export.types';

/**
 * Export job status component props
 */
interface ExportJobStatusProps {
  jobId: string;
  filename: string;
  onComplete?: () => void;
  autoDownload?: boolean;
}

/**
 * Status indicator component
 */
const StatusIndicator = ({ status }: { status: JobStatus }) => {
  const statusStyles: Record<JobStatus, { bg: string; text: string; label: string }> = {
    Pending: {
      bg: 'bg-gray-100',
      text: 'text-gray-700',
      label: 'Pending',
    },
    Processing: {
      bg: 'bg-blue-100',
      text: 'text-blue-700',
      label: 'Processing',
    },
    Completed: {
      bg: 'bg-green-100',
      text: 'text-green-700',
      label: 'Completed',
    },
    Failed: {
      bg: 'bg-red-100',
      text: 'text-red-700',
      label: 'Failed',
    },
  };

  const style = statusStyles[status];

  return (
    <span
      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${style.bg} ${style.text}`}
    >
      {status === 'Processing' && (
        <svg
          className="animate-spin -ml-1 mr-2 h-3 w-3"
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
        >
          <circle
            className="opacity-25"
            cx="12"
            cy="12"
            r="10"
            stroke="currentColor"
            strokeWidth="4"
          />
          <path
            className="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
          />
        </svg>
      )}
      {style.label}
    </span>
  );
};

/**
 * Export job status display component
 * Shows real-time status and provides download option when complete
 */
export const ExportJobStatus = ({
  jobId,
  filename,
  onComplete,
  autoDownload = true,
}: ExportJobStatusProps) => {
  const { data: job, isLoading, error } = useExportJob(jobId);
  const downloadExport = useDownloadExport();

  // Auto-download when job completes
  const handleAutoDownload = () => {
    if (job?.status === 'Completed' && autoDownload && job.fileUrl) {
      downloadExport.mutate({ jobId, filename });
      onComplete?.();
    }
  };

  // Trigger auto-download on completion
  if (job?.status === 'Completed' && autoDownload && !downloadExport.isPending) {
    handleAutoDownload();
  }

  // Handle manual download
  const handleManualDownload = () => {
    if (job?.status === 'Completed') {
      downloadExport.mutate({ jobId, filename });
      onComplete?.();
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center gap-2">
        <div className="animate-spin h-4 w-4 border-2 border-gray-300 border-t-blue-600 rounded-full" />
        <span className="text-sm text-gray-600">Loading job status...</span>
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-sm text-red-600">
        Error loading job status: {error instanceof Error ? error.message : 'Unknown error'}
      </div>
    );
  }

  if (!job) {
    return null;
  }

  return (
    <div className="flex items-center justify-between gap-4 p-3 bg-gray-50 rounded-lg">
      <div className="flex-1">
        <div className="flex items-center gap-2 mb-1">
          <StatusIndicator status={job.status} />
          <span className="text-sm font-medium text-gray-900">{filename}</span>
        </div>
        <div className="text-xs text-gray-500">
          Created: {new Date(job.createdAt).toLocaleString()}
          {job.completedAt && (
            <> • Completed: {new Date(job.completedAt).toLocaleString()}</>
          )}
        </div>
      </div>

      {job.status === 'Completed' && !autoDownload && (
        <button
          onClick={handleManualDownload}
          disabled={downloadExport.isPending}
          className="px-3 py-1.5 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {downloadExport.isPending ? 'Downloading...' : 'Download'}
        </button>
      )}

      {job.status === 'Failed' && (
        <span className="text-sm text-red-600">Export failed</span>
      )}
    </div>
  );
};
