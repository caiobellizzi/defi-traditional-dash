import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { PageLayout } from '../components/layout/PageLayout';
import { useClients } from '../hooks/useClients';
import { portfolioApi } from '../api/portfolio';
import { formatCurrency, formatDate } from '../lib/formatters';
import type { ClientPortfolioDto } from '../types/portfolio';

/**
 * Portfolio page - Overview of all client portfolios
 */
export default function PortfolioPage() {
  const { data: clientsData, isLoading: clientsLoading } = useClients();
  const [portfolios, setPortfolios] = useState<ClientPortfolioDto[]>([]);
  const [loading, setLoading] = useState(false);

  // Fetch all client portfolios
  useEffect(() => {
    const fetchPortfolios = async () => {
      if (!clientsData?.items) return;

      setLoading(true);
      try {
        const results = await Promise.all(
          clientsData.items.map((client) =>
            portfolioApi.getClientPortfolio(client.id).catch(() => null)
          )
        );

        const validPortfolios = results.filter(
          (p): p is ClientPortfolioDto => p !== null
        );
        setPortfolios(validPortfolios);
      } catch (error) {
        console.error('Failed to fetch portfolios:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchPortfolios();
  }, [clientsData]);

  // Calculate totals
  const totalAUM = portfolios.reduce((sum, p) => sum + p.totalValueUsd, 0);
  const totalCrypto = portfolios.reduce((sum, p) => sum + p.cryptoValueUsd, 0);
  const totalTradFi = portfolios.reduce((sum, p) => sum + p.traditionalValueUsd, 0);

  // Sort portfolios by total value
  const sortedPortfolios = [...portfolios].sort(
    (a, b) => b.totalValueUsd - a.totalValueUsd
  );

  const isLoading = clientsLoading || loading;

  return (
    <PageLayout>
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Portfolio</h1>
        <p className="text-gray-500 dark:text-gray-400 mt-1">
          Assets under management across all clients
        </p>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 shadow-card dark:shadow-card-dark transition-colors">
          <p className="text-sm text-gray-500 dark:text-gray-400">Total AUM</p>
          <p className="text-3xl font-bold text-gray-900 dark:text-white mt-2">
            {isLoading ? '...' : formatCurrency(totalAUM)}
          </p>
          <p className="text-xs text-gray-400 dark:text-gray-500 mt-1">
            {portfolios.length} clients
          </p>
        </div>

        <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 shadow-card dark:shadow-card-dark transition-colors">
          <p className="text-sm text-gray-500 dark:text-gray-400">Crypto Assets</p>
          <p className="text-3xl font-bold text-blue-600 dark:text-blue-400 mt-2">
            {isLoading ? '...' : formatCurrency(totalCrypto)}
          </p>
          <p className="text-xs text-gray-400 dark:text-gray-500 mt-1">
            {totalAUM > 0 ? `${((totalCrypto / totalAUM) * 100).toFixed(1)}%` : '0%'} of total
          </p>
        </div>

        <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 shadow-card dark:shadow-card-dark transition-colors">
          <p className="text-sm text-gray-500 dark:text-gray-400">Traditional Assets</p>
          <p className="text-3xl font-bold text-green-600 dark:text-green-400 mt-2">
            {isLoading ? '...' : formatCurrency(totalTradFi)}
          </p>
          <p className="text-xs text-gray-400 dark:text-gray-500 mt-1">
            {totalAUM > 0 ? `${((totalTradFi / totalAUM) * 100).toFixed(1)}%` : '0%'} of total
          </p>
        </div>
      </div>

      {/* Client Portfolios Table */}
      <div className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 shadow-card dark:shadow-card-dark overflow-hidden transition-colors">
        <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
            Client Portfolios
          </h2>
        </div>

        {isLoading ? (
          <div className="p-12 text-center">
            <div className="animate-pulse text-gray-400 dark:text-gray-500">
              Loading portfolios...
            </div>
          </div>
        ) : sortedPortfolios.length === 0 ? (
          <div className="p-12 text-center">
            <p className="text-gray-500 dark:text-gray-400">
              No client portfolios found. Add clients and allocations to see portfolio data.
            </p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-50 dark:bg-gray-900/50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Client
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Total Value
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Crypto
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Traditional
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Assets
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Last Updated
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                {sortedPortfolios.map((portfolio) => (
                  <tr
                    key={portfolio.clientId}
                    className="hover:bg-gray-50 dark:hover:bg-gray-900/30 transition-colors"
                  >
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900 dark:text-white">
                        {portfolio.clientName || 'Unknown Client'}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right">
                      <div className="text-sm font-semibold text-gray-900 dark:text-white">
                        {formatCurrency(portfolio.totalValueUsd)}
                      </div>
                      <div className="text-xs text-gray-500 dark:text-gray-400">
                        {totalAUM > 0
                          ? `${((portfolio.totalValueUsd / totalAUM) * 100).toFixed(1)}%`
                          : '0%'}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-blue-600 dark:text-blue-400">
                      {formatCurrency(portfolio.cryptoValueUsd)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-green-600 dark:text-green-400">
                      {formatCurrency(portfolio.traditionalValueUsd)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-gray-500 dark:text-gray-400">
                      {portfolio.assets.length}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-xs text-gray-500 dark:text-gray-400">
                      {formatDate(portfolio.calculatedAt)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm">
                      <Link
                        to={`/clients/${portfolio.clientId}`}
                        className="text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 font-medium transition-colors"
                      >
                        View Details
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </PageLayout>
  );
}
