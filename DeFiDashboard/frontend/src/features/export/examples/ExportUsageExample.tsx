/**
 * Export Feature Usage Examples
 * Demonstrates how to integrate export functionality into your pages
 */

import { useState } from 'react';
import { ExportButton, ExportDialog } from '@/features/export';

/**
 * Example 1: Simple Export Buttons
 * Use ExportButton for quick, one-click exports
 */
export const SimpleExportExample = ({ clientId }: { clientId: string }) => {
  return (
    <div className="space-y-3">
      <h3 className="text-lg font-semibold">Export Options</h3>

      {/* Export portfolio PDF with transactions */}
      <ExportButton
        type="portfolio-pdf"
        clientId={clientId}
        includeTransactions={true}
        variant="primary"
        size="md"
      >
        Export Portfolio with Transactions
      </ExportButton>

      {/* Export portfolio PDF without transactions */}
      <ExportButton
        type="portfolio-pdf"
        clientId={clientId}
        includeTransactions={false}
        variant="secondary"
        size="sm"
      >
        Export Portfolio Summary
      </ExportButton>
    </div>
  );
};

/**
 * Example 2: Export with Dialog
 * Use ExportDialog for exports that need user input/filters
 */
export const DialogExportExample = ({ clientId }: { clientId: string }) => {
  const [showTransactionsDialog, setShowTransactionsDialog] = useState(false);
  const [showPerformanceDialog, setShowPerformanceDialog] = useState(false);

  return (
    <div className="space-y-3">
      <h3 className="text-lg font-semibold">Advanced Export Options</h3>

      {/* Button to open transactions export dialog */}
      <button
        onClick={() => setShowTransactionsDialog(true)}
        className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
      >
        Export Transactions with Filters
      </button>

      {/* Button to open performance export dialog */}
      <button
        onClick={() => setShowPerformanceDialog(true)}
        className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
      >
        Export Performance Data
      </button>

      {/* Transactions export dialog */}
      <ExportDialog
        isOpen={showTransactionsDialog}
        onClose={() => setShowTransactionsDialog(false)}
        type="transactions-excel"
        clientId={clientId}
      />

      {/* Performance export dialog */}
      <ExportDialog
        isOpen={showPerformanceDialog}
        onClose={() => setShowPerformanceDialog(false)}
        type="performance-excel"
        clientId={clientId}
        defaultFromDate="2024-01-01"
        defaultToDate={new Date().toISOString().split('T')[0]}
      />
    </div>
  );
};

/**
 * Example 3: Export All Transactions (No Client Filter)
 * Export all transactions across all clients
 */
export const ExportAllTransactionsExample = () => {
  const [showDialog, setShowDialog] = useState(false);

  return (
    <div className="space-y-3">
      <h3 className="text-lg font-semibold">Export All Data</h3>

      {/* Quick export all transactions (last 30 days) */}
      <ExportButton
        type="transactions-excel"
        fromDate={
          new Date(Date.now() - 30 * 24 * 60 * 60 * 1000)
            .toISOString()
            .split('T')[0]
        }
        toDate={new Date().toISOString().split('T')[0]}
        variant="primary"
      >
        Export Last 30 Days
      </ExportButton>

      {/* Export with custom filters */}
      <button
        onClick={() => setShowDialog(true)}
        className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
      >
        Export with Custom Filters
      </button>

      <ExportDialog
        isOpen={showDialog}
        onClose={() => setShowDialog(false)}
        type="transactions-excel"
        // No clientId = export all clients
      />
    </div>
  );
};

/**
 * Example 4: Integration in a Client Detail Page
 * Shows how to add export buttons to an existing page
 */
export const ClientDetailExportSection = ({ clientId }: { clientId: string }) => {
  const [showExportMenu, setShowExportMenu] = useState(false);

  return (
    <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-xl font-bold">Export Client Data</h2>
        <button
          onClick={() => setShowExportMenu(!showExportMenu)}
          className="text-blue-600 hover:text-blue-700"
        >
          {showExportMenu ? 'Hide' : 'Show'} Export Options
        </button>
      </div>

      {showExportMenu && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          {/* Portfolio exports */}
          <div className="space-y-2">
            <h3 className="font-semibold text-sm text-gray-600 dark:text-gray-400">
              Portfolio
            </h3>
            <ExportButton
              type="portfolio-pdf"
              clientId={clientId}
              includeTransactions={false}
              size="sm"
              className="w-full"
            >
              Export Portfolio Summary
            </ExportButton>
            <ExportButton
              type="portfolio-pdf"
              clientId={clientId}
              includeTransactions={true}
              size="sm"
              className="w-full"
            >
              Export Portfolio + Transactions
            </ExportButton>
          </div>

          {/* Transaction exports */}
          <div className="space-y-2">
            <h3 className="font-semibold text-sm text-gray-600 dark:text-gray-400">
              Transactions & Performance
            </h3>
            <ExportButton
              type="transactions-excel"
              clientId={clientId}
              fromDate="2024-01-01"
              toDate={new Date().toISOString().split('T')[0]}
              size="sm"
              className="w-full"
            >
              Export Transactions (YTD)
            </ExportButton>
            <ExportButton
              type="performance-excel"
              clientId={clientId}
              fromDate="2024-01-01"
              toDate={new Date().toISOString().split('T')[0]}
              size="sm"
              className="w-full"
            >
              Export Performance (YTD)
            </ExportButton>
          </div>
        </div>
      )}
    </div>
  );
};

/**
 * Example 5: Dropdown Menu Integration
 * Shows how to add exports to a dropdown/action menu
 */
export const ExportDropdownMenu = ({ clientId }: { clientId: string }) => {
  const [isOpen, setIsOpen] = useState(false);
  const [dialogType, setDialogType] = useState<'portfolio' | 'transactions' | 'performance' | null>(null);

  const openDialog = (type: typeof dialogType) => {
    setDialogType(type);
    setIsOpen(false);
  };

  return (
    <>
      {/* Dropdown trigger */}
      <div className="relative">
        <button
          onClick={() => setIsOpen(!isOpen)}
          className="px-4 py-2 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 flex items-center gap-2"
        >
          <span>Export</span>
          <svg
            className={`w-4 h-4 transition-transform ${isOpen ? 'rotate-180' : ''}`}
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
          </svg>
        </button>

        {/* Dropdown menu */}
        {isOpen && (
          <div className="absolute right-0 mt-2 w-56 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg shadow-lg z-10">
            <div className="py-1">
              <button
                onClick={() => openDialog('portfolio')}
                className="block w-full text-left px-4 py-2 text-sm hover:bg-gray-100 dark:hover:bg-gray-700"
              >
                Export Portfolio PDF
              </button>
              <button
                onClick={() => openDialog('transactions')}
                className="block w-full text-left px-4 py-2 text-sm hover:bg-gray-100 dark:hover:bg-gray-700"
              >
                Export Transactions Excel
              </button>
              <button
                onClick={() => openDialog('performance')}
                className="block w-full text-left px-4 py-2 text-sm hover:bg-gray-100 dark:hover:bg-gray-700"
              >
                Export Performance Excel
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Dialogs */}
      {dialogType === 'portfolio' && (
        <ExportDialog
          isOpen={true}
          onClose={() => setDialogType(null)}
          type="portfolio-pdf"
          clientId={clientId}
        />
      )}
      {dialogType === 'transactions' && (
        <ExportDialog
          isOpen={true}
          onClose={() => setDialogType(null)}
          type="transactions-excel"
          clientId={clientId}
        />
      )}
      {dialogType === 'performance' && (
        <ExportDialog
          isOpen={true}
          onClose={() => setDialogType(null)}
          type="performance-excel"
          clientId={clientId}
        />
      )}
    </>
  );
};
