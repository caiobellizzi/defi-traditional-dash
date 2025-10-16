import { chromium } from '@playwright/test';

async function testApplication() {
  console.log('🚀 Starting Playwright test...\n');

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    ignoreHTTPSErrors: true,
  });
  const page = await context.newPage();

  // Collect console logs and errors
  const consoleLogs = [];
  const errors = [];

  page.on('console', (msg) => {
    consoleLogs.push({ type: msg.type(), text: msg.text() });
  });

  page.on('pageerror', (error) => {
    errors.push(error.message);
  });

  try {
    // Test 1: Check if frontend is accessible
    console.log('📍 Testing: Frontend accessibility...');
    const frontendUrl = process.env.FRONTEND_URL || 'http://localhost:5173';

    const response = await page.goto(frontendUrl, {
      waitUntil: 'networkidle',
      timeout: 30000,
    });

    if (!response.ok()) {
      console.error(`❌ Frontend returned status: ${response.status()}`);
    } else {
      console.log(`✅ Frontend accessible at ${frontendUrl}`);
    }

    // Test 2: Check Dashboard page
    console.log('\n📍 Testing: Dashboard page...');
    await page.waitForSelector('h1', { timeout: 10000 });
    const dashboardTitle = await page.textContent('h1');
    console.log(`   Title: "${dashboardTitle}"`);

    // Check for stat cards
    const statCards = await page.locator('.bg-white.rounded-lg.border.p-6').count();
    console.log(`   Found ${statCards} stat cards`);

    // Take screenshot
    await page.screenshot({ path: '/tmp/dashboard.png', fullPage: true });
    console.log('   Screenshot saved: /tmp/dashboard.png');

    // Test 3: Navigate to Clients page
    console.log('\n📍 Testing: Clients page...');
    await page.click('a[href="/clients"]');
    await page.waitForSelector('h1', { timeout: 10000 });
    const clientsTitle = await page.textContent('h1');
    console.log(`   Title: "${clientsTitle}"`);

    await page.screenshot({ path: '/tmp/clients.png', fullPage: true });
    console.log('   Screenshot saved: /tmp/clients.png');

    // Test 4: Navigate to Wallets page
    console.log('\n📍 Testing: Wallets page...');
    await page.click('a[href="/wallets"]');
    await page.waitForSelector('h1', { timeout: 10000 });
    const walletsTitle = await page.textContent('h1');
    console.log(`   Title: "${walletsTitle}"`);

    await page.screenshot({ path: '/tmp/wallets.png', fullPage: true });
    console.log('   Screenshot saved: /tmp/wallets.png');

    // Test 5: Navigate to Allocations page
    console.log('\n📍 Testing: Allocations page...');
    await page.click('a[href="/allocations"]');
    await page.waitForSelector('h1', { timeout: 10000 });
    const allocationsTitle = await page.textContent('h1');
    console.log(`   Title: "${allocationsTitle}"`);

    await page.screenshot({ path: '/tmp/allocations.png', fullPage: true });
    console.log('   Screenshot saved: /tmp/allocations.png');

    // Test 6: Check API connectivity
    console.log('\n📍 Testing: API connectivity...');
    const apiResponse = await page.evaluate(async () => {
      try {
        const res = await fetch('http://localhost:5280/api/clients?pageNumber=1&pageSize=10');
        return { ok: res.ok, status: res.status };
      } catch (error) {
        return { ok: false, error: error.message };
      }
    });

    if (apiResponse.ok) {
      console.log(`✅ API is accessible (status: ${apiResponse.status})`);
    } else {
      console.error(`❌ API not accessible: ${apiResponse.error || apiResponse.status}`);
    }

    // Report errors
    if (errors.length > 0) {
      console.log('\n⚠️  Page Errors:');
      errors.forEach((err, i) => {
        console.log(`   ${i + 1}. ${err}`);
      });
    } else {
      console.log('\n✅ No page errors detected');
    }

    // Report console warnings/errors
    const warnings = consoleLogs.filter((log) => log.type === 'warning' || log.type === 'error');
    if (warnings.length > 0) {
      console.log('\n⚠️  Console Warnings/Errors:');
      warnings.forEach((log, i) => {
        console.log(`   ${i + 1}. [${log.type}] ${log.text}`);
      });
    } else {
      console.log('✅ No console warnings/errors');
    }

    console.log('\n✅ All tests completed successfully!');
  } catch (error) {
    console.error('\n❌ Test failed:', error.message);
    await page.screenshot({ path: '/tmp/error.png' });
    console.log('   Error screenshot saved: /tmp/error.png');
    throw error;
  } finally {
    await browser.close();
  }
}

testApplication().catch((error) => {
  console.error('Fatal error:', error);
  process.exit(1);
});
