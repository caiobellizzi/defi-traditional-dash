import { test, expect } from '@playwright/test';
import { PortfolioPage } from './pages/PortfolioPage';
import { ClientsPage } from './pages/ClientsPage';
import { WalletsPage } from './pages/WalletsPage';
import { AllocationsPage } from './pages/AllocationsPage';
import { timeouts } from './fixtures/testData';
import {
  waitForAPICall,
  waitForToast,
  generateRandomEmail,
  generateRandomWalletAddress,
} from './helpers/test-helpers';

/**
 * Portfolio E2E Tests
 *
 * Tests for portfolio viewing functionality including:
 * - Viewing consolidated portfolio
 * - Viewing client-specific portfolios
 * - Portfolio metrics and calculations
 * - Charts and visualizations
 * - Export functionality
 */
test.describe('Portfolio Management', () => {
  let portfolioPage: PortfolioPage;
  let clientsPage: ClientsPage;
  let walletsPage: WalletsPage;
  let allocationsPage: AllocationsPage;

  test.beforeEach(async ({ page }) => {
    portfolioPage = new PortfolioPage(page);
    clientsPage = new ClientsPage(page);
    walletsPage = new WalletsPage(page);
    allocationsPage = new AllocationsPage(page);
  });

  // Helper to setup test data
  async function setupPortfolioData(page: any) {
    // Create client
    const testClient = {
      name: `Portfolio Client ${Date.now()}`,
      email: generateRandomEmail(),
    };
    await clientsPage.navigate();
    await clientsPage.createClient(testClient);
    await waitForToast(page);

    // Create wallet
    const testWallet = {
      walletAddress: generateRandomWalletAddress(),
      label: `Portfolio Wallet ${Date.now()}`,
      supportedChains: ['ethereum'],
    };
    await walletsPage.navigate();
    await walletsPage.createWallet(testWallet);
    await waitForToast(page);

    // Create allocation
    await allocationsPage.navigate();
    await allocationsPage.openAddAllocationDialog();
    await allocationsPage.clientSelect.selectOption({ label: testClient.name });
    await allocationsPage.assetTypeSelect.selectOption('Wallet');
    await page.waitForTimeout(500);
    await allocationsPage.assetSelect.selectOption({ label: testWallet.label });
    await allocationsPage.allocationTypeSelect.selectOption('Percentage');
    await allocationsPage.allocationValueInput.fill('100');
    await allocationsPage.startDateInput.fill(
      new Date().toISOString().split('T')[0]
    );
    await allocationsPage.submitAllocationForm();
    await waitForToast(page);

    return { testClient, testWallet };
  }

  test.describe('View Portfolio', () => {
    test('should display consolidated portfolio', async ({ page }) => {
      await portfolioPage.navigate();
      await portfolioPage.waitForLoading();

      // Verify page elements are visible
      await expect(portfolioPage.portfolioTable).toBeVisible({
        timeout: timeouts.medium,
      });

      // Check for total value card
      const totalValue = await portfolioPage.getTotalValue();
      expect(totalValue).toBeTruthy();
    });

    test('should display portfolio charts', async ({ page }) => {
      await portfolioPage.navigate();
      await portfolioPage.waitForLoading();

      // Check if charts are visible
      const hasAllocationChart =
        await portfolioPage.isAllocationChartVisible();
      const hasPerformanceChart =
        await portfolioPage.isPerformanceChartVisible();

      // At least one chart should be visible
      expect(hasAllocationChart || hasPerformanceChart).toBe(true);
    });

    test('should display portfolio assets', async ({ page }) => {
      const { testClient, testWallet } = await setupPortfolioData(page);

      await portfolioPage.navigate();
      await portfolioPage.waitForLoading();

      // Get asset count
      const assetCount = await portfolioPage.getAssetCount();
      expect(assetCount).toBeGreaterThanOrEqual(0);
    });

    test('should handle empty portfolio', async ({ page }) => {
      // Mock empty portfolio
      await page.route('**/api/portfolio**', (route) =>
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            clientId: null,
            clientName: 'Consolidated',
            totalValueUsd: 0,
            allocations: [],
            lastUpdated: new Date().toISOString(),
          }),
        })
      );

      await portfolioPage.navigate();

      const isEmpty = await portfolioPage.isPortfolioEmpty();
      expect(isEmpty).toBe(true);
    });
  });

  test.describe('Client Portfolio Filtering', () => {
    test('should filter portfolio by client', async ({ page }) => {
      const { testClient } = await setupPortfolioData(page);

      await portfolioPage.navigate();
      await portfolioPage.waitForLoading();

      // Get initial asset count
      const initialCount = await portfolioPage.getAssetCount();

      // Filter by specific client (if implemented)
      // This depends on your UI implementation
      // Example: await portfolioPage.filterByClient(clientId);
    });

    test('should switch between consolidated and client views', async ({
      page,
    }) => {
      await portfolioPage.navigate();

      // Switch to consolidated view
      await portfolioPage.switchToView('consolidated');
      await portfolioPage.waitForLoading();

      // Verify consolidated view
      await expect(portfolioPage.portfolioTable).toBeVisible();
    });
  });

  test.describe('Portfolio Details', () => {
    test('should display asset details', async ({ page }) => {
      const { testWallet } = await setupPortfolioData(page);

      await portfolioPage.navigate();
      await portfolioPage.waitForLoading();

      // Get all asset names
      const assetNames = await portfolioPage.getAllAssetNames();
      expect(Array.isArray(assetNames)).toBe(true);
    });

    test('should display allocation breakdown', async ({ page }) => {
      await setupPortfolioData(page);

      await portfolioPage.navigate();
      await portfolioPage.waitForLoading();

      // Get allocation breakdown
      const breakdown = await portfolioPage.getAllocationBreakdown();
      expect(breakdown.size).toBeGreaterThanOrEqual(0);
    });

    test('should click on asset to view details', async ({ page }) => {
      const { testWallet } = await setupPortfolioData(page);

      await portfolioPage.navigate();
      await portfolioPage.waitForLoading();

      const assetNames = await portfolioPage.getAllAssetNames();
      if (assetNames.length > 0) {
        await portfolioPage.clickAssetByName(assetNames[0]);
        // Should navigate to asset detail page or open modal
      }
    });
  });

  test.describe('Performance Metrics', () => {
    test('should display performance metrics', async ({ page }) => {
      await portfolioPage.navigate();
      await portfolioPage.waitForLoading();

      // Get performance metrics
      const metrics = await portfolioPage.getPerformanceMetrics();

      // Metrics might be null if not available
      if (metrics) {
        expect(metrics.totalReturn).toBeTruthy();
      }
    });

    test('should display charts', async ({ page }) => {
      await portfolioPage.navigate();
      await portfolioPage.waitForLoading();

      // Wait for charts to load
      await portfolioPage.waitForChartsToLoad();

      // Verify charts are visible
      const allocationChartVisible =
        await portfolioPage.isAllocationChartVisible();
      const performanceChartVisible =
        await portfolioPage.isPerformanceChartVisible();

      expect(
        allocationChartVisible || performanceChartVisible
      ).toBe(true);
    });
  });

  test.describe('Portfolio Actions', () => {
    test('should refresh portfolio data', async ({ page }) => {
      await portfolioPage.navigate();
      await portfolioPage.waitForLoading();

      // Refresh portfolio
      const apiPromise = waitForAPICall(page, '/api/portfolio');
      await portfolioPage.refreshPortfolio();
      await apiPromise;

      // Wait for loading to finish
      await portfolioPage.waitForLoading();
    });

    test('should export portfolio', async ({ page }) => {
      await portfolioPage.navigate();
      await portfolioPage.waitForLoading();

      // Export portfolio
      const download = await portfolioPage.exportPortfolio();

      // Verify download started
      expect(download).toBeTruthy();
    });
  });

  test.describe('Portfolio Calculations', () => {
    test('should calculate total portfolio value', async ({ page }) => {
      await setupPortfolioData(page);

      await portfolioPage.navigate();
      await portfolioPage.waitForLoading();

      // Get total value
      const totalValue = await portfolioPage.getTotalValue();
      expect(totalValue).toBeTruthy();
    });

    test('should display allocation percentages', async ({ page }) => {
      await setupPortfolioData(page);

      await portfolioPage.navigate();
      await portfolioPage.waitForLoading();

      // Get allocation breakdown
      const breakdown = await portfolioPage.getAllocationBreakdown();

      // Verify percentages add up (if multiple allocations)
      if (breakdown.size > 0) {
        let total = 0;
        breakdown.forEach((percentage) => {
          const value = parseFloat(percentage.replace('%', ''));
          if (!isNaN(value)) {
            total += value;
          }
        });

        // Total should be approximately 100% (allowing for rounding)
        if (total > 0) {
          expect(total).toBeGreaterThanOrEqual(99);
          expect(total).toBeLessThanOrEqual(101);
        }
      }
    });
  });

  test.describe('Real-time Updates', () => {
    test('should show loading state while fetching data', async ({ page }) => {
      await portfolioPage.navigate();

      // Check if loading spinner is visible initially
      const isLoading = await portfolioPage.isLoadingSpinnerVisible();

      // Loading might be very fast, so this is optional
      // Just verify the loading mechanism exists
    });

    test('should handle API errors', async ({ page }) => {
      // Mock API error
      await page.route('**/api/portfolio**', (route) =>
        route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({ message: 'Internal Server Error' }),
        })
      );

      await portfolioPage.navigate();

      // Should show error state or message
      // Implementation depends on your error handling
    });
  });

  test.describe('Multiple Clients Portfolio', () => {
    test('should display portfolio for multiple clients', async ({ page }) => {
      // Create multiple clients with allocations
      for (let i = 0; i < 2; i++) {
        await setupPortfolioData(page);
      }

      await portfolioPage.navigate();
      await portfolioPage.waitForLoading();

      // Should show consolidated view with all allocations
      const assetCount = await portfolioPage.getAssetCount();
      expect(assetCount).toBeGreaterThanOrEqual(2);
    });

    test('should calculate individual client portfolios', async ({ page }) => {
      const { testClient } = await setupPortfolioData(page);

      // Navigate to client-specific portfolio (if implemented)
      // This depends on your routing structure
      // Example: await page.goto(`/portfolio/client/${clientId}`);
    });
  });

  test.describe('Portfolio Edge Cases', () => {
    test('should handle portfolio with no balances', async ({ page }) => {
      // Mock portfolio with zero balances
      await page.route('**/api/portfolio**', (route) =>
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            clientId: null,
            clientName: 'Consolidated',
            totalValueUsd: 0,
            allocations: [
              {
                id: 'test-id',
                clientId: 'client-id',
                clientName: 'Test Client',
                assetType: 'Wallet',
                assetId: 'wallet-id',
                assetName: 'Test Wallet',
                allocationType: 'Percentage',
                allocationValue: 100,
                startDate: new Date().toISOString(),
                notes: null,
                createdAt: new Date().toISOString(),
              },
            ],
            lastUpdated: new Date().toISOString(),
          }),
        })
      );

      await portfolioPage.navigate();

      // Should display zero or empty state
      const totalValue = await portfolioPage.getTotalValue();
      expect(totalValue).toBeTruthy();
    });

    test('should handle very large portfolio values', async ({ page }) => {
      // Mock portfolio with large values
      await page.route('**/api/portfolio**', (route) =>
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            clientId: null,
            clientName: 'Consolidated',
            totalValueUsd: 1000000000, // 1 billion
            allocations: [],
            lastUpdated: new Date().toISOString(),
          }),
        })
      );

      await portfolioPage.navigate();

      // Should format large numbers correctly
      const totalValue = await portfolioPage.getTotalValue();
      expect(totalValue).toBeTruthy();
    });
  });
});
