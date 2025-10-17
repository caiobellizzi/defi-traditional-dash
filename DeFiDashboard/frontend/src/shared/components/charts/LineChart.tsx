import {
  LineChart as RechartsLineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';

/**
 * Data point for line chart
 */
export interface LineChartDataPoint {
  [key: string]: string | number;
}

/**
 * Line series configuration
 */
export interface LineSeries {
  dataKey: string;
  name: string;
  color?: string;
  strokeWidth?: number;
}

/**
 * Props for LineChart component
 */
interface LineChartProps {
  data: LineChartDataPoint[];
  lines: LineSeries[];
  xAxisKey: string;
  height?: number;
  showGrid?: boolean;
  showLegend?: boolean;
  showTooltip?: boolean;
  valueFormatter?: (value: number) => string;
  xAxisFormatter?: (value: any) => string;
}

/**
 * Default colors for multiple lines
 */
const DEFAULT_COLORS = [
  '#3b82f6', // blue-500
  '#10b981', // green-500
  '#f59e0b', // amber-500
  '#ef4444', // red-500
  '#8b5cf6', // violet-500
];

/**
 * Reusable Line Chart component using Recharts
 */
export function LineChart({
  data,
  lines,
  xAxisKey,
  height = 300,
  showGrid = true,
  showLegend = true,
  showTooltip = true,
  valueFormatter = (value: number) => value.toFixed(2),
  xAxisFormatter,
}: LineChartProps) {
  const CustomTooltip = ({ active, payload, label }: any) => {
    if (active && payload && payload.length) {
      return (
        <div className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg p-3 shadow-lg">
          <p className="text-sm font-medium text-gray-900 dark:text-white mb-2">
            {xAxisFormatter ? xAxisFormatter(label) : label}
          </p>
          {payload.map((entry: any, index: number) => (
            <p
              key={index}
              className="text-sm text-gray-600 dark:text-gray-400"
              style={{ color: entry.color }}
            >
              {entry.name}: {valueFormatter(entry.value)}
            </p>
          ))}
        </div>
      );
    }
    return null;
  };

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
      <RechartsLineChart
        data={data}
        margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
      >
        {showGrid && (
          <CartesianGrid
            strokeDasharray="3 3"
            className="stroke-gray-200 dark:stroke-gray-700"
          />
        )}
        <XAxis
          dataKey={xAxisKey}
          tickFormatter={xAxisFormatter}
          className="text-xs text-gray-600 dark:text-gray-400"
        />
        <YAxis
          tickFormatter={valueFormatter}
          className="text-xs text-gray-600 dark:text-gray-400"
        />
        {showTooltip && <Tooltip content={<CustomTooltip />} />}
        {showLegend && (
          <Legend
            wrapperStyle={{
              paddingTop: '20px',
            }}
          />
        )}
        {lines.map((line, index) => (
          <Line
            key={line.dataKey}
            type="monotone"
            dataKey={line.dataKey}
            name={line.name}
            stroke={line.color || DEFAULT_COLORS[index % DEFAULT_COLORS.length]}
            strokeWidth={line.strokeWidth || 2}
            dot={{ r: 4 }}
            activeDot={{ r: 6 }}
          />
        ))}
      </RechartsLineChart>
    </ResponsiveContainer>
  );
}
