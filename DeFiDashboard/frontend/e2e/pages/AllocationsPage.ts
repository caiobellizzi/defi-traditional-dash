import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';
import type { CreateAllocationCommand } from '../../src/types/api';

/**
 * Allocations Page Object
 *
 * Represents the Allocations page and provides methods to interact with it.
 */
export class AllocationsPage extends BasePage {
  readonly addAllocationButton: Locator;
  readonly allocationTable: Locator;
  readonly dialog: Locator;

  // Form fields
  readonly clientSelect: Locator;
  readonly assetTypeSelect: Locator;
  readonly assetSelect: Locator;
  readonly allocationTypeSelect: Locator;
  readonly allocationValueInput: Locator;
  readonly startDateInput: Locator;
  readonly notesTextarea: Locator;
  readonly saveButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page) {
    super(page);
    this.addAllocationButton = page.getByRole('button', {
      name: /add allocation/i,
    });
    this.allocationTable = page.locator('table');
    this.dialog = page.locator('[role="dialog"]');

    // Form fields
    this.clientSelect = page.locator('select[name="clientId"]');
    this.assetTypeSelect = page.locator('select[name="assetType"]');
    this.assetSelect = page.locator('select[name="assetId"]');
    this.allocationTypeSelect = page.locator('select[name="allocationType"]');
    this.allocationValueInput = page.locator('input[name="allocationValue"]');
    this.startDateInput = page.locator('input[name="startDate"]');
    this.notesTextarea = page.locator('textarea[name="notes"]');
    this.saveButton = page.getByRole('button', { name: /save|create/i });
    this.cancelButton = page.getByRole('button', { name: /cancel/i });
  }

  /**
   * Navigate to Allocations page
   */
  async navigate() {
    await this.goto('/allocations');
    await this.waitForLoading();
  }

  /**
   * Open Add Allocation dialog
   */
  async openAddAllocationDialog() {
    await this.addAllocationButton.click();
    await this.dialog.waitFor({ state: 'visible' });
  }

  /**
   * Fill allocation form
   */
  async fillAllocationForm(allocation: CreateAllocationCommand) {
    // Select client
    await this.clientSelect.selectOption(allocation.clientId);

    // Select asset type
    await this.assetTypeSelect.selectOption(allocation.assetType);

    // Wait for asset dropdown to populate
    await this.page.waitForTimeout(500);

    // Select asset
    await this.assetSelect.selectOption(allocation.assetId);

    // Select allocation type
    await this.allocationTypeSelect.selectOption(allocation.allocationType);

    // Enter allocation value
    await this.allocationValueInput.fill(allocation.allocationValue.toString());

    // Set start date
    await this.startDateInput.fill(allocation.startDate);

    // Add notes if provided
    if (allocation.notes) {
      await this.notesTextarea.fill(allocation.notes);
    }
  }

  /**
   * Submit allocation form
   */
  async submitAllocationForm() {
    await this.saveButton.click();
  }

  /**
   * Create a new allocation (full flow)
   */
  async createAllocation(allocation: CreateAllocationCommand) {
    await this.openAddAllocationDialog();
    await this.fillAllocationForm(allocation);
    await this.submitAllocationForm();
    await this.waitForLoading();
  }

  /**
   * Get allocation row by client name
   */
  getAllocationRowByClient(clientName: string): Locator {
    return this.page.locator(`tr:has-text("${clientName}")`);
  }

  /**
   * Check if allocation exists for client
   */
  async allocationExistsForClient(clientName: string): Promise<boolean> {
    return await this.getAllocationRowByClient(clientName).isVisible({
      timeout: 5000,
    });
  }

  /**
   * Get allocation count from table
   */
  async getAllocationCount(): Promise<number> {
    return await this.allocationTable.locator('tbody tr').count();
  }

  /**
   * Delete allocation
   */
  async deleteAllocation(clientName: string, assetName: string) {
    const row = this.page.locator(
      `tr:has-text("${clientName}"):has-text("${assetName}")`
    );
    const deleteButton = row.getByRole('button', { name: /delete/i });
    await deleteButton.click();

    // Confirm deletion in dialog
    const confirmButton = this.page.getByRole('button', { name: /confirm/i });
    await confirmButton.click();
    await this.waitForLoading();
  }

  /**
   * Edit allocation
   */
  async editAllocation(clientName: string, assetName: string) {
    const row = this.page.locator(
      `tr:has-text("${clientName}"):has-text("${assetName}")`
    );
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
   * Get form validation error (general)
   */
  async getFormError(): Promise<string | null> {
    const errorElement = this.page.locator('.form-error, [role="alert"]');
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
   * Filter allocations by client
   */
  async filterByClient(clientId: string) {
    const filterSelect = this.page.locator('select[name="clientFilter"]');
    await filterSelect.selectOption(clientId);
    await this.waitForLoading();
  }

  /**
   * Filter allocations by asset type
   */
  async filterByAssetType(assetType: string) {
    const filterSelect = this.page.locator('select[name="assetTypeFilter"]');
    await filterSelect.selectOption(assetType);
    await this.waitForLoading();
  }

  /**
   * Wait for table to load
   */
  async waitForTableLoad() {
    await this.allocationTable.waitFor({ state: 'visible' });
    await this.waitForLoading();
  }

  /**
   * Get allocation details from row
   */
  async getAllocationDetails(
    clientName: string,
    assetName: string
  ): Promise<{
    allocationType: string;
    allocationValue: string;
  } | null> {
    const row = this.page.locator(
      `tr:has-text("${clientName}"):has-text("${assetName}")`
    );

    if (!(await row.isVisible({ timeout: 5000 }))) {
      return null;
    }

    const cells = await row.locator('td').allTextContents();

    return {
      allocationType: cells[3] || '',
      allocationValue: cells[4] || '',
    };
  }

  /**
   * Check if percentage validation error appears
   */
  async isPercentageValidationErrorVisible(): Promise<boolean> {
    const errorMessage = this.page.getByText(
      /percentage.*must be.*between 0 and 100/i
    );
    return await errorMessage.isVisible({ timeout: 5000 });
  }

  /**
   * Check if negative amount validation error appears
   */
  async isNegativeAmountErrorVisible(): Promise<boolean> {
    const errorMessage = this.page.getByText(/amount.*must be.*positive/i);
    return await errorMessage.isVisible({ timeout: 5000 });
  }

  /**
   * End allocation
   */
  async endAllocation(clientName: string, assetName: string, endDate: string) {
    const row = this.page.locator(
      `tr:has-text("${clientName}"):has-text("${assetName}")`
    );
    const endButton = row.getByRole('button', { name: /end/i });
    await endButton.click();

    // Fill end date in dialog
    const endDateInput = this.page.locator('input[name="endDate"]');
    await endDateInput.fill(endDate);

    // Confirm
    const confirmButton = this.page.getByRole('button', { name: /confirm/i });
    await confirmButton.click();
    await this.waitForLoading();
  }
}
