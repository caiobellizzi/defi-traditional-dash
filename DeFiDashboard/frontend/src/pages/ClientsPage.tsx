import { useState } from 'react';
import toast from 'react-hot-toast';
import {
  useClients,
  useCreateClient,
  useUpdateClient,
  useDeleteClient,
} from '../hooks/useClients';
import ClientForm from '../components/clients/ClientForm';
import Button from '../components/ui/Button';
import Dialog from '../components/ui/Dialog';
import Table from '../components/ui/Table';
import { PageLayout } from '../components/layout/PageLayout';
import type { ClientDto, CreateClientCommand } from '../types/api';

const ClientsPage = () => {
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const [selectedClient, setSelectedClient] = useState<ClientDto | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const pageSize = 10;

  // React Query hooks
  const { data, isLoading, error } = useClients({ pageNumber, pageSize });
  const createMutation = useCreateClient();
  const updateMutation = useUpdateClient();
  const deleteMutation = useDeleteClient();

  // Handlers
  const handleCreateClient = async (formData: CreateClientCommand) => {
    try {
      await createMutation.mutateAsync(formData);
      setIsCreateDialogOpen(false);
      toast.success('Client created successfully!');
    } catch (error) {
      console.error('Failed to create client:', error);
      toast.error('Failed to create client. Please try again.');
    }
  };

  const handleUpdateClient = async (formData: CreateClientCommand) => {
    if (!selectedClient) return;

    try {
      await updateMutation.mutateAsync({
        id: selectedClient.id,
        data: formData,
      });
      setIsEditDialogOpen(false);
      setSelectedClient(null);
      toast.success('Client updated successfully!');
    } catch (error) {
      console.error('Failed to update client:', error);
      toast.error('Failed to update client. Please try again.');
    }
  };

  const handleDeleteClient = async (client: ClientDto) => {
    if (!confirm(`Are you sure you want to delete "${client.name}"?`)) {
      return;
    }

    try {
      await deleteMutation.mutateAsync(client.id);
      toast.success('Client deleted successfully!');
    } catch (error) {
      console.error('Failed to delete client:', error);
      toast.error('Failed to delete client. Please try again.');
    }
  };

  const handleEditClick = (client: ClientDto) => {
    setSelectedClient(client);
    setIsEditDialogOpen(true);
  };

  // Table columns
  const columns = [
    {
      header: 'Name',
      accessor: 'name' as keyof ClientDto,
    },
    {
      header: 'Email',
      accessor: 'email' as keyof ClientDto,
    },
    {
      header: 'Document',
      accessor: 'document' as keyof ClientDto,
    },
    {
      header: 'Phone',
      accessor: 'phoneNumber' as keyof ClientDto,
    },
    {
      header: 'Status',
      accessor: ((row: ClientDto) => (
        <span
          className={`px-2 py-1 text-xs font-semibold rounded-full ${
            row.status === 'Active'
              ? 'bg-green-100 text-green-800'
              : 'bg-gray-100 text-gray-800'
          }`}
        >
          {row.status}
        </span>
      )) as any,
    },
    {
      header: 'Actions',
      accessor: ((row: ClientDto) => (
        <div className="flex gap-2">
          <button
            onClick={(e) => {
              e.stopPropagation();
              handleEditClick(row);
            }}
            className="text-blue-600 hover:text-blue-800 font-medium"
          >
            Edit
          </button>
          <button
            onClick={(e) => {
              e.stopPropagation();
              handleDeleteClient(row);
            }}
            className="text-red-600 hover:text-red-800 font-medium"
          >
            Delete
          </button>
        </div>
      )) as any,
    },
  ];

  return (
    <PageLayout>
      {/* Header */}
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Clients</h1>
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
            Manage your client portfolio and allocations
          </p>
        </div>
        <Button onClick={() => setIsCreateDialogOpen(true)}>
          Add Client
        </Button>
      </div>

      {/* Content */}
      <div className="space-y-6">
        {/* Error State */}
        {error && (
          <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4 mb-6">
            <p className="text-red-800 dark:text-red-300">
              Failed to load clients. Please try again later.
            </p>
          </div>
        )}

        {/* Stats */}
        {data && (
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-card dark:shadow-card-dark p-6 mb-6 border border-gray-200 dark:border-gray-700 transition-colors">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div>
                <p className="text-sm text-gray-500 dark:text-gray-400">Total Clients</p>
                <p className="text-2xl font-semibold text-gray-900 dark:text-white">
                  {data.totalCount}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-500 dark:text-gray-400">Current Page</p>
                <p className="text-2xl font-semibold text-gray-900 dark:text-white">
                  {data.pageNumber} of {data.totalPages}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-500 dark:text-gray-400">Showing</p>
                <p className="text-2xl font-semibold text-gray-900 dark:text-white">
                  {data.items.length} clients
                </p>
              </div>
            </div>
          </div>
        )}

        {/* Table */}
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow-card dark:shadow-card-dark overflow-hidden border border-gray-200 dark:border-gray-700 transition-colors">
          <Table
            data={data?.items || []}
            columns={columns}
            isLoading={isLoading}
            emptyMessage="No clients found. Create your first client to get started."
          />

          {/* Pagination */}
          {data && data.totalPages > 1 && (
            <div className="px-6 py-4 border-t border-gray-200 dark:border-gray-700 flex items-center justify-between">
              <Button
                variant="ghost"
                onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                disabled={pageNumber === 1}
              >
                Previous
              </Button>
              <span className="text-sm text-gray-700 dark:text-gray-300">
                Page {pageNumber} of {data.totalPages}
              </span>
              <Button
                variant="ghost"
                onClick={() =>
                  setPageNumber((p) => Math.min(data.totalPages, p + 1))
                }
                disabled={pageNumber === data.totalPages}
              >
                Next
              </Button>
            </div>
          )}
        </div>
      </div>

      {/* Create Client Dialog */}
      <Dialog
        isOpen={isCreateDialogOpen}
        onClose={() => setIsCreateDialogOpen(false)}
        title="Create New Client"
      >
        <ClientForm
          onSubmit={handleCreateClient}
          onCancel={() => setIsCreateDialogOpen(false)}
          isLoading={createMutation.isPending}
        />
      </Dialog>

      {/* Edit Client Dialog */}
      <Dialog
        isOpen={isEditDialogOpen}
        onClose={() => {
          setIsEditDialogOpen(false);
          setSelectedClient(null);
        }}
        title="Edit Client"
      >
        {selectedClient && (
          <ClientForm
            onSubmit={handleUpdateClient}
            onCancel={() => {
              setIsEditDialogOpen(false);
              setSelectedClient(null);
            }}
            initialData={selectedClient}
            isLoading={updateMutation.isPending}
          />
        )}
      </Dialog>
    </PageLayout>
  );
};

export default ClientsPage;
