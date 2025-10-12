# Playwright E2E Testing Setup Guide

Complete guide for setting up and running Playwright end-to-end tests for the DeFi-Traditional Finance Dashboard.

---

## 📦 Installation

### Prerequisites

- Node.js 18+ installed
- Frontend project initialized (Vite + React + TypeScript)
- Backend API running (for E2E tests)

### Install Playwright

```bash
cd frontend

# Install Playwright and test dependencies
npm install -D @playwright/test

# Install Playwright browsers
npx playwright install

# Install additional testing libraries
npm install -D @testing-library/react @testing-library/jest-dom vitest
```

---

## 📁 Project Structure

```
frontend/
├── tests/
│   ├── e2e/                        # Playwright E2E tests
│   │   ├── clients.spec.ts
│   │   ├── wallets.spec.ts
│   │   ├── accounts.spec.ts
│   │   ├── allocations.spec.ts
│   │   ├── transactions.spec.ts
│   │   ├── portfolio.spec.ts
│   │   └── dashboard.spec.ts
│   ├── fixtures/                   # Test fixtures and helpers
│   │   ├── test-data.ts
│   │   ├── api-mocks.ts
│   │   └── auth-helper.ts
│   └── setup/
│       └── global-setup.ts         # Global test setup
├── playwright.config.ts            # Playwright configuration
├── vitest.config.ts                # Vitest configuration (unit tests)
└── package.json
```

---

## ⚙️ Configuration

### playwright.config.ts

```typescript
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['html'],
    ['json', { outputFile: 'test-results/results.json' }],
    ['junit', { outputFile: 'test-results/junit.xml' }]
  ],
  use: {
    baseURL: 'http://localhost:5173', // Vite dev server
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    actionTimeout: 10000,
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
    // Mobile viewports
    {
      name: 'Mobile Chrome',
      use: { ...devices['Pixel 5'] },
    },
    {
      name: 'Mobile Safari',
      use: { ...devices['iPhone 12'] },
    },
  ],

  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
    timeout: 120000,
  },
});
```

### package.json Scripts

```json
{
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "preview": "vite preview",
    "test": "vitest",
    "test:e2e": "playwright test",
    "test:e2e:ui": "playwright test --ui",
    "test:e2e:debug": "playwright test --debug",
    "test:e2e:headed": "playwright test --headed",
    "test:e2e:chromium": "playwright test --project=chromium",
    "test:report": "playwright show-report"
  }
}
```

---

## 🧪 Writing E2E Tests

### Example 1: Client Management Tests

```typescript
// tests/e2e/clients.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Client Management', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.click('a[href="/clients"]');
  });

  test('should display client list', async ({ page }) => {
    await expect(page.locator('h1')).toContainText('Clients');
    await expect(page.locator('[data-testid="client-list"]')).toBeVisible();
  });

  test('should create new client', async ({ page }) => {
    // Click "Add Client" button
    await page.click('button:has-text("Add Client")');

    // Wait for modal/form to appear
    await expect(page.locator('[data-testid="client-form"]')).toBeVisible();

    // Fill in form
    await page.fill('input[name="name"]', 'Test Client');
    await page.fill('input[name="email"]', `test${Date.now()}@example.com`);
    await page.fill('input[name="document"]', '12345678901');
    await page.fill('input[name="phoneNumber"]', '+5511999999999');
    await page.fill('textarea[name="notes"]', 'Test notes');

    // Submit form
    await page.click('button[type="submit"]:has-text("Save")');

    // Verify success
    await expect(page.locator('text=Test Client')).toBeVisible({ timeout: 5000 });
    await expect(page.locator('[role="alert"]:has-text("created")')).toBeVisible();
  });

  test('should edit existing client', async ({ page }) => {
    // Click first client
    await page.click('[data-testid="client-row"]:first-child');

    // Click edit button
    await page.click('button:has-text("Edit")');

    // Update name
    await page.fill('input[name="name"]', 'Updated Client Name');

    // Save
    await page.click('button[type="submit"]:has-text("Save")');

    // Verify update
    await expect(page.locator('text=Updated Client Name')).toBeVisible();
  });

  test('should delete client', async ({ page }) => {
    // Click first client
    await page.click('[data-testid="client-row"]:first-child');

    // Click delete button
    await page.click('button:has-text("Delete")');

    // Confirm deletion
    await page.click('button:has-text("Confirm")');

    // Verify deletion
    await expect(page.locator('[role="alert"]:has-text("deleted")')).toBeVisible();
  });

  test('should search clients', async ({ page }) => {
    // Enter search term
    await page.fill('input[placeholder*="Search"]', 'Test');

    // Wait for filtered results
    await page.waitForTimeout(500); // Debounce

    // Verify results contain search term
    const rows = page.locator('[data-testid="client-row"]');
    const count = await rows.count();

    for (let i = 0; i < count; i++) {
      const text = await rows.nth(i).textContent();
      expect(text?.toLowerCase()).toContain('test');
    }
  });

  test('should navigate to client portfolio', async ({ page }) => {
    // Click first client
    await page.click('[data-testid="client-row"]:first-child');

    // Click "View Portfolio" button
    await page.click('button:has-text("View Portfolio")');

    // Verify portfolio page loaded
    await expect(page).toHaveURL(/\/clients\/[a-f0-9-]+\/portfolio/);
    await expect(page.locator('h1')).toContainText('Portfolio');
  });
});
```

### Example 2: Wallet Management Tests

```typescript
// tests/e2e/wallets.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Wallet Management', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/custody/wallets');
  });

  test('should add new wallet', async ({ page }) => {
    await page.click('button:has-text("Add Wallet")');

    // Fill in wallet details
    await page.fill('input[name="walletAddress"]', '0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb');
    await page.fill('input[name="label"]', 'Test Wallet 1');

    // Select chains
    await page.click('[data-testid="chain-selector"]');
    await page.click('text=Ethereum');
    await page.click('text=Polygon');

    // Save
    await page.click('button[type="submit"]');

    // Verify wallet added
    await expect(page.locator('text=Test Wallet 1')).toBeVisible();
    await expect(page.locator('text=0x742d35Cc')).toBeVisible();
  });

  test('should sync wallet balances', async ({ page }) => {
    // Click first wallet
    await page.click('[data-testid="wallet-row"]:first-child');

    // Click sync button
    await page.click('button:has-text("Sync")');

    // Verify sync started
    await expect(page.locator('text=Syncing')).toBeVisible();

    // Wait for sync to complete
    await expect(page.locator('text=Last synced')).toBeVisible({ timeout: 30000 });
  });

  test('should display wallet balances', async ({ page }) => {
    // Click first wallet
    await page.click('[data-testid="wallet-row"]:first-child');

    // Verify balances section exists
    await expect(page.locator('[data-testid="wallet-balances"]')).toBeVisible();

    // Verify balance cards
    const balanceCards = page.locator('[data-testid="balance-card"]');
    await expect(balanceCards.first()).toBeVisible();

    // Verify balance details
    await expect(balanceCards.first()).toContainText(/ETH|MATIC|BNB/);
  });
});
```

### Example 3: Allocation Tests

```typescript
// tests/e2e/allocations.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Allocation Management', () => {
  test('should create wallet allocation', async ({ page }) => {
    await page.goto('/allocations');

    await page.click('button:has-text("New Allocation")');

    // Select client
    await page.click('[data-testid="client-select"]');
    await page.click('text=Test Client');

    // Select asset type
    await page.click('[data-testid="asset-type-select"]');
    await page.click('text=Wallet');

    // Select wallet
    await page.click('[data-testid="wallet-select"]');
    await page.click('text=Main Wallet');

    // Select allocation type
    await page.click('[data-testid="allocation-type-select"]');
    await page.click('text=Percentage');

    // Enter allocation value
    await page.fill('input[name="allocationValue"]', '30');

    // Set start date
    await page.fill('input[name="startDate"]', '2025-01-01');

    // Save
    await page.click('button[type="submit"]');

    // Verify allocation created
    await expect(page.locator('text=30%')).toBeVisible();
    await expect(page.locator('[role="alert"]:has-text("created")')).toBeVisible();
  });

  test('should prevent over-allocation', async ({ page }) => {
    await page.goto('/allocations');

    // Try to create allocation that exceeds 100%
    await page.click('button:has-text("New Allocation")');

    await page.click('[data-testid="client-select"]');
    await page.click('text=Test Client 2');

    await page.click('[data-testid="wallet-select"]');
    await page.click('text=Main Wallet'); // Already 100% allocated

    await page.fill('input[name="allocationValue"]', '50');

    await page.click('button[type="submit"]');

    // Verify error message
    await expect(page.locator('text=exceeds 100%')).toBeVisible();
  });
});
```

### Example 4: Portfolio E2E Flow

```typescript
// tests/e2e/portfolio.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Portfolio End-to-End Flow', () => {
  test('complete flow: add client → add wallet → create allocation → view portfolio', async ({ page }) => {
    // Step 1: Create client
    await page.goto('/clients');
    await page.click('button:has-text("Add Client")');
    await page.fill('input[name="name"]', 'E2E Test Client');
    await page.fill('input[name="email"]', `e2e${Date.now()}@test.com`);
    await page.click('button[type="submit"]');
    await expect(page.locator('text=E2E Test Client')).toBeVisible();

    // Step 2: Add wallet
    await page.goto('/custody/wallets');
    await page.click('button:has-text("Add Wallet")');
    await page.fill('input[name="walletAddress"]', '0x' + '1'.repeat(40));
    await page.fill('input[name="label"]', 'E2E Test Wallet');
    await page.click('button[type="submit"]');
    await expect(page.locator('text=E2E Test Wallet')).toBeVisible();

    // Step 3: Create allocation
    await page.goto('/allocations');
    await page.click('button:has-text("New Allocation")');
    await page.click('[data-testid="client-select"]');
    await page.click('text=E2E Test Client');
    await page.click('[data-testid="wallet-select"]');
    await page.click('text=E2E Test Wallet');
    await page.fill('input[name="allocationValue"]', '100');
    await page.click('button[type="submit"]');
    await expect(page.locator('text=100%')).toBeVisible();

    // Step 4: View portfolio
    await page.goto('/clients');
    await page.click('text=E2E Test Client');
    await page.click('button:has-text("View Portfolio")');

    // Verify portfolio shows allocation
    await expect(page.locator('text=E2E Test Wallet')).toBeVisible();
    await expect(page.locator('text=100%')).toBeVisible();
  });
});
```

---

## 🎯 Test Helpers

### Fixtures

```typescript
// tests/fixtures/test-data.ts
export const testClient = {
  name: 'Test Client',
  email: 'test@example.com',
  document: '12345678901',
  phoneNumber: '+5511999999999',
};

export const testWallet = {
  walletAddress: '0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb',
  label: 'Test Wallet',
  supportedChains: ['ethereum', 'polygon'],
};

export const testAllocation = {
  allocationType: 'Percentage',
  allocationValue: 30,
  startDate: '2025-01-01',
};
```

### Page Object Model

```typescript
// tests/fixtures/pages/clients-page.ts
import { Page } from '@playwright/test';

export class ClientsPage {
  constructor(private page: Page) {}

  async goto() {
    await this.page.goto('/clients');
  }

  async clickAddClient() {
    await this.page.click('button:has-text("Add Client")');
  }

  async fillClientForm(data: {
    name: string;
    email: string;
    document?: string;
    phoneNumber?: string;
  }) {
    await this.page.fill('input[name="name"]', data.name);
    await this.page.fill('input[name="email"]', data.email);
    if (data.document) {
      await this.page.fill('input[name="document"]', data.document);
    }
    if (data.phoneNumber) {
      await this.page.fill('input[name="phoneNumber"]', data.phoneNumber);
    }
  }

  async submitForm() {
    await this.page.click('button[type="submit"]');
  }

  async createClient(data: {
    name: string;
    email: string;
    document?: string;
    phoneNumber?: string;
  }) {
    await this.clickAddClient();
    await this.fillClientForm(data);
    await this.submitForm();
  }
}

// Usage in test
test('create client using POM', async ({ page }) => {
  const clientsPage = new ClientsPage(page);
  await clientsPage.goto();
  await clientsPage.createClient({
    name: 'Test Client',
    email: 'test@example.com',
  });
});
```

---

## 🚀 Running Tests

### Command Line

```bash
# Run all tests
npm run test:e2e

# Run specific test file
npx playwright test tests/e2e/clients.spec.ts

# Run in UI mode (interactive)
npm run test:e2e:ui

# Run in debug mode
npm run test:e2e:debug

# Run in headed mode (see browser)
npm run test:e2e:headed

# Run on specific browser
npm run test:e2e:chromium

# Run with specific tag
npx playwright test --grep @smoke

# View test report
npm run test:report
```

### CI/CD Integration

```yaml
# .github/workflows/playwright.yml
name: Playwright Tests

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - uses: actions/setup-node@v3
        with:
          node-version: 18

      - name: Install dependencies
        run: cd frontend && npm ci

      - name: Install Playwright browsers
        run: cd frontend && npx playwright install --with-deps

      - name: Run Playwright tests
        run: cd frontend && npm run test:e2e

      - uses: actions/upload-artifact@v3
        if: always()
        with:
          name: playwright-report
          path: frontend/playwright-report/
          retention-days: 30
```

---

## 📊 Best Practices

### 1. Use Data Test IDs

```tsx
// ✅ Good
<button data-testid="add-client-btn">Add Client</button>

// ❌ Avoid
<button className="btn-primary">Add Client</button>
```

### 2. Wait for Elements

```typescript
// ✅ Good - wait for element
await expect(page.locator('[data-testid="client-list"]')).toBeVisible();

// ❌ Avoid - arbitrary timeout
await page.waitForTimeout(2000);
```

### 3. Independent Tests

```typescript
// ✅ Good - each test is independent
test('create client', async ({ page }) => {
  await createClient(page, { name: 'Test 1' });
});

test('edit client', async ({ page }) => {
  await createClient(page, { name: 'Test 2' });
  await editClient(page, 'Test 2');
});

// ❌ Avoid - tests depend on each other
let clientId: string;

test('create client', async ({ page }) => {
  clientId = await createClient(page);
});

test('edit client', async ({ page }) => {
  await editClient(page, clientId); // Breaks if first test fails
});
```

### 4. Use Page Object Model

Organize reusable page interactions into classes.

### 5. Tag Tests

```typescript
test('create client @smoke', async ({ page }) => {
  // Smoke test
});

test('bulk import clients @slow', async ({ page }) => {
  // Slow test
});
```

---

## 🐛 Debugging

### Debug Mode

```bash
# Open Playwright Inspector
npx playwright test --debug

# Debug specific test
npx playwright test clients.spec.ts --debug
```

### Screenshots on Failure

Automatically captured (configured in `playwright.config.ts`).

### Trace Viewer

```bash
# Run with trace
npx playwright test --trace on

# View trace
npx playwright show-trace trace.zip
```

---

## 📈 Test Coverage

Target coverage for E2E tests:

- ✅ Critical user flows: 100%
- ✅ Main features: 80%
- ✅ Edge cases: 50%

### Critical Flows to Test

1. Client creation → Allocation → Portfolio view
2. Wallet connection → Sync → Balance view
3. Account connection (Pluggy) → Sync → Balance view
4. Transaction creation → Audit trail
5. Export PDF/Excel

---

**Documentation Version**: 1.0.0
**Last Updated**: 2025-10-12
