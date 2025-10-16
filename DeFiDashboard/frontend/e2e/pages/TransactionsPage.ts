import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Transactions Page Object
 *
 * Represents the Transactions page and provides methods to interact with it.
 */
export class TransactionsPage extends BasePage {
  readonly transactionTable: Locator;
  readonly searchInput: Locator;
  readonly typeFilter: Locator;
  readonly directionFilter: Locator;
  readonly dateFromInput: Locator;
  readonly dateToInput: Locator;
  readonly applyFiltersButton: Locator;
  readonly clearFiltersButton: Locator;
  readonly paginationNext: Locator;
  readonly paginationPrev: Locator;
  readonly exportButton: Locator;

  constructor(page: Page) {
    super(page);
    this.transactionTable = page.locator('table');
    this.searchInput = page.getByPlaceholder(/search/i);
    this.typeFilter = page.locator('select[name="transactionType"]');
    this.directionFilter = page.locator('select[name="direction"]');
    this.dateFromInput = page.locator('input[name="fromDate"]');
    this.dateToInput = page.locator('input[name="toDate"]');
    this.applyFiltersButton = page.getByRole('button', {
      name: /apply|filter/i,
    });
    this.clearFiltersButton = page.getByRole('button', { name: /clear/i });
    this.paginationNext = page.getByRole('button', { name: /next/i });
    this.paginationPrev = page.getByRole('button', { name: /previous|prev/i });
    this.exportButton = page.getByRole('button', { name: /export/i });
  }

  /**
   * Navigate to Transactions page
   */
  async navigate() {
    await this.goto('/transactions');
    await this.waitForLoading();
  }

  /**
   * Get transaction count from table
   */
  async getTransactionCount(): Promise<number> {
    return await this.transactionTable.locator('tbody tr').count();
  }

  /**
   * Get transaction row by hash or external ID
   */
  getTransactionRow(identifier: string): Locator {
    return this.page.locator(`tr:has-text("${identifier}")`);
  }

  /**
   * Check if transaction exists
   */
  async transactionExists(identifier: string): Promise<boolean> {
    return await this.getTransactionRow(identifier).isVisible({
      timeout: 5000,
    });
  }

  /**
   * Filter by transaction type
   */
  async filterByType(type: 'Wallet' | 'Account') {
    await this.typeFilter.selectOption(type);
    await this.applyFiltersButton.click();
    await this.waitForLoading();
  }

  /**
   * Filter by direction
   */
  async filterByDirection(direction: 'IN' | 'OUT' | 'INTERNAL') {
    await this.directionFilter.selectOption(direction);
    await this.applyFiltersButton.click();
    await this.waitForLoading();
  }

  /**
   * Filter by date range
   */
  async filterByDateRange(fromDate: string, toDate: string) {
    await this.dateFromInput.fill(fromDate);
    await this.dateToInput.fill(toDate);
    await this.applyFiltersButton.click();
    await this.waitForLoading();
  }

  /**
   * Apply multiple filters
   */
  async applyFilters(filters: {
    type?: 'Wallet' | 'Account';
    direction?: 'IN' | 'OUT' | 'INTERNAL';
    fromDate?: string;
    toDate?: string;
  }) {
    if (filters.type) {
      await this.typeFilter.selectOption(filters.type);
    }

    if (filters.direction) {
      await this.directionFilter.selectOption(filters.direction);
    }

    if (filters.fromDate) {
      await this.dateFromInput.fill(filters.fromDate);
    }

    if (filters.toDate) {
      await this.dateToInput.fill(filters.toDate);
    }

    await this.applyFiltersButton.click();
    await this.waitForLoading();
  }

  /**
   * Clear all filters
   */
  async clearFilters() {
    await this.clearFiltersButton.click();
    await this.waitForLoading();
  }

  /**
   * Search transactions
   */
  async searchTransactions(query: string) {
    await this.searchInput.fill(query);
    await this.waitForLoading();
  }

  /**
   * Click on transaction to view details
   */
  async clickTransaction(identifier: string) {
    await this.getTransactionRow(identifier).click();
    await this.waitForNavigation();
  }

  /**
   * Get transaction details from row
   */
  async getTransactionDetails(identifier: string): Promise<{
    date: string;
    type: string;
    direction: string;
    amount: string;
    status: string;
  } | null> {
    const row = this.getTransactionRow(identifier);

    if (!(await row.isVisible({ timeout: 5000 }))) {
      return null;
    }

    const cells = await row.locator('td').allTextContents();

    return {
      date: cells[0] || '',
      type: cells[1] || '',
      direction: cells[2] || '',
      amount: cells[3] || '',
      status: cells[4] || '',
    };
  }

  /**
   * Navigate to next page
   */
  async goToNextPage() {
    await this.paginationNext.click();
    await this.waitForLoading();
  }

  /**
   * Navigate to previous page
   */
  async goToPreviousPage() {
    await this.paginationPrev.click();
    await this.waitForLoading();
  }

  /**
   * Check if next page button is enabled
   */
  async isNextPageEnabled(): Promise<boolean> {
    return await this.paginationNext.isEnabled();
  }

  /**
   * Check if previous page button is enabled
   */
  async isPreviousPageEnabled(): Promise<boolean> {
    return await this.paginationPrev.isEnabled();
  }

  /**
   * Export transactions
   */
  async exportTransactions() {
    const downloadPromise = this.page.waitForEvent('download');
    await this.exportButton.click();
    return await downloadPromise;
  }

  /**
   * Get pagination info
   */
  async getPaginationInfo(): Promise<{
    currentPage: number;
    totalPages: number;
    totalItems: number;
  } | null> {
    const paginationInfo = this.page.locator(
      '[data-testid="pagination-info"]'
    );

    if (!(await paginationInfo.isVisible({ timeout: 5000 }))) {
      return null;
    }

    const text = await paginationInfo.textContent();
    if (!text) return null;

    // Parse text like "Page 1 of 5 (50 items)"
    const match = text.match(/Page (\d+) of (\d+).*\((\d+) items\)/i);
    if (match) {
      return {
        currentPage: parseInt(match[1]),
        totalPages: parseInt(match[2]),
        totalItems: parseInt(match[3]),
      };
    }

    return null;
  }

  /**
   * Get all transaction identifiers from current page
   */
  async getAllTransactionIdentifiers(): Promise<string[]> {
    const rows = await this.transactionTable.locator('tbody tr').all();
    const identifiers: string[] = [];

    for (const row of rows) {
      const idCell = row.locator('td').nth(1); // Assuming ID is second column
      const id = await idCell.textContent();
      if (id) identifiers.push(id.trim());
    }

    return identifiers;
  }

  /**
   * Check if no transactions message is visible
   */
  async isNoTransactionsMessageVisible(): Promise<boolean> {
    const noDataMessage = this.page.getByText(/no transactions found/i);
    return await noDataMessage.isVisible({ timeout: 5000 });
  }

  /**
   * Wait for table to load
   */
  async waitForTableLoad() {
    await this.transactionTable.waitFor({ state: 'visible' });
    await this.waitForLoading();
  }

  /**
   * Get total transaction amount for current page
   */
  async getTotalAmountOnPage(): Promise<number> {
    const rows = await this.transactionTable.locator('tbody tr').all();
    let total = 0;

    for (const row of rows) {
      const amountCell = row.locator('td').nth(3); // Assuming amount is 4th column
      const amountText = await amountCell.textContent();

      if (amountText) {
        // Remove currency symbols and commas, then parse
        const amount = parseFloat(amountText.replace(/[^0-9.-]+/g, ''));
        if (!isNaN(amount)) {
          total += amount;
        }
      }
    }

    return total;
  }

  /**
   * Filter by token symbol
   */
  async filterByTokenSymbol(symbol: string) {
    const symbolFilter = this.page.locator('select[name="tokenSymbol"]');
    await symbolFilter.selectOption(symbol);
    await this.applyFiltersButton.click();
    await this.waitForLoading();
  }

  /**
   * View transaction audit trail
   */
  async viewAuditTrail(identifier: string) {
    const row = this.getTransactionRow(identifier);
    const auditButton = row.getByRole('button', { name: /audit|history/i });
    await auditButton.click();
    await this.waitForLoading();
  }
}
