import { useState } from 'react';
import { useCreateAllocation } from '@/features/allocations/hooks/useAllocations';
import AllocationForm from '@/features/allocations/components/AllocationForm';
import Button from '@/shared/components/ui/Button';
import Dialog from '@/shared/components/ui/Dialog';
import { PageLayout } from '@/shared/components/layout/PageLayout';
import type { CreateAllocationCommand } from '@/shared/types/api.types';

const AllocationsPage = () => {
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);

  // React Query hooks
  const createMutation = useCreateAllocation();

  // Handlers
  const handleCreateAllocation = async (formData: CreateAllocationCommand) => {
    try {
      await createMutation.mutateAsync(formData);
      setIsCreateDialogOpen(false);
      alert('Allocation created successfully!');
    } catch (error) {
      console.error('Failed to create allocation:', error);
      alert('Failed to create allocation. Please try again.');
    }
  };

  return (
    <PageLayout>
      {/* Header */}
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            Client Allocations
          </h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage how clients are allocated to custody assets
          </p>
        </div>
        <Button onClick={() => setIsCreateDialogOpen(true)}>
          Create Allocation
        </Button>
      </div>

      {/* Content */}
      <div className="space-y-6">
        {/* Info Card */}
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-6 mb-6">
          <div className="flex">
            <div className="flex-shrink-0">
              <svg
                className="h-5 w-5 text-blue-400"
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 20 20"
                fill="currentColor"
              >
                <path
                  fillRule="evenodd"
                  d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z"
                  clipRule="evenodd"
                />
              </svg>
            </div>
            <div className="ml-3">
              <h3 className="text-sm font-medium text-blue-800">
                How Allocations Work
              </h3>
              <div className="mt-2 text-sm text-blue-700">
                <p>
                  Allocations define how custody assets (wallets and traditional
                  accounts) are distributed among your clients. You can allocate
                  assets by:
                </p>
                <ul className="list-disc list-inside mt-2 space-y-1">
                  <li>
                    <strong>Percentage</strong>: Client receives a percentage of
                    the total asset value
                  </li>
                  <li>
                    <strong>Fixed Amount</strong>: Client receives a fixed USD
                    amount from the asset
                  </li>
                </ul>
                <p className="mt-3">
                  To view a client's allocations, go to the{' '}
                  <a href="/clients" className="font-medium underline">
                    Clients page
                  </a>{' '}
                  and view their portfolio.
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* Quick Actions */}
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">
            Quick Actions
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <a
              href="/clients"
              className="block p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
            >
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <svg
                    className="h-6 w-6 text-blue-600"
                    xmlns="http://www.w3.org/2000/svg"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"
                    />
                  </svg>
                </div>
                <div className="ml-3">
                  <h3 className="text-sm font-medium text-gray-900">
                    View Clients
                  </h3>
                  <p className="text-xs text-gray-500">
                    See all clients and their portfolios
                  </p>
                </div>
              </div>
            </a>

            <a
              href="/wallets"
              className="block p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
            >
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <svg
                    className="h-6 w-6 text-purple-600"
                    xmlns="http://www.w3.org/2000/svg"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z"
                    />
                  </svg>
                </div>
                <div className="ml-3">
                  <h3 className="text-sm font-medium text-gray-900">
                    Manage Wallets
                  </h3>
                  <p className="text-xs text-gray-500">
                    Add and manage custody wallets
                  </p>
                </div>
              </div>
            </a>

            <button
              onClick={() => setIsCreateDialogOpen(true)}
              className="block p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-left w-full"
            >
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <svg
                    className="h-6 w-6 text-green-600"
                    xmlns="http://www.w3.org/2000/svg"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M12 6v6m0 0v6m0-6h6m-6 0H6"
                    />
                  </svg>
                </div>
                <div className="ml-3">
                  <h3 className="text-sm font-medium text-gray-900">
                    Create Allocation
                  </h3>
                  <p className="text-xs text-gray-500">
                    Allocate assets to a client
                  </p>
                </div>
              </div>
            </button>
          </div>
        </div>
      </div>

      {/* Create Allocation Dialog */}
      <Dialog
        isOpen={isCreateDialogOpen}
        onClose={() => setIsCreateDialogOpen(false)}
        title="Create New Allocation"
      >
        <AllocationForm
          onSubmit={handleCreateAllocation}
          onCancel={() => setIsCreateDialogOpen(false)}
          isLoading={createMutation.isPending}
        />
      </Dialog>
    </PageLayout>
  );
};

export default AllocationsPage;
