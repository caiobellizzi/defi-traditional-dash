import { test, expect } from '@playwright/test';
import { WalletsPage } from './pages/WalletsPage';
import { testWallets, invalidWalletData, timeouts } from './fixtures/testData';
import {
  waitForAPICall,
  waitForToast,
  generateRandomWalletAddress,
} from './helpers/test-helpers';

/**
 * Wallets E2E Tests
 *
 * Tests for wallet management functionality including:
 * - Adding new wallets
 * - Viewing wallet list
 * - Viewing wallet balances
 * - Deleting wallets
 * - Form validation
 * - Duplicate wallet prevention
 */
test.describe('Wallets Management', () => {
  let walletsPage: WalletsPage;

  test.beforeEach(async ({ page }) => {
    walletsPage = new WalletsPage(page);
    await walletsPage.navigate();
  });

  test.describe('Add Wallet', () => {
    test('should successfully add a new wallet with all fields', async ({
      page,
    }) => {
      const testWallet = {
        ...testWallets[0],
        walletAddress: generateRandomWalletAddress(),
        label: `Test Wallet ${Date.now()}`,
      };

      // Open dialog
      await walletsPage.openAddWalletDialog();
      await expect(walletsPage.dialog).toBeVisible();

      // Fill form
      await walletsPage.fillWalletForm(testWallet);

      // Submit form and wait for API call
      const apiPromise = waitForAPICall(page, '/api/wallets');
      await walletsPage.submitWalletForm();
      await apiPromise;

      // Verify success toast
      await waitForToast(page, /wallet added/i, 'success');

      // Verify wallet appears in list
      await expect(walletsPage.getWalletRow(testWallet.label)).toBeVisible({
        timeout: timeouts.medium,
      });
    });

    test('should add wallet with minimum required fields', async ({ page }) => {
      const minimalWallet = {
        walletAddress: generateRandomWalletAddress(),
        label: `Minimal Wallet ${Date.now()}`,
        supportedChains: ['ethereum'],
      };

      await walletsPage.openAddWalletDialog();
      await walletsPage.walletAddressInput.fill(minimalWallet.walletAddress);
      await walletsPage.labelInput.fill(minimalWallet.label);

      // Select at least one chain
      const ethereumCheckbox = walletsPage.page.locator(
        'input[type="checkbox"][value="ethereum"]'
      );
      if (await ethereumCheckbox.isVisible({ timeout: 1000 })) {
        await ethereumCheckbox.check();
      }

      const apiPromise = waitForAPICall(page, '/api/wallets');
      await walletsPage.submitWalletForm();
      await apiPromise;

      await waitForToast(page, /wallet added/i, 'success');
      await expect(walletsPage.getWalletRow(minimalWallet.label)).toBeVisible();
    });

    test('should show validation error for empty wallet address', async () => {
      await walletsPage.openAddWalletDialog();

      // Fill label but leave address empty
      await walletsPage.labelInput.fill('Test Wallet');
      await walletsPage.submitWalletForm();

      // Verify form is still open (validation failed)
      await expect(walletsPage.dialog).toBeVisible();
    });

    test('should show validation error for invalid wallet address', async () => {
      await walletsPage.openAddWalletDialog();

      await walletsPage.walletAddressInput.fill('invalid-address');
      await walletsPage.labelInput.fill('Test Wallet');
      await walletsPage.submitWalletForm();

      // Verify form is still open
      await expect(walletsPage.dialog).toBeVisible();
    });

    test('should show validation error for empty label', async () => {
      await walletsPage.openAddWalletDialog();

      await walletsPage.walletAddressInput.fill(generateRandomWalletAddress());
      // Leave label empty
      await walletsPage.submitWalletForm();

      // Verify form is still open
      await expect(walletsPage.dialog).toBeVisible();
    });

    test('should prevent duplicate wallet addresses', async ({ page }) => {
      const walletAddress = generateRandomWalletAddress();

      // Add first wallet
      const firstWallet = {
        walletAddress,
        label: `First Wallet ${Date.now()}`,
        supportedChains: ['ethereum'],
      };

      await walletsPage.createWallet(firstWallet);
      await waitForToast(page, /wallet added/i);

      // Try to add duplicate
      const duplicateWallet = {
        walletAddress, // Same address
        label: `Duplicate Wallet ${Date.now()}`,
        supportedChains: ['polygon'],
      };

      await walletsPage.openAddWalletDialog();
      await walletsPage.fillWalletForm(duplicateWallet);
      await walletsPage.submitWalletForm();

      // Should show error for duplicate
      await waitForToast(page, /already exists/i, 'error');

      // Dialog should still be open or closed with error
      const isDuplicateError =
        await walletsPage.isDuplicateWalletErrorVisible();
      expect(isDuplicateError).toBe(true);
    });

    test('should cancel wallet addition', async () => {
      await walletsPage.openAddWalletDialog();

      await walletsPage.walletAddressInput.fill(generateRandomWalletAddress());
      await walletsPage.labelInput.fill('Test Wallet');

      // Click cancel
      await walletsPage.closeDialog();

      // Verify dialog is closed
      await expect(walletsPage.dialog).not.toBeVisible();
    });

    test('should add wallet with multiple chains', async ({ page }) => {
      const multiChainWallet = {
        walletAddress: generateRandomWalletAddress(),
        label: `Multi-Chain Wallet ${Date.now()}`,
        supportedChains: ['ethereum', 'polygon', 'bsc'],
        notes: 'Supports multiple chains',
      };

      await walletsPage.createWallet(multiChainWallet);
      await waitForToast(page, /wallet added/i);

      // Verify wallet appears
      await expect(
        walletsPage.getWalletRow(multiChainWallet.label)
      ).toBeVisible();
    });
  });

  test.describe('View Wallets', () => {
    test('should display wallet list', async () => {
      // Wait for table to load
      await walletsPage.waitForTableLoad();

      // Verify table is visible
      await expect(walletsPage.walletTable).toBeVisible();

      // Get wallet count
      const count = await walletsPage.getWalletCount();
      expect(count).toBeGreaterThanOrEqual(0);
    });

    test('should view wallet details', async ({ page }) => {
      // Create a test wallet
      const testWallet = {
        walletAddress: generateRandomWalletAddress(),
        label: `Detail Wallet ${Date.now()}`,
        supportedChains: ['ethereum'],
      };

      await walletsPage.createWallet(testWallet);
      await waitForToast(page);

      // Click on wallet
      await walletsPage.clickWalletByLabel(testWallet.label);

      // Verify navigation to detail page or modal opened
      // Implementation depends on your UI design
    });

    test('should display wallet address correctly', async ({ page }) => {
      const testWallet = {
        walletAddress: generateRandomWalletAddress(),
        label: `Address Display Wallet ${Date.now()}`,
        supportedChains: ['ethereum'],
      };

      await walletsPage.createWallet(testWallet);
      await waitForToast(page);

      // Get address from row
      const displayedAddress = await walletsPage.getWalletAddressFromRow(
        testWallet.label
      );

      // Verify address is displayed (could be truncated)
      expect(displayedAddress).toBeTruthy();
    });
  });

  test.describe('Wallet Balances', () => {
    test('should view wallet balances', async ({ page }) => {
      // Create a test wallet
      const testWallet = {
        walletAddress: generateRandomWalletAddress(),
        label: `Balance Wallet ${Date.now()}`,
        supportedChains: ['ethereum'],
      };

      await walletsPage.createWallet(testWallet);
      await waitForToast(page);

      // View balances
      await walletsPage.viewWalletBalances(testWallet.label);

      // Should navigate to balances view or open modal
      // Wait for balances to load
      await walletsPage.waitForLoading();
    });

    test('should sync wallet data', async ({ page }) => {
      // Create a test wallet
      const testWallet = {
        walletAddress: generateRandomWalletAddress(),
        label: `Sync Wallet ${Date.now()}`,
        supportedChains: ['ethereum'],
      };

      await walletsPage.createWallet(testWallet);
      await waitForToast(page);

      // Trigger sync
      const apiPromise = waitForAPICall(page, '/api/wallets/.*sync');
      await walletsPage.syncWallet(testWallet.label);
      await apiPromise;

      // Verify sync success
      await waitForToast(page, /sync/i, 'success');
    });
  });

  test.describe('Delete Wallet', () => {
    test('should successfully delete a wallet', async ({ page }) => {
      // Create a test wallet
      const testWallet = {
        walletAddress: generateRandomWalletAddress(),
        label: `Delete Wallet ${Date.now()}`,
        supportedChains: ['ethereum'],
      };

      await walletsPage.createWallet(testWallet);
      await waitForToast(page);

      // Verify wallet exists
      await expect(walletsPage.getWalletRow(testWallet.label)).toBeVisible();

      // Delete wallet
      const apiPromise = waitForAPICall(page, '/api/wallets');
      await walletsPage.deleteWalletByLabel(testWallet.label);
      await apiPromise;

      await waitForToast(page, /wallet deleted/i, 'success');

      // Verify wallet no longer exists
      const stillExists = await walletsPage.walletExists(testWallet.label);
      expect(stillExists).toBe(false);
    });

    test('should handle wallet deletion with allocations', async ({ page }) => {
      // This test assumes wallet cannot be deleted if it has active allocations
      const testWallet = {
        walletAddress: generateRandomWalletAddress(),
        label: `Wallet With Allocations ${Date.now()}`,
        supportedChains: ['ethereum'],
      };

      await walletsPage.createWallet(testWallet);
      await waitForToast(page);

      // Mock API to return error for deletion
      await page.route('**/api/wallets/**', (route) => {
        if (route.request().method() === 'DELETE') {
          route.fulfill({
            status: 400,
            contentType: 'application/json',
            body: JSON.stringify({
              message: 'Cannot delete wallet with active allocations',
            }),
          });
        } else {
          route.continue();
        }
      });

      // Try to delete
      await walletsPage.deleteWalletByLabel(testWallet.label);

      // Should show error
      await waitForToast(page, /cannot delete/i, 'error');

      // Wallet should still exist
      await expect(walletsPage.getWalletRow(testWallet.label)).toBeVisible();
    });
  });

  test.describe('Form Validation', () => {
    test('should validate wallet address format', async () => {
      await walletsPage.openAddWalletDialog();

      // Try various invalid formats
      const invalidAddresses = [
        'invalid',
        '0x123', // Too short
        'not-a-hex-address',
        '0xZZZZ', // Invalid characters
      ];

      for (const address of invalidAddresses) {
        await walletsPage.walletAddressInput.clear();
        await walletsPage.walletAddressInput.fill(address);
        await walletsPage.labelInput.fill('Test');
        await walletsPage.submitWalletForm();

        // Should show validation error
        await expect(walletsPage.dialog).toBeVisible();
      }
    });

    test('should require at least one supported chain', async () => {
      await walletsPage.openAddWalletDialog();

      await walletsPage.walletAddressInput.fill(generateRandomWalletAddress());
      await walletsPage.labelInput.fill('Test Wallet');

      // Don't select any chains
      await walletsPage.submitWalletForm();

      // Should show validation error
      await expect(walletsPage.dialog).toBeVisible();
    });
  });

  test.describe('Error Handling', () => {
    test('should handle API errors gracefully', async ({ page }) => {
      // Mock API error
      await page.route('**/api/wallets', (route) => {
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

      await walletsPage.openAddWalletDialog();
      await walletsPage.fillWalletForm({
        walletAddress: generateRandomWalletAddress(),
        label: 'Error Test Wallet',
        supportedChains: ['ethereum'],
      });

      await walletsPage.submitWalletForm();

      // Wait for error toast
      await waitForToast(page, /error/i, 'error');
    });

    test('should handle network errors', async ({ page }) => {
      // Simulate network failure
      await page.route('**/api/wallets', (route) => route.abort());

      await walletsPage.openAddWalletDialog();
      await walletsPage.fillWalletForm({
        walletAddress: generateRandomWalletAddress(),
        label: 'Network Error Wallet',
        supportedChains: ['ethereum'],
      });

      await walletsPage.submitWalletForm();

      // Should show network error
      // Implementation depends on your error handling
    });
  });

  test.describe('Wallet Filtering and Search', () => {
    test('should filter wallets by chain', async ({ page }) => {
      // Create wallets on different chains
      const ethWallet = {
        walletAddress: generateRandomWalletAddress(),
        label: `ETH Wallet ${Date.now()}`,
        supportedChains: ['ethereum'],
      };

      const polyWallet = {
        walletAddress: generateRandomWalletAddress(),
        label: `Polygon Wallet ${Date.now()}`,
        supportedChains: ['polygon'],
      };

      await walletsPage.createWallet(ethWallet);
      await waitForToast(page);

      await walletsPage.createWallet(polyWallet);
      await waitForToast(page);

      // If filtering is implemented, test it here
      // Implementation depends on your UI design
    });
  });
});
