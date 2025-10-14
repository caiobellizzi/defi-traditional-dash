import { useState } from 'react';
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
      alert('Client created successfully!');
    } catch (error) {
      console.error('Failed to create client:', error);
      alert('Failed to create client. Please try again.');
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
      alert('Client updated successfully!');
    } catch (error) {
      console.error('Failed to update client:', error);
      alert('Failed to update client. Please try again.');
    }
  };

  const handleDeleteClient = async (client: ClientDto) => {
    if (!confirm(`Are you sure you want to delete "${client.name}"?`)) {
      return;
    }

    try {
      await deleteMutation.mutateAsync(client.id);
      alert('Client deleted successfully!');
    } catch (error) {
      console.error('Failed to delete client:', error);
      alert('Failed to delete client. Please try again.');
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
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <div className="flex justify-between items-center">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">Clients</h1>
              <p className="mt-1 text-sm text-gray-500">
                Manage your client portfolio and allocations
              </p>
            </div>
            <Button onClick={() => setIsCreateDialogOpen(true)}>
              Add Client
            </Button>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Error State */}
        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
            <p className="text-red-800">
              Failed to load clients. Please try again later.
            </p>
          </div>
        )}

        {/* Stats */}
        {data && (
          <div className="bg-white rounded-lg shadow p-6 mb-6">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div>
                <p className="text-sm text-gray-500">Total Clients</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {data.totalCount}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-500">Current Page</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {data.pageNumber} of {data.totalPages}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-500">Showing</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {data.items.length} clients
                </p>
              </div>
            </div>
          </div>
        )}

        {/* Table */}
        <div className="bg-white rounded-lg shadow overflow-hidden">
          <Table
            data={data?.items || []}
            columns={columns}
            isLoading={isLoading}
            emptyMessage="No clients found. Create your first client to get started."
          />

          {/* Pagination */}
          {data && data.totalPages > 1 && (
            <div className="px-6 py-4 border-t border-gray-200 flex items-center justify-between">
              <Button
                variant="ghost"
                onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                disabled={pageNumber === 1}
              >
                Previous
              </Button>
              <span className="text-sm text-gray-700">
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
    </div>
  );
};

export default ClientsPage;
