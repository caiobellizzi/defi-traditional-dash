import { Navigation } from './Navigation';
import type { ReactNode } from 'react';

/**
 * Common page layout wrapper with navigation
 */

export interface PageLayoutProps {
  children: ReactNode;
  title?: string;
  subtitle?: string;
  actions?: ReactNode;
}

export function PageLayout({ children, title, subtitle, actions }: PageLayoutProps) {
  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 transition-colors duration-200">
      <Navigation />
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {(title || subtitle || actions) && (
          <div className="mb-6">
            <div className="flex items-center justify-between">
              <div>
                {title && (
                  <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
                    {title}
                  </h1>
                )}
                {subtitle && (
                  <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
                    {subtitle}
                  </p>
                )}
              </div>
              {actions && <div className="flex-shrink-0">{actions}</div>}
            </div>
          </div>
        )}
        {children}
      </main>
    </div>
  );
}
