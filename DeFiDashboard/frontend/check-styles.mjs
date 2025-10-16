import { chromium } from '@playwright/test';

async function checkStyles() {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();

  const logs = [];
  const errors = [];

  page.on('console', (msg) => logs.push({ type: msg.type(), text: msg.text() }));
  page.on('pageerror', (error) => errors.push(error.message));

  await page.goto('http://localhost:5173', { waitUntil: 'networkidle' });

  // Check if stylesheets are loaded
  const stylesheets = await page.evaluate(() => {
    return Array.from(document.styleSheets).map(sheet => ({
      href: sheet.href,
      rules: sheet.cssRules ? sheet.cssRules.length : 0,
      disabled: sheet.disabled
    }));
  });

  console.log('📊 Loaded Stylesheets:');
  stylesheets.forEach(sheet => {
    console.log(`   ${sheet.href || 'inline'} - ${sheet.rules} rules${sheet.disabled ? ' (disabled)' : ''}`);
  });

  // Check computed styles on navigation
  const navStyles = await page.evaluate(() => {
    const nav = document.querySelector('nav');
    if (!nav) return null;
    const styles = window.getComputedStyle(nav);
    return {
      backgroundColor: styles.backgroundColor,
      borderBottom: styles.borderBottom,
      padding: styles.padding,
      display: styles.display,
    };
  });

  console.log('\n🎨 Navigation Computed Styles:', navStyles);

  // Check if Tailwind classes are applied
  const hasClasses = await page.evaluate(() => {
    const nav = document.querySelector('nav');
    return {
      hasNav: !!nav,
      classes: nav?.className || 'no classes',
      childCount: nav?.children.length || 0
    };
  });

  console.log('\n📝 Navigation Classes:', hasClasses);

  // Check network requests for CSS
  const cssRequests = [];
  page.on('request', request => {
    if (request.url().includes('.css') || request.resourceType() === 'stylesheet') {
      cssRequests.push({ url: request.url(), method: request.method() });
    }
  });

  await page.reload({ waitUntil: 'networkidle' });

  console.log('\n🌐 CSS Network Requests:');
  if (cssRequests.length === 0) {
    console.log('   ⚠️  No CSS requests found!');
  } else {
    cssRequests.forEach(req => console.log(`   ${req.method} ${req.url}`));
  }

  // Check console errors
  const cssErrors = logs.filter(log =>
    log.type === 'error' && (log.text.includes('css') || log.text.includes('style'))
  );

  if (cssErrors.length > 0) {
    console.log('\n❌ CSS-related errors:');
    cssErrors.forEach(err => console.log(`   ${err.text}`));
  }

  await browser.close();
}

checkStyles().catch(console.error);
