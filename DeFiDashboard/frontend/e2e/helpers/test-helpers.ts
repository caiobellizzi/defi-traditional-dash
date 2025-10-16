import { Page, expect } from '@playwright/test';

/**
 * Test Helper Functions
 *
 * Common utility functions used across E2E tests.
 */

/**
 * Wait for API call to complete
 */
export async function waitForAPICall(
  page: Page,
  urlPattern: string | RegExp,
  options?: { timeout?: number }
) {
  const timeout = options?.timeout || 15000;

  return await page.waitForResponse(
    (response) => {
      const matches =
        typeof urlPattern === 'string'
          ? response.url().includes(urlPattern)
          : urlPattern.test(response.url());

      return matches && response.status() >= 200 && response.status() < 300;
    },
    { timeout }
  );
}

/**
 * Wait for multiple API calls
 */
export async function waitForMultipleAPICalls(
  page: Page,
  urlPatterns: (string | RegExp)[],
  options?: { timeout?: number }
) {
  const promises = urlPatterns.map((pattern) =>
    waitForAPICall(page, pattern, options)
  );
  return await Promise.all(promises);
}

/**
 * Wait for toast notification and verify message
 */
export async function waitForToast(
  page: Page,
  expectedText?: string,
  type: 'success' | 'error' = 'success'
) {
  const toast = page.locator('[data-testid="toast"]').first();
  await toast.waitFor({ state: 'visible', timeout: 10000 });

  if (expectedText) {
    await expect(toast).toContainText(expectedText);
  }

  return toast;
}

/**
 * Clear all test data (use with caution!)
 */
export async function clearTestData(page: Page, entity: string) {
  // This is a placeholder - implement based on your API
  // Could call DELETE /api/test/clear endpoint
  await page.request.delete(`/api/test/clear/${entity}`);
}

/**
 * Create test data via API
 */
export async function createTestDataViaAPI(
  page: Page,
  endpoint: string,
  data: any
) {
  const response = await page.request.post(endpoint, {
    data,
  });

  expect(response.ok()).toBeTruthy();
  return await response.json();
}

/**
 * Format currency for display
 */
export function formatCurrency(amount: number, currency = 'USD'): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency,
  }).format(amount);
}

/**
 * Format date for input fields
 */
export function formatDateForInput(date: Date): string {
  return date.toISOString().split('T')[0];
}

/**
 * Generate random email
 */
export function generateRandomEmail(): string {
  const timestamp = Date.now();
  return `test.user.${timestamp}@example.com`;
}

/**
 * Generate random wallet address
 */
export function generateRandomWalletAddress(): string {
  const chars = '0123456789abcdef';
  let address = '0x';
  for (let i = 0; i < 40; i++) {
    address += chars[Math.floor(Math.random() * chars.length)];
  }
  return address;
}

/**
 * Generate random string
 */
export function generateRandomString(length = 10): string {
  const chars =
    'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  let result = '';
  for (let i = 0; i < length; i++) {
    result += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return result;
}

/**
 * Wait for network idle
 */
export async function waitForNetworkIdle(
  page: Page,
  options?: { timeout?: number }
) {
  await page.waitForLoadState('networkidle', options);
}

/**
 * Retry an action until it succeeds or times out
 */
export async function retryAction<T>(
  action: () => Promise<T>,
  options?: {
    maxRetries?: number;
    delay?: number;
    onError?: (error: Error, attempt: number) => void;
  }
): Promise<T> {
  const maxRetries = options?.maxRetries || 3;
  const delay = options?.delay || 1000;

  for (let attempt = 1; attempt <= maxRetries; attempt++) {
    try {
      return await action();
    } catch (error) {
      if (options?.onError) {
        options.onError(error as Error, attempt);
      }

      if (attempt === maxRetries) {
        throw error;
      }

      await new Promise((resolve) => setTimeout(resolve, delay));
    }
  }

  throw new Error('Retry action failed');
}

/**
 * Take screenshot with timestamp
 */
export async function takeScreenshot(
  page: Page,
  name: string,
  options?: { fullPage?: boolean }
) {
  const timestamp = Date.now();
  const fileName = `screenshots/${name}-${timestamp}.png`;
  await page.screenshot({
    path: fileName,
    fullPage: options?.fullPage ?? true,
  });
  return fileName;
}

/**
 * Get table cell text by row and column index
 */
export async function getTableCellText(
  page: Page,
  tableSelector: string,
  rowIndex: number,
  columnIndex: number
): Promise<string | null> {
  const cell = page
    .locator(`${tableSelector} tbody tr`)
    .nth(rowIndex)
    .locator('td')
    .nth(columnIndex);

  if (await cell.isVisible({ timeout: 5000 })) {
    return await cell.textContent();
  }

  return null;
}

/**
 * Check if element is in viewport
 */
export async function isElementInViewport(
  page: Page,
  selector: string
): Promise<boolean> {
  const element = page.locator(selector);
  return await element.isVisible();
}

/**
 * Scroll to element
 */
export async function scrollToElement(page: Page, selector: string) {
  await page.locator(selector).scrollIntoViewIfNeeded();
}

/**
 * Wait for element to be stable (not animating)
 */
export async function waitForElementStable(page: Page, selector: string) {
  const element = page.locator(selector);
  await element.waitFor({ state: 'visible' });

  // Wait a bit for animations to complete
  await page.waitForTimeout(300);
}

/**
 * Get all text contents from locators
 */
export async function getAllTextContents(page: Page, selector: string): Promise<string[]> {
  const elements = await page.locator(selector).all();
  const texts: string[] = [];

  for (const element of elements) {
    const text = await element.textContent();
    if (text) texts.push(text.trim());
  }

  return texts;
}

/**
 * Check if API endpoint returns success
 */
export async function checkAPIHealth(page: Page, endpoint: string): Promise<boolean> {
  try {
    const response = await page.request.get(endpoint);
    return response.ok();
  } catch {
    return false;
  }
}

/**
 * Mock API response
 */
export async function mockAPIResponse(
  page: Page,
  urlPattern: string | RegExp,
  responseData: any,
  status = 200
) {
  await page.route(urlPattern, (route) =>
    route.fulfill({
      status,
      contentType: 'application/json',
      body: JSON.stringify(responseData),
    })
  );
}

/**
 * Mock API error
 */
export async function mockAPIError(
  page: Page,
  urlPattern: string | RegExp,
  status = 500,
  errorMessage = 'Internal Server Error'
) {
  await page.route(urlPattern, (route) =>
    route.fulfill({
      status,
      contentType: 'application/json',
      body: JSON.stringify({ message: errorMessage }),
    })
  );
}

/**
 * Cleanup function to run after each test
 */
export async function cleanup(page: Page) {
  // Clear any open dialogs
  const dialogs = page.locator('[role="dialog"]');
  const dialogCount = await dialogs.count();

  for (let i = 0; i < dialogCount; i++) {
    const closeButton = dialogs.nth(i).locator('[aria-label="Close"]');
    if (await closeButton.isVisible({ timeout: 1000 })) {
      await closeButton.click();
    }
  }

  // Clear any toasts
  const toasts = page.locator('[data-testid="toast"]');
  const toastCount = await toasts.count();

  for (let i = 0; i < toastCount; i++) {
    const closeButton = toasts.nth(i).locator('[aria-label="Close"]');
    if (await closeButton.isVisible({ timeout: 1000 })) {
      await closeButton.click();
    }
  }
}

/**
 * Setup function to run before each test
 */
export async function setup(page: Page, baseURL: string) {
  await page.goto(baseURL);
  await page.waitForLoadState('networkidle');
}
