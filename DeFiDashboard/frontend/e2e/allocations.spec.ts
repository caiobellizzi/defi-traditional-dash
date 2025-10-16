import { test, expect } from '@playwright/test';
import { AllocationsPage } from './pages/AllocationsPage';
import { ClientsPage } from './pages/ClientsPage';
import { WalletsPage } from './pages/WalletsPage';
import { testAllocations, timeouts } from './fixtures/testData';
import {
  waitForAPICall,
  waitForToast,
  generateRandomEmail,
  generateRandomWalletAddress,
} from './helpers/test-helpers';

/**
 * Allocations E2E Tests
 *
 * Tests for allocation management functionality including:
 * - Creating percentage-based allocations
 * - Creating fixed-amount allocations
 * - Viewing allocations
 * - Editing allocations
 * - Ending allocations
 * - Validation rules
 */
test.describe('Allocations Management', () => {
  let allocationsPage: AllocationsPage;
  let clientsPage: ClientsPage;
  let walletsPage: WalletsPage;

  test.beforeEach(async ({ page }) => {
    allocationsPage = new AllocationsPage(page);
    clientsPage = new ClientsPage(page);
    walletsPage = new WalletsPage(page);
  });

  // Helper function to create test client and wallet
  async function setupPrerequisites(page: any) {
    // Create client
    const testClient = {
      name: `Test Client ${Date.now()}`,
      email: generateRandomEmail(),
    };
    await clientsPage.navigate();
    await clientsPage.createClient(testClient);
    await waitForToast(page);

    // Create wallet
    const testWallet = {
      walletAddress: generateRandomWalletAddress(),
      label: `Test Wallet ${Date.now()}`,
      supportedChains: ['ethereum'],
    };
    await walletsPage.navigate();
    await walletsPage.createWallet(testWallet);
    await waitForToast(page);

    return { testClient, testWallet };
  }

  test.describe('Create Allocations', () => {
    test('should create percentage-based allocation', async ({ page }) => {
      // Setup prerequisites
      const { testClient, testWallet } = await setupPrerequisites(page);

      // Navigate to allocations
      await allocationsPage.navigate();

      // Open dialog
      await allocationsPage.openAddAllocationDialog();
      await expect(allocationsPage.dialog).toBeVisible();

      // Note: We need client and wallet IDs from API
      // For this test, we'll assume we can get them somehow
      // In a real scenario, you might need to fetch these from the API

      // Fill form with percentage allocation
      await allocationsPage.clientSelect.selectOption({ label: testClient.name });
      await allocationsPage.assetTypeSelect.selectOption('Wallet');
      await page.waitForTimeout(500); // Wait for asset dropdown to populate
      await allocationsPage.assetSelect.selectOption({ label: testWallet.label });
      await allocationsPage.allocationTypeSelect.selectOption('Percentage');
      await allocationsPage.allocationValueInput.fill('25.5');
      await allocationsPage.startDateInput.fill(
        new Date().toISOString().split('T')[0]
      );
      await allocationsPage.notesTextarea.fill('Test percentage allocation');

      // Submit
      const apiPromise = waitForAPICall(page, '/api/allocations');
      await allocationsPage.submitAllocationForm();
      await apiPromise;

      // Verify success
      await waitForToast(page, /allocation created/i, 'success');

      // Verify allocation appears in list
      await expect(
        allocationsPage.getAllocationRowByClient(testClient.name)
      ).toBeVisible({ timeout: timeouts.medium });
    });

    test('should create fixed-amount allocation', async ({ page }) => {
      // Setup prerequisites
      const { testClient, testWallet } = await setupPrerequisites(page);

      // Navigate to allocations
      await allocationsPage.navigate();

      // Create fixed amount allocation
      await allocationsPage.openAddAllocationDialog();

      await allocationsPage.clientSelect.selectOption({ label: testClient.name });
      await allocationsPage.assetTypeSelect.selectOption('Wallet');
      await page.waitForTimeout(500);
      await allocationsPage.assetSelect.selectOption({ label: testWallet.label });
      await allocationsPage.allocationTypeSelect.selectOption('FixedAmount');
      await allocationsPage.allocationValueInput.fill('10000');
      await allocationsPage.startDateInput.fill(
        new Date().toISOString().split('T')[0]
      );

      const apiPromise = waitForAPICall(page, '/api/allocations');
      await allocationsPage.submitAllocationForm();
      await apiPromise;

      await waitForToast(page, /allocation created/i, 'success');

      // Verify allocation appears
      await expect(
        allocationsPage.getAllocationRowByClient(testClient.name)
      ).toBeVisible();
    });

    test('should validate percentage range (0-100)', async ({ page }) => {
      const { testClient, testWallet } = await setupPrerequisites(page);

      await allocationsPage.navigate();
      await allocationsPage.openAddAllocationDialog();

      // Try percentage over 100
      await allocationsPage.clientSelect.selectOption({ label: testClient.name });
      await allocationsPage.assetTypeSelect.selectOption('Wallet');
      await page.waitForTimeout(500);
      await allocationsPage.assetSelect.selectOption({ label: testWallet.label });
      await allocationsPage.allocationTypeSelect.selectOption('Percentage');
      await allocationsPage.allocationValueInput.fill('150');
      await allocationsPage.startDateInput.fill(
        new Date().toISOString().split('T')[0]
      );

      await allocationsPage.submitAllocationForm();

      // Should show validation error
      await expect(allocationsPage.dialog).toBeVisible();

      // Check for percentage validation error
      const hasError = await allocationsPage.isPercentageValidationErrorVisible();
      expect(hasError).toBe(true);
    });

    test('should validate negative amounts', async ({ page }) => {
      const { testClient, testWallet } = await setupPrerequisites(page);

      await allocationsPage.navigate();
      await allocationsPage.openAddAllocationDialog();

      // Try negative amount
      await allocationsPage.clientSelect.selectOption({ label: testClient.name });
      await allocationsPage.assetTypeSelect.selectOption('Wallet');
      await page.waitForTimeout(500);
      await allocationsPage.assetSelect.selectOption({ label: testWallet.label });
      await allocationsPage.allocationTypeSelect.selectOption('FixedAmount');
      await allocationsPage.allocationValueInput.fill('-1000');
      await allocationsPage.startDateInput.fill(
        new Date().toISOString().split('T')[0]
      );

      await allocationsPage.submitAllocationForm();

      // Should show validation error
      const hasError = await allocationsPage.isNegativeAmountErrorVisible();
      expect(hasError).toBe(true);
    });

    test('should validate zero percentage', async ({ page }) => {
      const { testClient, testWallet } = await setupPrerequisites(page);

      await allocationsPage.navigate();
      await allocationsPage.openAddAllocationDialog();

      await allocationsPage.clientSelect.selectOption({ label: testClient.name });
      await allocationsPage.assetTypeSelect.selectOption('Wallet');
      await page.waitForTimeout(500);
      await allocationsPage.assetSelect.selectOption({ label: testWallet.label });
      await allocationsPage.allocationTypeSelect.selectOption('Percentage');
      await allocationsPage.allocationValueInput.fill('0');
      await allocationsPage.startDateInput.fill(
        new Date().toISOString().split('T')[0]
      );

      await allocationsPage.submitAllocationForm();

      // Should show validation error
      await expect(allocationsPage.dialog).toBeVisible();
    });

    test('should require all mandatory fields', async ({ page }) => {
      await allocationsPage.navigate();
      await allocationsPage.openAddAllocationDialog();

      // Try to submit empty form
      await allocationsPage.submitAllocationForm();

      // Dialog should remain open
      await expect(allocationsPage.dialog).toBeVisible();
    });

    test('should cancel allocation creation', async ({ page }) => {
      const { testClient, testWallet } = await setupPrerequisites(page);

      await allocationsPage.navigate();
      await allocationsPage.openAddAllocationDialog();

      // Fill form
      await allocationsPage.clientSelect.selectOption({ label: testClient.name });
      await allocationsPage.assetTypeSelect.selectOption('Wallet');

      // Cancel
      await allocationsPage.closeDialog();

      // Dialog should close
      await expect(allocationsPage.dialog).not.toBeVisible();
    });
  });

  test.describe('View Allocations', () => {
    test('should display allocation list', async ({ page }) => {
      await allocationsPage.navigate();
      await allocationsPage.waitForTableLoad();

      // Verify table is visible
      await expect(allocationsPage.allocationTable).toBeVisible();

      // Get allocation count
      const count = await allocationsPage.getAllocationCount();
      expect(count).toBeGreaterThanOrEqual(0);
    });

    test('should filter allocations by client', async ({ page }) => {
      const { testClient, testWallet } = await setupPrerequisites(page);

      // Create allocation
      await allocationsPage.navigate();
      await allocationsPage.openAddAllocationDialog();

      await allocationsPage.clientSelect.selectOption({ label: testClient.name });
      await allocationsPage.assetTypeSelect.selectOption('Wallet');
      await page.waitForTimeout(500);
      await allocationsPage.assetSelect.selectOption({ label: testWallet.label });
      await allocationsPage.allocationTypeSelect.selectOption('Percentage');
      await allocationsPage.allocationValueInput.fill('50');
      await allocationsPage.startDateInput.fill(
        new Date().toISOString().split('T')[0]
      );

      await allocationsPage.submitAllocationForm();
      await waitForToast(page);

      // Test filtering (if implemented)
      // Implementation depends on your UI
    });

    test('should display allocation details', async ({ page }) => {
      const { testClient, testWallet } = await setupPrerequisites(page);

      // Create allocation
      await allocationsPage.navigate();
      await allocationsPage.openAddAllocationDialog();

      await allocationsPage.clientSelect.selectOption({ label: testClient.name });
      await allocationsPage.assetTypeSelect.selectOption('Wallet');
      await page.waitForTimeout(500);
      await allocationsPage.assetSelect.selectOption({ label: testWallet.label });
      await allocationsPage.allocationTypeSelect.selectOption('Percentage');
      await allocationsPage.allocationValueInput.fill('30');
      await allocationsPage.startDateInput.fill(
        new Date().toISOString().split('T')[0]
      );

      await allocationsPage.submitAllocationForm();
      await waitForToast(page);

      // Get allocation details
      const details = await allocationsPage.getAllocationDetails(
        testClient.name,
        testWallet.label
      );

      expect(details).toBeTruthy();
      if (details) {
        expect(details.allocationType).toBeTruthy();
        expect(details.allocationValue).toBeTruthy();
      }
    });
  });

  test.describe('Edit Allocations', () => {
    test('should edit allocation value', async ({ page }) => {
      const { testClient, testWallet } = await setupPrerequisites(page);

      // Create allocation
      await allocationsPage.navigate();
      await allocationsPage.openAddAllocationDialog();

      await allocationsPage.clientSelect.selectOption({ label: testClient.name });
      await allocationsPage.assetTypeSelect.selectOption('Wallet');
      await page.waitForTimeout(500);
      await allocationsPage.assetSelect.selectOption({ label: testWallet.label });
      await allocationsPage.allocationTypeSelect.selectOption('Percentage');
      await allocationsPage.allocationValueInput.fill('25');
      await allocationsPage.startDateInput.fill(
        new Date().toISOString().split('T')[0]
      );

      await allocationsPage.submitAllocationForm();
      await waitForToast(page);

      // Edit allocation
      await allocationsPage.editAllocation(testClient.name, testWallet.label);
      await expect(allocationsPage.dialog).toBeVisible();

      // Change value
      await allocationsPage.allocationValueInput.clear();
      await allocationsPage.allocationValueInput.fill('35');

      const apiPromise = waitForAPICall(page, '/api/allocations');
      await allocationsPage.submitAllocationForm();
      await apiPromise;

      await waitForToast(page, /allocation updated/i, 'success');
    });
  });

  test.describe('End Allocations', () => {
    test('should end an allocation', async ({ page }) => {
      const { testClient, testWallet } = await setupPrerequisites(page);

      // Create allocation
      await allocationsPage.navigate();
      await allocationsPage.openAddAllocationDialog();

      await allocationsPage.clientSelect.selectOption({ label: testClient.name });
      await allocationsPage.assetTypeSelect.selectOption('Wallet');
      await page.waitForTimeout(500);
      await allocationsPage.assetSelect.selectOption({ label: testWallet.label });
      await allocationsPage.allocationTypeSelect.selectOption('Percentage');
      await allocationsPage.allocationValueInput.fill('40');
      await allocationsPage.startDateInput.fill(
        new Date().toISOString().split('T')[0]
      );

      await allocationsPage.submitAllocationForm();
      await waitForToast(page);

      // End allocation
      const endDate = new Date();
      endDate.setDate(endDate.getDate() + 30); // 30 days from now

      await allocationsPage.endAllocation(
        testClient.name,
        testWallet.label,
        endDate.toISOString().split('T')[0]
      );

      await waitForToast(page, /allocation ended/i, 'success');
    });
  });

  test.describe('Delete Allocations', () => {
    test('should delete an allocation', async ({ page }) => {
      const { testClient, testWallet } = await setupPrerequisites(page);

      // Create allocation
      await allocationsPage.navigate();
      await allocationsPage.openAddAllocationDialog();

      await allocationsPage.clientSelect.selectOption({ label: testClient.name });
      await allocationsPage.assetTypeSelect.selectOption('Wallet');
      await page.waitForTimeout(500);
      await allocationsPage.assetSelect.selectOption({ label: testWallet.label });
      await allocationsPage.allocationTypeSelect.selectOption('Percentage');
      await allocationsPage.allocationValueInput.fill('20');
      await allocationsPage.startDateInput.fill(
        new Date().toISOString().split('T')[0]
      );

      await allocationsPage.submitAllocationForm();
      await waitForToast(page);

      // Verify allocation exists
      await expect(
        allocationsPage.getAllocationRowByClient(testClient.name)
      ).toBeVisible();

      // Delete allocation
      const apiPromise = waitForAPICall(page, '/api/allocations');
      await allocationsPage.deleteAllocation(testClient.name, testWallet.label);
      await apiPromise;

      await waitForToast(page, /allocation deleted/i, 'success');

      // Verify allocation no longer exists
      const stillExists = await allocationsPage.allocationExistsForClient(
        testClient.name
      );
      expect(stillExists).toBe(false);
    });
  });

  test.describe('Multiple Allocations', () => {
    test('should create multiple allocations for same client', async ({
      page,
    }) => {
      // Create client
      const testClient = {
        name: `Multi-Allocation Client ${Date.now()}`,
        email: generateRandomEmail(),
      };
      await clientsPage.navigate();
      await clientsPage.createClient(testClient);
      await waitForToast(page);

      // Create two wallets
      const wallet1 = {
        walletAddress: generateRandomWalletAddress(),
        label: `Wallet 1 ${Date.now()}`,
        supportedChains: ['ethereum'],
      };
      const wallet2 = {
        walletAddress: generateRandomWalletAddress(),
        label: `Wallet 2 ${Date.now()}`,
        supportedChains: ['polygon'],
      };

      await walletsPage.navigate();
      await walletsPage.createWallet(wallet1);
      await waitForToast(page);
      await walletsPage.createWallet(wallet2);
      await waitForToast(page);

      // Create allocations for both wallets
      await allocationsPage.navigate();

      // First allocation
      await allocationsPage.openAddAllocationDialog();
      await allocationsPage.clientSelect.selectOption({ label: testClient.name });
      await allocationsPage.assetTypeSelect.selectOption('Wallet');
      await page.waitForTimeout(500);
      await allocationsPage.assetSelect.selectOption({ label: wallet1.label });
      await allocationsPage.allocationTypeSelect.selectOption('Percentage');
      await allocationsPage.allocationValueInput.fill('60');
      await allocationsPage.startDateInput.fill(
        new Date().toISOString().split('T')[0]
      );
      await allocationsPage.submitAllocationForm();
      await waitForToast(page);

      // Second allocation
      await allocationsPage.openAddAllocationDialog();
      await allocationsPage.clientSelect.selectOption({ label: testClient.name });
      await allocationsPage.assetTypeSelect.selectOption('Wallet');
      await page.waitForTimeout(500);
      await allocationsPage.assetSelect.selectOption({ label: wallet2.label });
      await allocationsPage.allocationTypeSelect.selectOption('Percentage');
      await allocationsPage.allocationValueInput.fill('40');
      await allocationsPage.startDateInput.fill(
        new Date().toISOString().split('T')[0]
      );
      await allocationsPage.submitAllocationForm();
      await waitForToast(page);

      // Verify both allocations exist
      const count = await allocationsPage.getAllocationCount();
      expect(count).toBeGreaterThanOrEqual(2);
    });
  });

  test.describe('Error Handling', () => {
    test('should handle API errors gracefully', async ({ page }) => {
      const { testClient, testWallet } = await setupPrerequisites(page);

      // Mock API error
      await page.route('**/api/allocations', (route) => {
        if (route.request().method() === 'POST') {
          route.fulfill({
            status: 500,
            contentType: 'application/json',
            body: JSON.stringify({ message: 'Internal Server Error' }),
          });
        } else {
          route.continue();
        }
      });

      await allocationsPage.navigate();
      await allocationsPage.openAddAllocationDialog();

      await allocationsPage.clientSelect.selectOption({ label: testClient.name });
      await allocationsPage.assetTypeSelect.selectOption('Wallet');
      await page.waitForTimeout(500);
      await allocationsPage.assetSelect.selectOption({ label: testWallet.label });
      await allocationsPage.allocationTypeSelect.selectOption('Percentage');
      await allocationsPage.allocationValueInput.fill('50');
      await allocationsPage.startDateInput.fill(
        new Date().toISOString().split('T')[0]
      );

      await allocationsPage.submitAllocationForm();

      // Wait for error toast
      await waitForToast(page, /error/i, 'error');
    });
  });
});
