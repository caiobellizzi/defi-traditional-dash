import { chromium } from '@playwright/test';

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();

  // Set viewport for consistent screenshots
  await page.setViewportSize({ width: 1920, height: 1080 });

  console.log('Starting UI analysis...');

  const pages = [
    { path: '', name: 'dashboard' },
    { path: '/clients', name: 'clients' },
    { path: '/wallets', name: 'wallets' },
    { path: '/allocations', name: 'allocations' }
  ];

  for (const pageInfo of pages) {
    console.log(`Analyzing ${pageInfo.name} page...`);

    try {
      await page.goto(`http://localhost:5173${pageInfo.path}`, {
        waitUntil: 'networkidle',
        timeout: 10000
      });

      // Wait a bit for any animations/charts to render
      await page.waitForTimeout(2000);

      // Take full page screenshot
      await page.screenshot({
        path: `/tmp/ui-${pageInfo.name}.png`,
        fullPage: true
      });

      console.log(`✓ Captured ${pageInfo.name} page`);

      // Capture some basic metrics
      const metrics = await page.evaluate(() => {
        return {
          buttons: document.querySelectorAll('button').length,
          inputs: document.querySelectorAll('input').length,
          tables: document.querySelectorAll('table').length,
          charts: document.querySelectorAll('[class*="recharts"]').length,
          headings: {
            h1: document.querySelectorAll('h1').length,
            h2: document.querySelectorAll('h2').length,
            h3: document.querySelectorAll('h3').length
          },
          colors: Array.from(document.querySelectorAll('*')).map(el => {
            const style = window.getComputedStyle(el);
            return {
              bg: style.backgroundColor,
              color: style.color
            };
          }).filter(c => c.bg !== 'rgba(0, 0, 0, 0)').slice(0, 10)
        };
      });

      console.log(`  Metrics:`, JSON.stringify(metrics, null, 2));

    } catch (error) {
      console.error(`✗ Error capturing ${pageInfo.name}:`, error.message);
    }
  }

  // Capture mobile view of dashboard
  console.log('Analyzing mobile view...');
  await page.setViewportSize({ width: 375, height: 812 });
  await page.goto('http://localhost:5173', { waitUntil: 'networkidle' });
  await page.waitForTimeout(2000);
  await page.screenshot({
    path: `/tmp/ui-mobile-dashboard.png`,
    fullPage: true
  });
  console.log('✓ Captured mobile view');

  // Capture tablet view
  console.log('Analyzing tablet view...');
  await page.setViewportSize({ width: 768, height: 1024 });
  await page.goto('http://localhost:5173', { waitUntil: 'networkidle' });
  await page.waitForTimeout(2000);
  await page.screenshot({
    path: `/tmp/ui-tablet-dashboard.png`,
    fullPage: true
  });
  console.log('✓ Captured tablet view');

  await browser.close();
  console.log('\nScreenshots saved to /tmp/');
})();
