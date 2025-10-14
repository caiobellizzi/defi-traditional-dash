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
    <div className="bg-white rounded-lg border p-6 hover:shadow-lg transition-shadow">
      <div className="flex items-start space-x-4">
        <div className="text-blue-600 flex-shrink-0">{icon}</div>
        <div className="flex-1 min-w-0">
          <h3 className="text-lg font-semibold text-gray-900 mb-1">
            {title}
          </h3>
          <p className="text-sm text-gray-500 mb-4">{description}</p>
          <Link
            to={linkTo}
            className="inline-flex items-center text-sm font-medium text-blue-600 hover:text-blue-700"
          >
            {linkText}
            <svg
              className="ml-1 w-4 h-4"
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
