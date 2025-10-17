import { Link } from 'react-router-dom';

/**
 * Quick action card component for dashboard shortcuts
 */

interface QuickActionCardProps {
  title: string;
  description: string;
  icon: React.ReactNode;
  linkTo: string;
  linkText: string;
}

export function QuickActionCard({
  title,
  description,
  icon,
  linkTo,
  linkText,
}: QuickActionCardProps) {
  return (
    <div className="group bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 hover:shadow-card-hover dark:hover:shadow-card-dark-hover transition-all duration-300 hover:border-blue-300 dark:hover:border-blue-600">
      <div className="flex items-start space-x-4">
        <div className="text-blue-600 dark:text-blue-400 flex-shrink-0 group-hover:scale-110 transition-transform duration-300">{icon}</div>
        <div className="flex-1 min-w-0">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-1">
            {title}
          </h3>
          <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">{description}</p>
          <Link
            to={linkTo}
            className="inline-flex items-center text-sm font-medium text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 group/link"
          >
            {linkText}
            <svg
              className="ml-1 w-4 h-4 group-hover/link:translate-x-1 transition-transform duration-200"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 5l7 7-7 7"
              />
            </svg>
          </Link>
        </div>
      </div>
    </div>
  );
}
