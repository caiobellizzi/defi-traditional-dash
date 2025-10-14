import { chromium } from 'playwright';

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();

  console.log('Navigating to http://localhost:5173/...');
  await page.goto('http://localhost:5173/', { waitUntil: 'domcontentloaded', timeout: 10000 });
  await page.waitForTimeout(2000); // Wait 2 seconds for React to render

  console.log('Page title:', await page.title());
  console.log('Page URL:', page.url());

  // Get page content
  const bodyText = await page.locator('body').textContent();
  console.log('Body text:', bodyText);

  // Get HTML structure
  const rootDiv = await page.locator('#root').innerHTML();
  console.log('Root div content:', rootDiv);

  // Check console errors
  page.on('console', msg => console.log('Browser console:', msg.text()));
  page.on('pageerror', error => console.log('Page error:', error.message));

  // Take screenshot
  await page.screenshot({ path: 'frontend-screenshot.png', fullPage: true });
  console.log('Screenshot saved to frontend-screenshot.png');

  // Get all text elements
  const allText = await page.evaluate(() => document.body.innerText);
  console.log('All visible text:', allText);

  // Check for React mounting
  const hasReact = await page.evaluate(() => {
    return {
      hasRoot: !!document.getElementById('root'),
      rootHasChildren: document.getElementById('root')?.children.length > 0,
      rootHTML: document.getElementById('root')?.innerHTML
    };
  });
  console.log('React mounting check:', JSON.stringify(hasReact, null, 2));

  await browser.close();
})();
