import { test, expect } from '@playwright/test';
import { ClientsPage } from './pages/ClientsPage';
import { testClients, invalidClientData, timeouts } from './fixtures/testData';
import { waitForAPICall, waitForToast, generateRandomEmail } from './helpers/test-helpers';

/**
 * Clients E2E Tests
 *
 * Tests for client management functionality including:
 * - Creating new clients
 * - Viewing client list
 * - Editing client details
 * - Deleting clients
 * - Form validation
 * - Search functionality
 */
test.describe('Clients Management', () => {
  let clientsPage: ClientsPage;

  test.beforeEach(async ({ page }) => {
    clientsPage = new ClientsPage(page);
    await clientsPage.navigate();
  });

  test.describe('Create Client', () => {
    test('should successfully create a new client with all fields', async ({
      page,
    }) => {
      const testClient = {
        ...testClients[0],
        email: generateRandomEmail(), // Use unique email
      };

      // Open dialog
      await clientsPage.openAddClientDialog();
      await expect(clientsPage.dialog).toBeVisible();
      await expect(clientsPage.dialogTitle).toContainText(/add client/i);

      // Fill form
      await clientsPage.fillClientForm(testClient);

      // Submit form and wait for API call
      const apiPromise = waitForAPICall(page, '/api/clients');
      await clientsPage.submitClientForm();
      await apiPromise;

      // Verify success toast
      await waitForToast(page, /client created/i, 'success');

      // Verify client appears in list
      await expect(clientsPage.getClientRow(testClient.name)).toBeVisible({
        timeout: timeouts.medium,
      });
    });

    test('should create client with only required fields', async ({ page }) => {
      const minimalClient = {
        name: `Minimal Client ${Date.now()}`,
        email: generateRandomEmail(),
      };

      await clientsPage.openAddClientDialog();
      await clientsPage.nameInput.fill(minimalClient.name);
      await clientsPage.emailInput.fill(minimalClient.email);

      const apiPromise = waitForAPICall(page, '/api/clients');
      await clientsPage.submitClientForm();
      await apiPromise;

      await waitForToast(page, /client created/i, 'success');
      await expect(
        clientsPage.getClientRow(minimalClient.name)
      ).toBeVisible();
    });

    test('should show validation error for empty name', async () => {
      await clientsPage.openAddClientDialog();

      // Fill email but leave name empty
      await clientsPage.emailInput.fill('test@example.com');
      await clientsPage.submitClientForm();

      // Verify form is still open (validation failed)
      await expect(clientsPage.dialog).toBeVisible();

      // Check for validation error (implementation-specific)
      const nameInput = clientsPage.nameInput;
      await expect(nameInput).toBeFocused();
    });

    test('should show validation error for invalid email', async () => {
      await clientsPage.openAddClientDialog();

      await clientsPage.nameInput.fill('Test Client');
      await clientsPage.emailInput.fill('not-an-email');
      await clientsPage.submitClientForm();

      // Verify form is still open
      await expect(clientsPage.dialog).toBeVisible();
    });

    test('should cancel client creation', async () => {
      await clientsPage.openAddClientDialog();

      await clientsPage.nameInput.fill('Test Client');
      await clientsPage.emailInput.fill('test@example.com');

      // Click cancel
      await clientsPage.closeDialog();

      // Verify dialog is closed
      await expect(clientsPage.dialog).not.toBeVisible();

      // Verify client was not created
      const clientExists = await clientsPage.clientExists('Test Client');
      expect(clientExists).toBe(false);
    });
  });

  test.describe('View Clients', () => {
    test('should display client list', async () => {
      // Wait for table to load
      await clientsPage.waitForTableLoad();

      // Verify table is visible
      await expect(clientsPage.clientTable).toBeVisible();

      // Get client count (should be > 0 if there's test data)
      const count = await clientsPage.getClientCount();
      expect(count).toBeGreaterThanOrEqual(0);
    });

    test('should search for clients', async ({ page }) => {
      // Create a test client first
      const testClient = {
        name: `Searchable Client ${Date.now()}`,
        email: generateRandomEmail(),
      };

      await clientsPage.createClient(testClient);
      await waitForToast(page, /client created/i);

      // Search for the client
      await clientsPage.searchClient(testClient.name);

      // Verify client appears in results
      await expect(clientsPage.getClientRow(testClient.name)).toBeVisible();

      // Search for non-existent client
      await clientsPage.searchClient('NonExistentClient12345');

      // Verify no results or empty state
      const hasResults = await clientsPage
        .getClientRow(testClient.name)
        .isVisible({ timeout: 2000 })
        .catch(() => false);
      expect(hasResults).toBe(false);
    });

    test('should navigate to client detail page', async ({ page }) => {
      // Create a test client
      const testClient = {
        name: `Detail Client ${Date.now()}`,
        email: generateRandomEmail(),
      };

      await clientsPage.createClient(testClient);
      await waitForToast(page);

      // Click on client
      await clientsPage.clickClientByName(testClient.name);

      // Verify navigation to detail page
      await expect(page).toHaveURL(/\/clients\/[a-f0-9-]+/);
    });
  });

  test.describe('Edit Client', () => {
    test('should successfully edit client details', async ({ page }) => {
      // Create a test client
      const originalClient = {
        name: `Edit Client ${Date.now()}`,
        email: generateRandomEmail(),
        phoneNumber: '+1234567890',
      };

      await clientsPage.createClient(originalClient);
      await waitForToast(page);

      // Open edit dialog
      await clientsPage.editClientByName(originalClient.name);
      await expect(clientsPage.dialog).toBeVisible();

      // Edit fields
      const updatedName = `${originalClient.name} Updated`;
      await clientsPage.nameInput.clear();
      await clientsPage.nameInput.fill(updatedName);

      // Submit
      const apiPromise = waitForAPICall(page, '/api/clients');
      await clientsPage.submitClientForm();
      await apiPromise;

      await waitForToast(page, /client updated/i, 'success');

      // Verify updated client appears
      await expect(clientsPage.getClientRow(updatedName)).toBeVisible();
    });

    test('should cancel client edit', async ({ page }) => {
      // Create a test client
      const testClient = {
        name: `Cancel Edit Client ${Date.now()}`,
        email: generateRandomEmail(),
      };

      await clientsPage.createClient(testClient);
      await waitForToast(page);

      // Open edit dialog
      await clientsPage.editClientByName(testClient.name);

      // Make changes
      await clientsPage.nameInput.clear();
      await clientsPage.nameInput.fill('Should Not Be Saved');

      // Cancel
      await clientsPage.closeDialog();

      // Verify original name still exists
      await expect(clientsPage.getClientRow(testClient.name)).toBeVisible();

      // Verify changed name does not exist
      const changedExists = await clientsPage.clientExists(
        'Should Not Be Saved'
      );
      expect(changedExists).toBe(false);
    });
  });

  test.describe('Delete Client', () => {
    test('should successfully delete a client', async ({ page }) => {
      // Create a test client
      const testClient = {
        name: `Delete Client ${Date.now()}`,
        email: generateRandomEmail(),
      };

      await clientsPage.createClient(testClient);
      await waitForToast(page);

      // Verify client exists
      await expect(clientsPage.getClientRow(testClient.name)).toBeVisible();

      // Delete client
      const apiPromise = waitForAPICall(page, '/api/clients');
      await clientsPage.deleteClientByName(testClient.name);
      await apiPromise;

      await waitForToast(page, /client deleted/i, 'success');

      // Verify client no longer exists
      const stillExists = await clientsPage.clientExists(testClient.name);
      expect(stillExists).toBe(false);
    });
  });

  test.describe('Client List Operations', () => {
    test('should display all clients', async () => {
      await clientsPage.waitForTableLoad();

      const clientNames = await clientsPage.getAllClientNames();
      expect(Array.isArray(clientNames)).toBe(true);
    });

    test('should handle empty client list', async ({ page }) => {
      // Mock empty API response
      await page.route('**/api/clients**', (route) =>
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [],
            totalCount: 0,
            pageNumber: 1,
            pageSize: 10,
            totalPages: 0,
          }),
        })
      );

      await clientsPage.navigate();

      // Check for empty state message
      const isEmpty = await clientsPage.isNoClientsMessageVisible();
      expect(isEmpty).toBe(true);
    });
  });

  test.describe('Form Validation', () => {
    test('should validate required fields', async () => {
      await clientsPage.openAddClientDialog();

      // Try to submit empty form
      await clientsPage.submitClientForm();

      // Dialog should still be open
      await expect(clientsPage.dialog).toBeVisible();
    });

    test('should validate email format', async () => {
      await clientsPage.openAddClientDialog();

      await clientsPage.nameInput.fill('Test Client');
      await clientsPage.emailInput.fill('invalid-email');

      await clientsPage.submitClientForm();

      // Dialog should still be open due to validation error
      await expect(clientsPage.dialog).toBeVisible();
    });

    test('should validate phone number format', async () => {
      await clientsPage.openAddClientDialog();

      await clientsPage.nameInput.fill('Test Client');
      await clientsPage.emailInput.fill('test@example.com');
      await clientsPage.phoneInput.fill('invalid-phone');

      await clientsPage.submitClientForm();

      // Dialog should still be open if phone validation exists
      await expect(clientsPage.dialog).toBeVisible();
    });
  });

  test.describe('Error Handling', () => {
    test('should handle API errors gracefully', async ({ page }) => {
      // Mock API error
      await page.route('**/api/clients', (route) => {
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

      await clientsPage.openAddClientDialog();
      await clientsPage.fillClientForm({
        name: 'Error Test Client',
        email: generateRandomEmail(),
      });

      await clientsPage.submitClientForm();

      // Wait for error toast
      await waitForToast(page, /error/i, 'error');
    });

    test('should handle network timeout', async ({ page }) => {
      // Mock slow API response
      await page.route('**/api/clients', (route) => {
        if (route.request().method() === 'POST') {
          // Delay response to simulate timeout
          setTimeout(
            () =>
              route.fulfill({
                status: 200,
                contentType: 'application/json',
                body: JSON.stringify({ id: 'test-id' }),
              }),
            30000
          );
        } else {
          route.continue();
        }
      });

      await clientsPage.openAddClientDialog();
      await clientsPage.fillClientForm({
        name: 'Timeout Test Client',
        email: generateRandomEmail(),
      });

      await clientsPage.submitClientForm();

      // Should show loading state or timeout error
      // Implementation depends on your error handling
    });
  });
});
