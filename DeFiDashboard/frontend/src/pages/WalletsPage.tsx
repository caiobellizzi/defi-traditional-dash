import { useState } from 'react';
import { useWallets, useAddWallet, useDeleteWallet } from '../hooks/useWallets';
import WalletForm from '../components/wallets/WalletForm';
import Button from '../components/ui/Button';
import Dialog from '../components/ui/Dialog';
import Table from '../components/ui/Table';
import { formatDate, truncateAddress } from '../lib/formatters';
import type { WalletDto, AddWalletCommand } from '../types/api';

const WalletsPage = () => {
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);

  // React Query hooks
  const { data: wallets, isLoading, error } = useWallets();
  const addMutation = useAddWallet();
  const deleteMutation = useDeleteWallet();

  // Handlers
  const handleAddWallet = async (formData: AddWalletCommand) => {
    try {
      await addMutation.mutateAsync(formData);
      setIsCreateDialogOpen(false);
      alert('Wallet added successfully!');
    } catch (error) {
      console.error('Failed to add wallet:', error);
      alert('Failed to add wallet. Please try again.');
    }
  };

  const handleDeleteWallet = async (wallet: WalletDto) => {
    if (
      !confirm(
        `Are you sure you want to delete wallet "${wallet.label}" (${truncateAddress(wallet.walletAddress)})?`
      )
    ) {
      return;
    }

    try {
      await deleteMutation.mutateAsync(wallet.id);
      alert('Wallet deleted successfully!');
    } catch (error) {
      console.error('Failed to delete wallet:', error);
      alert('Failed to delete wallet. Please try again.');
    }
  };

  // Table columns
  const columns = [
    {
      header: 'Label',
      accessor: 'label' as keyof WalletDto,
    },
    {
      header: 'Wallet Address',
      accessor: ((row: WalletDto) => (
        <span
          className="font-mono text-sm cursor-help"
          title={row.walletAddress}
        >
          {truncateAddress(row.walletAddress, 10, 8)}
        </span>
      )) as any,
    },
    {
      header: 'Supported Chains',
      accessor: ((row: WalletDto) => (
        <div className="flex flex-wrap gap-1">
          {row.supportedChains.map((chain) => (
            <span
              key={chain}
              className="px-2 py-0.5 text-xs font-medium bg-blue-100 text-blue-800 rounded"
            >
              {chain}
            </span>
          ))}
        </div>
      )) as any,
    },
    {
      header: 'Status',
      accessor: ((row: WalletDto) => (
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
      header: 'Created',
      accessor: ((row: WalletDto) => (
        <span className="text-sm text-gray-600">
          {formatDate(row.createdAt)}
        </span>
      )) as any,
    },
    {
      header: 'Actions',
      accessor: ((row: WalletDto) => (
        <div className="flex gap-2">
          <button
            onClick={(e) => {
              e.stopPropagation();
              handleDeleteWallet(row);
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
              <h1 className="text-3xl font-bold text-gray-900">
                Custody Wallets
              </h1>
              <p className="mt-1 text-sm text-gray-500">
                Manage blockchain wallets for client asset allocations
              </p>
            </div>
            <Button onClick={() => setIsCreateDialogOpen(true)}>
              Add Wallet
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
              Failed to load wallets. Please try again later.
            </p>
          </div>
        )}

        {/* Stats */}
        {wallets && (
          <div className="bg-white rounded-lg shadow p-6 mb-6">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div>
                <p className="text-sm text-gray-500">Total Wallets</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {wallets.length}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-500">Active Wallets</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {wallets.filter((w) => w.status === 'Active').length}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-500">Monitored Chains</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {
                    new Set(
                      wallets.flatMap((w) => w.supportedChains)
                    ).size
                  }
                </p>
              </div>
            </div>
          </div>
        )}

        {/* Table */}
        <div className="bg-white rounded-lg shadow overflow-hidden">
          <Table
            data={wallets || []}
            columns={columns}
            isLoading={isLoading}
            emptyMessage="No wallets found. Add your first wallet to get started."
          />
        </div>
      </div>

      {/* Add Wallet Dialog */}
      <Dialog
        isOpen={isCreateDialogOpen}
        onClose={() => setIsCreateDialogOpen(false)}
        title="Add New Wallet"
      >
        <WalletForm
          onSubmit={handleAddWallet}
          onCancel={() => setIsCreateDialogOpen(false)}
          isLoading={addMutation.isPending}
        />
      </Dialog>
    </div>
  );
};

export default WalletsPage;
