import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Portfolio Page Object
 *
 * Represents the Portfolio page and provides methods to interact with it.
 */
export class PortfolioPage extends BasePage {
  readonly clientFilter: Locator;
  readonly totalValueCard: Locator;
  readonly allocationChart: Locator;
  readonly performanceChart: Locator;
  readonly portfolioTable: Locator;
  readonly refreshButton: Locator;
  readonly exportButton: Locator;

  constructor(page: Page) {
    super(page);
    this.clientFilter = page.locator('select[name="clientFilter"]');
    this.totalValueCard = page.locator('[data-testid="total-value-card"]');
    this.allocationChart = page.locator('[data-testid="allocation-chart"]');
    this.performanceChart = page.locator('[data-testid="performance-chart"]');
    this.portfolioTable = page.locator('table');
    this.refreshButton = page.getByRole('button', { name: /refresh/i });
    this.exportButton = page.getByRole('button', { name: /export/i });
  }

  /**
   * Navigate to Portfolio page
   */
  async navigate() {
    await this.goto('/portfolio');
    await this.waitForLoading();
  }

  /**
   * Get total portfolio value
   */
  async getTotalValue(): Promise<string | null> {
    if (await this.totalValueCard.isVisible({ timeout: 5000 })) {
      return await this.totalValueCard.textContent();
    }
    return null;
  }

  /**
   * Filter portfolio by client
   */
  async filterByClient(clientId: string) {
    await this.clientFilter.selectOption(clientId);
    await this.waitForLoading();
  }

  /**
   * Check if allocation chart is visible
   */
  async isAllocationChartVisible(): Promise<boolean> {
    return await this.allocationChart.isVisible({ timeout: 5000 });
  }

  /**
   * Check if performance chart is visible
   */
  async isPerformanceChartVisible(): Promise<boolean> {
    return await this.performanceChart.isVisible({ timeout: 5000 });
  }

  /**
   * Get asset count from portfolio table
   */
  async getAssetCount(): Promise<number> {
    return await this.portfolioTable.locator('tbody tr').count();
  }

  /**
   * Get asset details by name
   */
  async getAssetDetails(assetName: string): Promise<{
    balance: string;
    value: string;
    allocation: string;
  } | null> {
    const row = this.page.locator(`tr:has-text("${assetName}")`);

    if (!(await row.isVisible({ timeout: 5000 }))) {
      return null;
    }

    const cells = await row.locator('td').allTextContents();

    return {
      balance: cells[1] || '',
      value: cells[2] || '',
      allocation: cells[3] || '',
    };
  }

  /**
   * Refresh portfolio data
   */
  async refreshPortfolio() {
    await this.refreshButton.click();
    await this.waitForLoading();
  }

  /**
   * Export portfolio
   */
  async exportPortfolio() {
    const downloadPromise = this.page.waitForEvent('download');
    await this.exportButton.click();
    return await downloadPromise;
  }

  /**
   * Check if portfolio is empty
   */
  async isPortfolioEmpty(): Promise<boolean> {
    const emptyMessage = this.page.getByText(/no portfolio data/i);
    return await emptyMessage.isVisible({ timeout: 5000 });
  }

  /**
   * Get all asset names from portfolio
   */
  async getAllAssetNames(): Promise<string[]> {
    const rows = await this.portfolioTable.locator('tbody tr').all();
    const names: string[] = [];

    for (const row of rows) {
      const nameCell = row.locator('td').first();
      const name = await nameCell.textContent();
      if (name) names.push(name.trim());
    }

    return names;
  }

  /**
   * Click on asset to view details
   */
  async clickAssetByName(assetName: string) {
    const row = this.page.locator(`tr:has-text("${assetName}")`);
    await row.click();
    await this.waitForNavigation();
  }

  /**
   * Get performance metrics
   */
  async getPerformanceMetrics(): Promise<{
    totalReturn: string;
    dailyChange: string;
    weeklyChange: string;
    monthlyChange: string;
  } | null> {
    const metricsCard = this.page.locator(
      '[data-testid="performance-metrics"]'
    );

    if (!(await metricsCard.isVisible({ timeout: 5000 }))) {
      return null;
    }

    const totalReturn = await metricsCard
      .locator('[data-testid="total-return"]')
      .textContent();
    const dailyChange = await metricsCard
      .locator('[data-testid="daily-change"]')
      .textContent();
    const weeklyChange = await metricsCard
      .locator('[data-testid="weekly-change"]')
      .textContent();
    const monthlyChange = await metricsCard
      .locator('[data-testid="monthly-change"]')
      .textContent();

    return {
      totalReturn: totalReturn || '',
      dailyChange: dailyChange || '',
      weeklyChange: weeklyChange || '',
      monthlyChange: monthlyChange || '',
    };
  }

  /**
   * Check if loading spinner is visible
   */
  async isLoadingSpinnerVisible(): Promise<boolean> {
    const spinner = this.page.locator('[data-testid="loading-spinner"]');
    return await spinner.isVisible({ timeout: 1000 }).catch(() => false);
  }

  /**
   * Wait for charts to load
   */
  async waitForChartsToLoad() {
    await this.page.waitForSelector('[data-testid="allocation-chart"]', {
      state: 'visible',
      timeout: 10000,
    });
    await this.page.waitForSelector('[data-testid="performance-chart"]', {
      state: 'visible',
      timeout: 10000,
    });
  }

  /**
   * Switch between consolidated and client view
   */
  async switchToView(view: 'consolidated' | 'client') {
    const viewToggle = this.page.locator(
      `button[data-view="${view}"], a[href*="${view}"]`
    );
    await viewToggle.click();
    await this.waitForLoading();
  }

  /**
   * Get allocation breakdown percentages
   */
  async getAllocationBreakdown(): Promise<Map<string, string>> {
    const breakdown = new Map<string, string>();
    const rows = await this.portfolioTable.locator('tbody tr').all();

    for (const row of rows) {
      const cells = await row.locator('td').allTextContents();
      if (cells.length >= 4) {
        breakdown.set(cells[0].trim(), cells[3].trim());
      }
    }

    return breakdown;
  }
}
