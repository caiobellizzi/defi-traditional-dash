import { Link, useLocation } from 'react-router-dom';

/**
 * Main navigation component
 */

interface NavItem {
  path: string;
  label: string;
}

const navItems: NavItem[] = [
  { path: '/dashboard', label: 'Dashboard' },
  { path: '/clients', label: 'Clients' },
  { path: '/wallets', label: 'Wallets' },
  { path: '/allocations', label: 'Allocations' },
];

export function Navigation() {
  const location = useLocation();

  return (
    <nav className="bg-white border-b shadow-sm">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16">
          {/* Logo/Brand */}
          <div className="flex-shrink-0">
            <h1 className="text-xl font-bold text-gray-900">
              DeFi Custody Manager
            </h1>
          </div>

          {/* Navigation Links */}
          <div className="flex space-x-8">
            {navItems.map((item) => (
              <Link
                key={item.path}
                to={item.path}
                className={`py-4 px-3 border-b-2 text-sm font-medium transition-colors ${
                  location.pathname === item.path
                    ? 'border-blue-500 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                {item.label}
              </Link>
            ))}
          </div>
        </div>
      </div>
    </nav>
  );
}
