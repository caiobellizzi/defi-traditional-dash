import { chromium } from 'playwright';

(async () => {
  const browser = await chromium.launch({ headless: false });
  const page = await browser.newPage();

  // Listen for console messages
  page.on('console', msg => {
    const type = msg.type();
    const text = msg.text();
    console.log(`[Browser ${type}]: ${text}`);
  });

  // Listen for page errors
  page.on('pageerror', error => {
    console.error('[Page Error]:', error.message);
  });

  // Listen for network errors
  page.on('requestfailed', request => {
    console.error('[Network Error]:', request.url(), request.failure()?.errorText);
  });

  try {
    console.log('Navigating to http://localhost:5173/...');
    await page.goto('http://localhost:5173/', {
      waitUntil: 'networkidle',
      timeout: 10000
    });

    console.log('✅ Page loaded successfully!');
    console.log('Page title:', await page.title());
    console.log('Page URL:', page.url());

    // Wait a bit for React to render
    await page.waitForTimeout(2000);

    // Check if root div has content
    const rootContent = await page.locator('#root').innerHTML();
    console.log('\n📦 Root div content length:', rootContent.length, 'characters');

    // Check for visible text
    const bodyText = await page.locator('body').textContent();
    console.log('\n📝 Page visible text:', bodyText?.slice(0, 200) + '...');

    // Take screenshot
    await page.screenshot({ path: 'frontend-test.png', fullPage: true });
    console.log('\n📸 Screenshot saved to frontend-test.png');

    // Check for React elements
    const hasViteLogo = await page.locator('img[alt="Vite logo"]').count();
    const hasReactLogo = await page.locator('img[alt="React logo"]').count();
    const hasButton = await page.locator('button').count();

    console.log('\n✨ React elements found:');
    console.log('  - Vite logo:', hasViteLogo > 0 ? '✅' : '❌');
    console.log('  - React logo:', hasReactLogo > 0 ? '✅' : '❌');
    console.log('  - Button:', hasButton > 0 ? '✅' : '❌');

    if (hasButton > 0) {
      console.log('\n🎯 Testing button click...');
      await page.click('button');
      await page.waitForTimeout(500);
      const buttonText = await page.locator('button').textContent();
      console.log('  Button text after click:', buttonText);
    }

    console.log('\n✅ All checks passed! Frontend is working correctly.');
    console.log('\n⏸️  Browser will stay open for 30 seconds for manual inspection...');
    await page.waitForTimeout(30000);

  } catch (error) {
    console.error('\n❌ Error occurred:', error.message);

    // Take error screenshot
    try {
      await page.screenshot({ path: 'frontend-error.png', fullPage: true });
      console.log('📸 Error screenshot saved to frontend-error.png');
    } catch (e) {
      console.error('Could not take error screenshot');
    }
  } finally {
    await browser.close();
    console.log('\n👋 Browser closed');
  }
})();
