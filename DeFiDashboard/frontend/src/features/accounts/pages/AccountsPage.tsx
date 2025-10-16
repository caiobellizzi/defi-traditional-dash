import { PageLayout } from '@/shared/components/layout/PageLayout';
import Button from '@/shared/components/ui/Button';
import { DataTable, type Column } from '@/shared/components/DataTable';
import { useAccounts, usePluggyConnect } from '../hooks/useAccounts';
import type { Account } from '../types/account.types';
import toast from 'react-hot-toast';

export default function AccountsPage() {
  const { data: accounts, isLoading } = useAccounts();
  const pluggyConnect = usePluggyConnect();

  const handleConnectAccount = async () => {
    try {
      const result = await pluggyConnect.mutateAsync();
      toast.success('Connect token generated. Opening Pluggy Connect...');
      // TODO: Open Pluggy Connect widget with result.accessToken
      console.log('Pluggy Connect Token:', result.accessToken);
    } catch (error) {
      toast.error('Failed to generate connect token');
      console.error(error);
    }
  };

  const columns: Column<Account>[] = [
    {
      key: 'accountName',
      header: 'Account Name',
    },
    {
      key: 'institutionName',
      header: 'Institution',
    },
    {
      key: 'accountType',
      header: 'Type',
    },
    {
      key: 'balance',
      header: 'Balance',
      render: (account: Account) => `${account.currency} ${account.balance.toFixed(2)}`,
    },
    {
      key: 'status',
      header: 'Status',
      render: (account: Account) => (
        <span
          className={`px-2 py-1 rounded-full text-xs ${
            account.status === 'Active'
              ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
              : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
          }`}
        >
          {account.status}
        </span>
      ),
    },
  ];

  return (
    <PageLayout
      title="Traditional Accounts"
      subtitle="Manage bank and investment accounts via OpenFinance"
      actions={
        <Button onClick={handleConnectAccount} disabled={pluggyConnect.isPending}>
          {pluggyConnect.isPending ? 'Connecting...' : 'Connect Account'}
        </Button>
      }
    >
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
        <DataTable
          data={accounts || []}
          columns={columns}
          isLoading={isLoading}
          emptyMessage="No accounts connected yet. Click 'Connect Account' to get started."
        />
      </div>
    </PageLayout>
  );
}
