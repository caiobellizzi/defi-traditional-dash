import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import ClientsPage from './pages/ClientsPage';

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
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          {/* Redirect root to clients page */}
          <Route path="/" element={<Navigate to="/clients" replace />} />

          {/* Clients routes */}
          <Route path="/clients" element={<ClientsPage />} />

          {/* Future routes */}
          {/* <Route path="/wallets" element={<WalletsPage />} /> */}
          {/* <Route path="/allocations" element={<AllocationsPage />} /> */}
          {/* <Route path="/dashboard" element={<DashboardPage />} /> */}

          {/* 404 fallback */}
          <Route path="*" element={<Navigate to="/clients" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
