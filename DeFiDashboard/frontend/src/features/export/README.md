# Export Feature

Complete export functionality for DeFi Dashboard with job status tracking and file downloads.

## Features

- Export client portfolios to PDF
- Export transactions to Excel
- Export performance data to Excel
- Real-time job status polling
- Automatic file downloads
- Error handling with toast notifications

## Quick Start

### Simple Export Button

```tsx
import { ExportButton } from '@/features/export';

// Export client portfolio
<ExportButton
  type="portfolio-pdf"
  clientId="client-123"
  includeTransactions={true}
/>

// Export all transactions
<ExportButton
  type="transactions-excel"
  fromDate="2024-01-01"
  toDate="2024-12-31"
/>

// Export performance data
<ExportButton
  type="performance-excel"
  clientId="client-123"
  fromDate="2024-01-01"
  toDate="2024-12-31"
/>
```

### Export Dialog with Filters

```tsx
import { useState } from 'react';
import { ExportDialog } from '@/features/export';

function MyComponent() {
  const [showDialog, setShowDialog] = useState(false);

  return (
    <>
      <button onClick={() => setShowDialog(true)}>
        Export Transactions
      </button>

      <ExportDialog
        isOpen={showDialog}
        onClose={() => setShowDialog(false)}
        type="transactions-excel"
        clientId="client-123"
        defaultFromDate="2024-01-01"
        defaultToDate="2024-12-31"
      />
    </>
  );
}
```

### Custom Implementation with Hooks

```tsx
import {
  useExportPortfolioPdf,
  useExportJob,
  useDownloadExport
} from '@/features/export';

function CustomExport() {
  const exportPortfolio = useExportPortfolioPdf();
  const [jobId, setJobId] = useState<string | null>(null);
  const { data: job } = useExportJob(jobId);
  const downloadExport = useDownloadExport();

  const handleExport = async () => {
    const result = await exportPortfolio.mutateAsync({
      clientId: 'client-123',
      includeTransactions: true,
    });
    setJobId(result.jobId);
  };

  const handleDownload = () => {
    if (job?.status === 'Completed') {
      downloadExport.mutate({
        jobId: job.jobId,
        filename: 'portfolio.pdf',
      });
    }
  };

  return (
    <div>
      <button onClick={handleExport}>Export</button>
      {job && (
        <div>
          Status: {job.status}
          {job.status === 'Completed' && (
            <button onClick={handleDownload}>Download</button>
          )}
        </div>
      )}
    </div>
  );
}
```

## Components

### ExportButton

Simple button component that handles the complete export workflow.

**Props:**
- `type`: Export type ('portfolio-pdf' | 'transactions-excel' | 'performance-excel')
- `clientId?`: Client ID (required for portfolio)
- `includeTransactions?`: Include transactions in portfolio (default: false)
- `fromDate?`: Start date filter (ISO string)
- `toDate?`: End date filter (ISO string)
- `transactionType?`: Transaction type filter
- `variant?`: Button variant ('primary' | 'secondary' | 'success')
- `size?`: Button size ('sm' | 'md' | 'lg')
- `className?`: Additional CSS classes
- `children?`: Custom button text

### ExportJobStatus

Display real-time export job status with progress indicator.

**Props:**
- `jobId`: Export job ID
- `filename`: Filename for download
- `onComplete?`: Callback when job completes
- `autoDownload?`: Auto-download when complete (default: true)

### ExportDialog

Dialog with export options and filters.

**Props:**
- `isOpen`: Dialog open state
- `onClose`: Close handler
- `type`: Export type
- `clientId?`: Pre-filled client ID
- `defaultFromDate?`: Default start date
- `defaultToDate?`: Default end date

## Hooks

### useExportPortfolioPdf()

Mutation hook for exporting portfolio to PDF.

```tsx
const exportPortfolio = useExportPortfolioPdf();

exportPortfolio.mutate({
  clientId: 'client-123',
  includeTransactions: true,
});
```

### useExportTransactionsExcel()

Mutation hook for exporting transactions to Excel.

```tsx
const exportTransactions = useExportTransactionsExcel();

exportTransactions.mutate({
  clientId: 'client-123',
  fromDate: '2024-01-01',
  toDate: '2024-12-31',
  transactionType: 'Deposit',
});
```

### useExportPerformanceExcel()

Mutation hook for exporting performance data to Excel.

```tsx
const exportPerformance = useExportPerformanceExcel();

exportPerformance.mutate({
  clientId: 'client-123',
  fromDate: '2024-01-01',
  toDate: '2024-12-31',
});
```

### useExportJob(jobId, enabled?)

Query hook with polling for job status.

```tsx
const { data: job, isLoading } = useExportJob(jobId);

// Polls every 2 seconds until job is completed or failed
```

### useDownloadExport()

Mutation hook for downloading completed exports.

```tsx
const downloadExport = useDownloadExport();

downloadExport.mutate({
  jobId: 'job-123',
  filename: 'portfolio.pdf',
});
```

## API Functions

All API functions are available via `exportApi`:

```tsx
import { exportApi } from '@/features/export';

// Export portfolio
const job = await exportApi.exportPortfolioPdf({
  clientId: 'client-123',
  includeTransactions: true,
});

// Check job status
const status = await exportApi.getExportJobStatus(job.jobId);

// Download file
const blob = await exportApi.downloadExport(job.jobId);
exportApi.triggerFileDownload(blob, 'portfolio.pdf');
```

## Job Status Flow

1. **Trigger Export** → Job created with status 'Pending'
2. **Backend Processing** → Status changes to 'Processing'
3. **Completion** → Status changes to 'Completed' or 'Failed'
4. **Download** → File URL available, trigger download

## Error Handling

All hooks include automatic error handling with toast notifications:

- Export creation errors
- Job status polling errors
- Download errors

## Architecture

Follows Vertical Slice Architecture:

```
features/export/
├── api/export.api.ts          # API client functions
├── hooks/useExportJobs.ts     # TanStack Query hooks
├── components/
│   ├── ExportButton.tsx       # Simple export button
│   ├── ExportJobStatus.tsx    # Job status display
│   └── ExportDialog.tsx       # Export configuration dialog
├── types/export.types.ts      # TypeScript types
└── index.ts                   # Barrel exports
```

## Backend Integration

Connects to these backend endpoints:

- `POST /api/export/portfolio/{clientId}/pdf`
- `POST /api/export/transactions/excel`
- `POST /api/export/performance/excel`
- `GET /api/export/jobs/{jobId}`
- `GET /api/export/download/{jobId}`

## Notes

- Job polling stops automatically when status is 'Completed' or 'Failed'
- Files are downloaded as blobs and trigger browser download
- Toast notifications provide user feedback for all operations
- All components support dark mode
