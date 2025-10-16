/**
 * Test Data Fixtures
 *
 * Shared test data used across E2E tests.
 * These fixtures provide consistent data for testing.
 */

import type {
  CreateClientCommand,
  AddWalletCommand,
  CreateAllocationCommand,
} from '../../src/types/api';

/**
 * Client Test Data
 */
export const testClients: CreateClientCommand[] = [
  {
    name: 'John Doe',
    email: 'john.doe@test.com',
    document: '12345678901',
    phoneNumber: '+1234567890',
    notes: 'Test client for E2E testing',
  },
  {
    name: 'Jane Smith',
    email: 'jane.smith@test.com',
    document: '98765432109',
    phoneNumber: '+9876543210',
    notes: 'Another test client',
  },
  {
    name: 'Acme Corporation',
    email: 'contact@acme.test',
    document: '12345678000190',
    phoneNumber: '+1555000111',
    notes: 'Corporate client',
  },
];

/**
 * Wallet Test Data
 */
export const testWallets: AddWalletCommand[] = [
  {
    walletAddress: '0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb1',
    label: 'Main Custody Wallet',
    supportedChains: ['ethereum', 'polygon', 'bsc'],
    notes: 'Primary wallet for testing',
  },
  {
    walletAddress: '0x8ba1f109551bD432803012645Ac136ddd64DBA72',
    label: 'Secondary Custody Wallet',
    supportedChains: ['ethereum', 'avalanche'],
    notes: 'Secondary wallet for testing',
  },
  {
    walletAddress: '0xAb5801a7D398351b8bE11C439e05C5B3259aeC9B',
    label: 'DeFi Strategy Wallet',
    supportedChains: ['ethereum', 'polygon', 'arbitrum'],
    notes: 'Used for DeFi strategies',
  },
];

/**
 * Allocation Test Data (templates - needs runtime IDs)
 */
export const testAllocations = {
  percentage: (clientId: string, assetId: string): CreateAllocationCommand => ({
    clientId,
    assetType: 'Wallet',
    assetId,
    allocationType: 'Percentage',
    allocationValue: 25.5,
    startDate: new Date().toISOString().split('T')[0],
    notes: 'Percentage-based allocation',
  }),

  fixedAmount: (clientId: string, assetId: string): CreateAllocationCommand => ({
    clientId,
    assetType: 'Wallet',
    assetId,
    allocationType: 'FixedAmount',
    allocationValue: 10000,
    startDate: new Date().toISOString().split('T')[0],
    notes: 'Fixed amount allocation',
  }),
};

/**
 * Invalid Test Data (for validation tests)
 */
export const invalidClientData = {
  emptyName: {
    name: '',
    email: 'test@test.com',
  },
  invalidEmail: {
    name: 'Test User',
    email: 'not-an-email',
  },
  missingRequired: {
    name: 'Test User',
    // Missing email
  },
};

export const invalidWalletData = {
  emptyAddress: {
    walletAddress: '',
    label: 'Test Wallet',
    supportedChains: ['ethereum'],
  },
  invalidAddress: {
    walletAddress: 'not-a-valid-address',
    label: 'Test Wallet',
    supportedChains: ['ethereum'],
  },
  emptyLabel: {
    walletAddress: '0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb1',
    label: '',
    supportedChains: ['ethereum'],
  },
};

export const invalidAllocationData = {
  percentageOver100: {
    allocationType: 'Percentage',
    allocationValue: 150,
  },
  negativeAmount: {
    allocationType: 'FixedAmount',
    allocationValue: -1000,
  },
  zeroPercentage: {
    allocationType: 'Percentage',
    allocationValue: 0,
  },
};

/**
 * Test Date Ranges
 */
export const testDateRanges = {
  lastWeek: {
    from: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000)
      .toISOString()
      .split('T')[0],
    to: new Date().toISOString().split('T')[0],
  },
  lastMonth: {
    from: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000)
      .toISOString()
      .split('T')[0],
    to: new Date().toISOString().split('T')[0],
  },
  lastYear: {
    from: new Date(Date.now() - 365 * 24 * 60 * 60 * 1000)
      .toISOString()
      .split('T')[0],
    to: new Date().toISOString().split('T')[0],
  },
};

/**
 * Timeout values for different operations
 */
export const timeouts = {
  short: 5000, // 5 seconds
  medium: 10000, // 10 seconds
  long: 30000, // 30 seconds
  apiCall: 15000, // 15 seconds for API calls
};

/**
 * Selectors for common UI elements
 */
export const selectors = {
  // Navigation
  navDashboard: 'a[href="/dashboard"]',
  navClients: 'a[href="/clients"]',
  navWallets: 'a[href="/wallets"]',
  navAllocations: 'a[href="/allocations"]',
  navPortfolio: 'a[href="/portfolio"]',
  navTransactions: 'a[href="/transactions"]',

  // Common buttons
  btnAdd: 'button:has-text("Add")',
  btnCreate: 'button:has-text("Create")',
  btnSave: 'button:has-text("Save")',
  btnCancel: 'button:has-text("Cancel")',
  btnDelete: 'button:has-text("Delete")',
  btnEdit: 'button:has-text("Edit")',
  btnConfirm: 'button:has-text("Confirm")',
  btnClose: 'button:has-text("Close")',

  // Common form fields
  inputName: 'input[name="name"]',
  inputEmail: 'input[name="email"]',
  inputDocument: 'input[name="document"]',
  inputPhoneNumber: 'input[name="phoneNumber"]',
  textareaNotes: 'textarea[name="notes"]',

  // Table elements
  table: 'table',
  tableRow: 'tbody tr',
  tableCell: 'td',
  tableHeader: 'thead th',

  // Dialog/Modal
  dialog: '[role="dialog"]',
  dialogTitle: '[role="dialog"] h2',
  dialogClose: '[role="dialog"] button[aria-label="Close"]',

  // Toast notifications
  toast: '.Toastify__toast',
  toastSuccess: '.Toastify__toast--success',
  toastError: '.Toastify__toast--error',

  // Loading states
  loading: '[data-testid="loading"]',
  spinner: '.spinner',

  // Pagination
  paginationNext: 'button[aria-label="Next page"]',
  paginationPrev: 'button[aria-label="Previous page"]',
  paginationInfo: '[data-testid="pagination-info"]',
};
