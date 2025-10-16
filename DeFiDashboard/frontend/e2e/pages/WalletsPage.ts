import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';
import type { AddWalletCommand } from '../../src/types/api';

/**
 * Wallets Page Object
 *
 * Represents the Wallets page and provides methods to interact with it.
 */
export class WalletsPage extends BasePage {
  readonly addWalletButton: Locator;
  readonly walletTable: Locator;
  readonly dialog: Locator;

  // Form fields
  readonly walletAddressInput: Locator;
  readonly labelInput: Locator;
  readonly notesTextarea: Locator;
  readonly saveButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page) {
    super(page);
    this.addWalletButton = page.getByRole('button', { name: /add wallet/i });
    this.walletTable = page.locator('table');
    this.dialog = page.locator('[role="dialog"]');

    // Form fields
    this.walletAddressInput = page.locator('input[name="walletAddress"]');
    this.labelInput = page.locator('input[name="label"]');
    this.notesTextarea = page.locator('textarea[name="notes"]');
    this.saveButton = page.getByRole('button', { name: /save|add/i });
    this.cancelButton = page.getByRole('button', { name: /cancel/i });
  }

  /**
   * Navigate to Wallets page
   */
  async navigate() {
    await this.goto('/wallets');
    await this.waitForLoading();
  }

  /**
   * Open Add Wallet dialog
   */
  async openAddWalletDialog() {
    await this.addWalletButton.click();
    await this.dialog.waitFor({ state: 'visible' });
  }

  /**
   * Fill wallet form
   */
  async fillWalletForm(wallet: AddWalletCommand) {
    await this.walletAddressInput.fill(wallet.walletAddress);
    await this.labelInput.fill(wallet.label);

    // Handle supported chains (checkboxes or multi-select)
    for (const chain of wallet.supportedChains) {
      const chainCheckbox = this.page.locator(
        `input[type="checkbox"][value="${chain}"]`
      );
      if (await chainCheckbox.isVisible({ timeout: 1000 })) {
        await chainCheckbox.check();
      }
    }

    if (wallet.notes) {
      await this.notesTextarea.fill(wallet.notes);
    }
  }

  /**
   * Submit wallet form
   */
  async submitWalletForm() {
    await this.saveButton.click();
  }

  /**
   * Create a new wallet (full flow)
   */
  async createWallet(wallet: AddWalletCommand) {
    await this.openAddWalletDialog();
    await this.fillWalletForm(wallet);
    await this.submitWalletForm();
    await this.waitForLoading();
  }

  /**
   * Get wallet row by label
   */
  getWalletRow(label: string): Locator {
    return this.page.locator(`tr:has-text("${label}")`);
  }

  /**
   * Get wallet row by address
   */
  getWalletRowByAddress(address: string): Locator {
    return this.page.locator(`tr:has-text("${address}")`);
  }

  /**
   * Check if wallet exists in table by label
   */
  async walletExists(label: string): Promise<boolean> {
    return await this.getWalletRow(label).isVisible({ timeout: 5000 });
  }

  /**
   * Check if wallet exists by address
   */
  async walletExistsByAddress(address: string): Promise<boolean> {
    return await this.getWalletRowByAddress(address).isVisible({
      timeout: 5000,
    });
  }

  /**
   * Click on a wallet to view details
   */
  async clickWalletByLabel(label: string) {
    await this.getWalletRow(label).click();
    await this.waitForNavigation();
  }

  /**
   * Get wallet count from table
   */
  async getWalletCount(): Promise<number> {
    return await this.walletTable.locator('tbody tr').count();
  }

  /**
   * Delete wallet by label
   */
  async deleteWalletByLabel(label: string) {
    const row = this.getWalletRow(label);
    const deleteButton = row.getByRole('button', { name: /delete/i });
    await deleteButton.click();

    // Confirm deletion in dialog
    const confirmButton = this.page.getByRole('button', { name: /confirm/i });
    await confirmButton.click();
    await this.waitForLoading();
  }

  /**
   * Get validation error message
   */
  async getValidationError(fieldName: string): Promise<string | null> {
    const errorElement = this.page.locator(
      `[name="${fieldName}"] ~ .error-message, [name="${fieldName}"] + .error-message`
    );
    if (await errorElement.isVisible({ timeout: 2000 })) {
      return await errorElement.textContent();
    }
    return null;
  }

  /**
   * Close dialog
   */
  async closeDialog() {
    await this.cancelButton.click();
    await this.dialog.waitFor({ state: 'hidden' });
  }

  /**
   * Get wallet address from row
   */
  async getWalletAddressFromRow(label: string): Promise<string | null> {
    const row = this.getWalletRow(label);
    const addressCell = row.locator('td').nth(1); // Assuming address is second column
    return await addressCell.textContent();
  }

  /**
   * Check if duplicate wallet error appears
   */
  async isDuplicateWalletErrorVisible(): Promise<boolean> {
    const errorMessage = this.page.getByText(/wallet.*already exists/i);
    return await errorMessage.isVisible({ timeout: 5000 });
  }

  /**
   * Wait for table to load
   */
  async waitForTableLoad() {
    await this.walletTable.waitFor({ state: 'visible' });
    await this.waitForLoading();
  }

  /**
   * View wallet balances
   */
  async viewWalletBalances(label: string) {
    const row = this.getWalletRow(label);
    const viewButton = row.getByRole('button', { name: /view|balances/i });
    await viewButton.click();
    await this.waitForLoading();
  }

  /**
   * Sync wallet
   */
  async syncWallet(label: string) {
    const row = this.getWalletRow(label);
    const syncButton = row.getByRole('button', { name: /sync/i });
    await syncButton.click();
    await this.waitForLoading();
  }
}
