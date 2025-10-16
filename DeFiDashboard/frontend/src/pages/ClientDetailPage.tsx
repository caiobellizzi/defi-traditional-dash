import { useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { PageLayout } from '../components/layout/PageLayout';
import { clientsApi } from '../api/clients';
import { portfolioApi } from '../api/portfolio';
import { formatCurrency, formatDate } from '../lib/formatters';

/**
 * Client Detail Page - Shows individual client information and portfolio breakdown
 */
export default function ClientDetailPage() {
  const { clientId } = useParams<{ clientId: string }>();
  const navigate = useNavigate();
  const [isDeleting, setIsDeleting] = useState(false);

  // Fetch client data
  const {
    data: client,
    isLoading: clientLoading,
    error: clientError,
  } = useQuery({
    queryKey: ['client', clientId],
    queryFn: () => clientsApi.getById(clientId!),
    enabled: !!clientId,
  });

  // Fetch portfolio data
  const {
    data: portfolio,
    isLoading: portfolioLoading,
    error: portfolioError,
  } = useQuery({
    queryKey: ['portfolio', clientId],
    queryFn: () => portfolioApi.getClientPortfolio(clientId!),
    enabled: !!clientId,
  });

  const handleDelete = async () => {
    if (!clientId) return;

    if (!confirm('Are you sure you want to delete this client? This action cannot be undone.')) {
      return;
    }

    setIsDeleting(true);
    try {
      await clientsApi.delete(clientId);
      toast.success('Client deleted successfully');
      navigate('/clients');
    } catch (error) {
      toast.error('Failed to delete client');
      console.error('Delete error:', error);
    } finally {
      setIsDeleting(false);
    }
  };

  const isLoading = clientLoading || portfolioLoading;

  if (clientError || portfolioError) {
    return (
      <PageLayout>
        <div className="text-center py-12">
          <p className="text-red-600 dark:text-red-400">
            Error loading client data. Please try again.
          </p>
          <Link
            to="/clients"
            className="text-blue-600 dark:text-blue-400 hover:underline mt-4 inline-block"
          >
            Back to Clients
          </Link>
        </div>
      </PageLayout>
    );
  }

  return (
    <PageLayout>
      {/* Header */}
      <div className="mb-8">
        <div className="flex items-center justify-between">
          <div>
            <Link
              to="/clients"
              className="text-sm text-blue-600 dark:text-blue-400 hover:underline mb-2 inline-block"
            >
              ← Back to Clients
            </Link>
            <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
              {isLoading ? 'Loading...' : client?.name || 'Unknown Client'}
            </h1>
            <p className="text-gray-500 dark:text-gray-400 mt-1">
              Client details and portfolio breakdown
            </p>
          </div>
          <button
            onClick={handleDelete}
            disabled={isDeleting}
            className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50 transition-colors"
          >
            {isDeleting ? 'Deleting...' : 'Delete Client'}
          </button>
        </div>
      </div>

      {isLoading ? (
        <div className="text-center py-12">
          <div className="animate-pulse text-gray-400 dark:text-gray-500">
            Loading client data...
          </div>
        </div>
      ) : (
        <>
          {/* Client Information */}
          <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 shadow-card dark:shadow-card-dark mb-6 transition-colors">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
              Client Information
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <p className="text-sm text-gray-500 dark:text-gray-400">Email</p>
                <p className="text-gray-900 dark:text-white mt-1">
                  {client?.email || 'N/A'}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-500 dark:text-gray-400">Phone</p>
                <p className="text-gray-900 dark:text-white mt-1">
                  {client?.phoneNumber || 'N/A'}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-500 dark:text-gray-400">Document</p>
                <p className="text-gray-900 dark:text-white mt-1">
                  {client?.document || 'N/A'}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-500 dark:text-gray-400">Status</p>
                <p className="text-gray-900 dark:text-white mt-1">
                  <span
                    className={`px-2 py-1 rounded-full text-xs ${
                      client?.status === 'Active'
                        ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400'
                        : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
                    }`}
                  >
                    {client?.status || 'Unknown'}
                  </span>
                </p>
              </div>
              <div className="md:col-span-2">
                <p className="text-sm text-gray-500 dark:text-gray-400">Notes</p>
                <p className="text-gray-900 dark:text-white mt-1">
                  {client?.notes || 'No notes'}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-500 dark:text-gray-400">Created</p>
                <p className="text-gray-900 dark:text-white mt-1">
                  {client?.createdAt ? formatDate(client.createdAt) : 'N/A'}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-500 dark:text-gray-400">Last Updated</p>
                <p className="text-gray-900 dark:text-white mt-1">
                  {client?.updatedAt ? formatDate(client.updatedAt) : 'N/A'}
                </p>
              </div>
            </div>
          </div>

          {/* Portfolio Summary */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
            <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 shadow-card dark:shadow-card-dark transition-colors">
              <p className="text-sm text-gray-500 dark:text-gray-400">Total Portfolio Value</p>
              <p className="text-3xl font-bold text-gray-900 dark:text-white mt-2">
                {formatCurrency(portfolio?.totalValueUsd || 0)}
              </p>
            </div>

            <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 shadow-card dark:shadow-card-dark transition-colors">
              <p className="text-sm text-gray-500 dark:text-gray-400">Crypto Assets</p>
              <p className="text-3xl font-bold text-blue-600 dark:text-blue-400 mt-2">
                {formatCurrency(portfolio?.cryptoValueUsd || 0)}
              </p>
              <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                {portfolio?.totalValueUsd
                  ? `${((portfolio.cryptoValueUsd / portfolio.totalValueUsd) * 100).toFixed(1)}%`
                  : '0%'}{' '}
                of total
              </p>
            </div>

            <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 shadow-card dark:shadow-card-dark transition-colors">
              <p className="text-sm text-gray-500 dark:text-gray-400">Traditional Assets</p>
              <p className="text-3xl font-bold text-green-600 dark:text-green-400 mt-2">
                {formatCurrency(portfolio?.traditionalValueUsd || 0)}
              </p>
              <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                {portfolio?.totalValueUsd
                  ? `${((portfolio.traditionalValueUsd / portfolio.totalValueUsd) * 100).toFixed(1)}%`
                  : '0%'}{' '}
                of total
              </p>
            </div>
          </div>

          {/* Asset Breakdown */}
          <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 shadow-card dark:shadow-card-dark overflow-hidden transition-colors">
            <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
                Asset Breakdown
              </h2>
              <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                Last calculated: {portfolio?.calculatedAt ? formatDate(portfolio.calculatedAt) : 'N/A'}
              </p>
            </div>

            {!portfolio?.assets || portfolio.assets.length === 0 ? (
              <div className="p-12 text-center">
                <p className="text-gray-500 dark:text-gray-400">
                  No assets allocated to this client.
                </p>
                <Link
                  to="/allocations"
                  className="text-blue-600 dark:text-blue-400 hover:underline mt-2 inline-block"
                >
                  Create Allocation
                </Link>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-gray-50 dark:bg-gray-900/50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Asset
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Type
                      </th>
                      <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Allocation
                      </th>
                      <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Total Asset Value
                      </th>
                      <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Client Value
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Tokens
                      </th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                    {portfolio.assets.map((asset, idx) => (
                      <tr
                        key={idx}
                        className="hover:bg-gray-50 dark:hover:bg-gray-900/30 transition-colors"
                      >
                        <td className="px-6 py-4">
                          <div className="text-sm font-medium text-gray-900 dark:text-white">
                            {asset.assetIdentifier}
                          </div>
                          <div className="text-xs text-gray-500 dark:text-gray-400">
                            ID: {asset.assetId.substring(0, 8)}...
                          </div>
                        </td>
                        <td className="px-6 py-4">
                          <span
                            className={`px-2 py-1 rounded-full text-xs ${
                              asset.assetType === 'Wallet'
                                ? 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400'
                                : 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400'
                            }`}
                          >
                            {asset.assetType}
                          </span>
                        </td>
                        <td className="px-6 py-4 text-right">
                          <div className="text-sm text-gray-900 dark:text-white">
                            {asset.allocationValue}
                            {asset.allocationType === 'Percentage' ? '%' : ' USD'}
                          </div>
                          <div className="text-xs text-gray-500 dark:text-gray-400">
                            {asset.allocationType}
                          </div>
                        </td>
                        <td className="px-6 py-4 text-right text-sm text-gray-900 dark:text-white">
                          {formatCurrency(asset.totalAssetValueUsd)}
                        </td>
                        <td className="px-6 py-4 text-right">
                          <div className="text-sm font-semibold text-gray-900 dark:text-white">
                            {formatCurrency(asset.clientAllocatedValueUsd)}
                          </div>
                          <div className="text-xs text-gray-500 dark:text-gray-400">
                            {asset.totalAssetValueUsd > 0
                              ? `${((asset.clientAllocatedValueUsd / asset.totalAssetValueUsd) * 100).toFixed(1)}%`
                              : '0%'}
                          </div>
                        </td>
                        <td className="px-6 py-4">
                          {asset.tokens.length > 0 ? (
                            <div className="space-y-1">
                              {asset.tokens.map((token, tokenIdx) => (
                                <div
                                  key={tokenIdx}
                                  className="text-xs text-gray-600 dark:text-gray-300"
                                >
                                  {token.tokenSymbol}: {token.balance.toFixed(4)}
                                  {token.balanceUsd && (
                                    <span className="text-gray-500 dark:text-gray-400 ml-1">
                                      ({formatCurrency(token.balanceUsd)})
                                    </span>
                                  )}
                                </div>
                              ))}
                            </div>
                          ) : (
                            <span className="text-xs text-gray-400 dark:text-gray-500">
                              No tokens
                            </span>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </>
      )}
    </PageLayout>
  );
}
