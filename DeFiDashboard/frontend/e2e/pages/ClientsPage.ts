import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';
import type { CreateClientCommand } from '../../src/types/api';

/**
 * Clients Page Object
 *
 * Represents the Clients page and provides methods to interact with it.
 */
export class ClientsPage extends BasePage {
  readonly addClientButton: Locator;
  readonly clientTable: Locator;
  readonly searchInput: Locator;
  readonly dialog: Locator;
  readonly dialogTitle: Locator;

  // Form fields
  readonly nameInput: Locator;
  readonly emailInput: Locator;
  readonly documentInput: Locator;
  readonly phoneInput: Locator;
  readonly notesTextarea: Locator;
  readonly saveButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page) {
    super(page);
    this.addClientButton = page.getByRole('button', { name: /add client/i });
    this.clientTable = page.locator('table');
    this.searchInput = page.getByPlaceholder(/search/i);
    this.dialog = page.locator('[role="dialog"]');
    this.dialogTitle = page.locator('[role="dialog"] h2');

    // Form fields
    this.nameInput = page.locator('input[name="name"]');
    this.emailInput = page.locator('input[name="email"]');
    this.documentInput = page.locator('input[name="document"]');
    this.phoneInput = page.locator('input[name="phoneNumber"]');
    this.notesTextarea = page.locator('textarea[name="notes"]');
    this.saveButton = page.getByRole('button', { name: /save|create/i });
    this.cancelButton = page.getByRole('button', { name: /cancel/i });
  }

  /**
   * Navigate to Clients page
   */
  async navigate() {
    await this.goto('/clients');
    await this.waitForLoading();
  }

  /**
   * Open Add Client dialog
   */
  async openAddClientDialog() {
    await this.addClientButton.click();
    await this.dialog.waitFor({ state: 'visible' });
  }

  /**
   * Fill client form
   */
  async fillClientForm(client: CreateClientCommand) {
    await this.nameInput.fill(client.name);
    await this.emailInput.fill(client.email);

    if (client.document) {
      await this.documentInput.fill(client.document);
    }

    if (client.phoneNumber) {
      await this.phoneInput.fill(client.phoneNumber);
    }

    if (client.notes) {
      await this.notesTextarea.fill(client.notes);
    }
  }

  /**
   * Submit client form
   */
  async submitClientForm() {
    await this.saveButton.click();
  }

  /**
   * Create a new client (full flow)
   */
  async createClient(client: CreateClientCommand) {
    await this.openAddClientDialog();
    await this.fillClientForm(client);
    await this.submitClientForm();
    await this.waitForLoading();
  }

  /**
   * Search for a client
   */
  async searchClient(query: string) {
    await this.searchInput.fill(query);
    await this.waitForLoading();
  }

  /**
   * Get client row by name
   */
  getClientRow(name: string): Locator {
    return this.page.locator(`tr:has-text("${name}")`);
  }

  /**
   * Check if client exists in table
   */
  async clientExists(name: string): Promise<boolean> {
    return await this.getClientRow(name).isVisible({ timeout: 5000 });
  }

  /**
   * Click on a client to view details
   */
  async clickClientByName(name: string) {
    await this.getClientRow(name).click();
    await this.waitForNavigation();
  }

  /**
   * Get client count from table
   */
  async getClientCount(): Promise<number> {
    return await this.clientTable.locator('tbody tr').count();
  }

  /**
   * Delete client by name
   */
  async deleteClientByName(name: string) {
    const row = this.getClientRow(name);
    const deleteButton = row.getByRole('button', { name: /delete/i });
    await deleteButton.click();

    // Confirm deletion in dialog
    const confirmButton = this.page.getByRole('button', { name: /confirm/i });
    await confirmButton.click();
    await this.waitForLoading();
  }

  /**
   * Edit client by name
   */
  async editClientByName(name: string) {
    const row = this.getClientRow(name);
    const editButton = row.getByRole('button', { name: /edit/i });
    await editButton.click();
    await this.dialog.waitFor({ state: 'visible' });
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
   * Get all client names from table
   */
  async getAllClientNames(): Promise<string[]> {
    const rows = await this.clientTable.locator('tbody tr').all();
    const names: string[] = [];

    for (const row of rows) {
      const nameCell = row.locator('td').first();
      const name = await nameCell.textContent();
      if (name) names.push(name.trim());
    }

    return names;
  }

  /**
   * Close dialog
   */
  async closeDialog() {
    await this.cancelButton.click();
    await this.dialog.waitFor({ state: 'hidden' });
  }

  /**
   * Wait for table to load
   */
  async waitForTableLoad() {
    await this.clientTable.waitFor({ state: 'visible' });
    await this.waitForLoading();
  }

  /**
   * Check if no clients message is visible
   */
  async isNoClientsMessageVisible(): Promise<boolean> {
    const noDataMessage = this.page.getByText(/no clients found/i);
    return await noDataMessage.isVisible({ timeout: 5000 });
  }
}
