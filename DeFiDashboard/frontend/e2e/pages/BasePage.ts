import { Page, Locator } from '@playwright/test';

/**
 * Base Page Object
 *
 * Provides common functionality for all page objects.
 */
export class BasePage {
  readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  /**
   * Navigate to a specific path
   */
  async goto(path: string) {
    await this.page.goto(path);
  }

  /**
   * Wait for navigation to complete
   */
  async waitForNavigation() {
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Wait for a specific selector
   */
  async waitForSelector(selector: string, options?: { timeout?: number }) {
    await this.page.waitForSelector(selector, options);
  }

  /**
   * Get toast notification text
   */
  async getToastText(): Promise<string | null> {
    const toast = this.page.locator('[data-testid="toast"]').first();
    if (await toast.isVisible({ timeout: 5000 })) {
      return await toast.textContent();
    }
    return null;
  }

  /**
   * Wait for toast to appear and return its text
   */
  async waitForToast(type: 'success' | 'error' = 'success'): Promise<string> {
    const toast = this.page.locator('[data-testid="toast"]').first();
    await toast.waitFor({ state: 'visible', timeout: 10000 });
    const text = await toast.textContent();
    return text || '';
  }

  /**
   * Check if element exists on page
   */
  async elementExists(selector: string): Promise<boolean> {
    return await this.page.locator(selector).count() > 0;
  }

  /**
   * Fill form field by name
   */
  async fillField(name: string, value: string) {
    await this.page.fill(`[name="${name}"]`, value);
  }

  /**
   * Click button by text
   */
  async clickButton(text: string) {
    await this.page.click(`button:has-text("${text}")`);
  }

  /**
   * Wait for API response
   */
  async waitForAPIResponse(urlPattern: string | RegExp) {
    return await this.page.waitForResponse(
      (response) =>
        (typeof urlPattern === 'string'
          ? response.url().includes(urlPattern)
          : urlPattern.test(response.url())) && response.status() === 200
    );
  }

  /**
   * Intercept API call
   */
  async interceptAPI(
    urlPattern: string | RegExp,
    response: any,
    status = 200
  ) {
    await this.page.route(urlPattern, (route) =>
      route.fulfill({
        status,
        contentType: 'application/json',
        body: JSON.stringify(response),
      })
    );
  }

  /**
   * Get page title
   */
  async getTitle(): Promise<string> {
    return await this.page.title();
  }

  /**
   * Get current URL
   */
  getCurrentURL(): string {
    return this.page.url();
  }

  /**
   * Take screenshot
   */
  async screenshot(path: string) {
    await this.page.screenshot({ path, fullPage: true });
  }

  /**
   * Wait for loading to finish
   */
  async waitForLoading() {
    // Wait for any loading indicators to disappear
    const loadingIndicator = this.page.locator(
      '[data-testid="loading"], .spinner, [role="progressbar"]'
    );
    if (await loadingIndicator.isVisible({ timeout: 1000 }).catch(() => false)) {
      await loadingIndicator.waitFor({ state: 'hidden', timeout: 30000 });
    }
  }

  /**
   * Reload page
   */
  async reload() {
    await this.page.reload();
  }

  /**
   * Go back in browser history
   */
  async goBack() {
    await this.page.goBack();
  }
}
