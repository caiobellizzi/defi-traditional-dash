import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { ThemeProvider } from './contexts/ThemeContext';
import { DashboardPage } from '@/features/dashboard';
import { ClientsPage, ClientDetailPage } from '@/features/clients';
import { WalletsPage } from '@/features/wallets';
import { AccountsPage } from '@/features/accounts';
import { AllocationsPage } from '@/features/allocations';
import { PortfolioPage } from '@/features/portfolio';
import { TransactionsPage } from '@/features/transactions';
import { AnalyticsPage } from '@/features/analytics';
import { AlertsPage } from '@/features/alerts';

/**
 * Configure React Query client
 */
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
      staleTime: 30000, // 30 seconds
    },
    mutations: {
      retry: 0,
    },
  },
});

function App() {
  return (
    <ThemeProvider>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <Routes>
            {/* Redirect root to dashboard */}
            <Route path="/" element={<Navigate to="/dashboard" replace />} />

            {/* Dashboard */}
            <Route path="/dashboard" element={<DashboardPage />} />

            {/* Portfolio routes */}
            <Route path="/portfolio" element={<PortfolioPage />} />

            {/* Clients routes */}
            <Route path="/clients" element={<ClientsPage />} />
            <Route path="/clients/:clientId" element={<ClientDetailPage />} />

            {/* Wallets routes */}
            <Route path="/wallets" element={<WalletsPage />} />

            {/* Accounts routes */}
            <Route path="/accounts" element={<AccountsPage />} />

            {/* Allocations routes */}
            <Route path="/allocations" element={<AllocationsPage />} />

            {/* Transactions routes */}
            <Route path="/transactions" element={<TransactionsPage />} />

            {/* Analytics routes */}
            <Route path="/analytics" element={<AnalyticsPage />} />

            {/* Alerts routes */}
            <Route path="/alerts" element={<AlertsPage />} />

            {/* 404 fallback */}
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
          </Routes>
        </BrowserRouter>
        <Toaster
          position="top-right"
          toastOptions={{
            duration: 4000,
            style: {
              background: 'var(--toast-bg, #fff)',
              color: 'var(--toast-color, #333)',
              border: '1px solid var(--toast-border, #e5e7eb)',
            },
            success: {
              iconTheme: {
                primary: '#10b981',
                secondary: '#fff',
              },
            },
            error: {
              iconTheme: {
                primary: '#ef4444',
                secondary: '#fff',
              },
            },
          }}
        />
      </QueryClientProvider>
    </ThemeProvider>
  );
}

export default App;
