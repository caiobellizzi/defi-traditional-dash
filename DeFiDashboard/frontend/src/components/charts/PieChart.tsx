import {
  PieChart as RechartsPieChart,
  Pie,
  Cell,
  ResponsiveContainer,
  Legend,
  Tooltip,
} from 'recharts';

/**
 * Color palette for pie chart segments
 */
const COLORS = [
  '#3b82f6', // blue-500
  '#10b981', // green-500
  '#f59e0b', // amber-500
  '#ef4444', // red-500
  '#8b5cf6', // violet-500
  '#ec4899', // pink-500
  '#14b8a6', // teal-500
  '#f97316', // orange-500
];

/**
 * Data point for pie chart
 */
export interface PieChartDataPoint {
  name: string;
  value: number;
  color?: string;
}

/**
 * Props for PieChart component
 */
interface PieChartProps {
  data: PieChartDataPoint[];
  height?: number;
  showLegend?: boolean;
  showTooltip?: boolean;
  valueFormatter?: (value: number) => string;
}

/**
 * Reusable Pie Chart component using Recharts
 */
export function PieChart({
  data,
  height = 300,
  showLegend = true,
  showTooltip = true,
  valueFormatter = (value: number) => value.toFixed(2),
}: PieChartProps) {
  const CustomTooltip = ({ active, payload }: any) => {
    if (active && payload && payload.length) {
      return (
        <div className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg p-3 shadow-lg">
          <p className="text-sm font-medium text-gray-900 dark:text-white">
            {payload[0].name}
          </p>
          <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
            Value: {valueFormatter(payload[0].value)}
          </p>
          <p className="text-xs text-gray-500 dark:text-gray-500 mt-1">
            {payload[0].payload.percent}%
          </p>
        </div>
      );
    }
    return null;
  };

  const CustomLegend = ({ payload }: any) => {
    return (
      <ul className="flex flex-wrap justify-center gap-4 mt-4">
        {payload.map((entry: any, index: number) => (
          <li key={`legend-${index}`} className="flex items-center gap-2">
            <span
              className="w-3 h-3 rounded-full"
              style={{ backgroundColor: entry.color }}
            />
            <span className="text-sm text-gray-700 dark:text-gray-300">
              {entry.value}
            </span>
          </li>
        ))}
      </ul>
    );
  };

  // Calculate total for percentage
  const total = data.reduce((sum, item) => sum + item.value, 0);

  // Add percentage to data
  const dataWithPercent = data.map((item) => ({
    ...item,
    percent: total > 0 ? ((item.value / total) * 100).toFixed(1) : 0,
  }));

  if (data.length === 0) {
    return (
      <div
        className="flex items-center justify-center text-gray-400 dark:text-gray-500"
        style={{ height: `${height}px` }}
      >
        No data available
      </div>
    );
  }

  return (
    <ResponsiveContainer width="100%" height={height}>
      <RechartsPieChart>
        <Pie
          data={dataWithPercent}
          cx="50%"
          cy="50%"
          labelLine={false}
          label={({ percent }) => `${percent}%`}
          outerRadius={80}
          fill="#8884d8"
          dataKey="value"
        >
          {dataWithPercent.map((entry, index) => (
            <Cell
              key={`cell-${index}`}
              fill={entry.color || COLORS[index % COLORS.length]}
            />
          ))}
        </Pie>
        {showTooltip && <Tooltip content={<CustomTooltip />} />}
        {showLegend && <Legend content={<CustomLegend />} />}
      </RechartsPieChart>
    </ResponsiveContainer>
  );
}
