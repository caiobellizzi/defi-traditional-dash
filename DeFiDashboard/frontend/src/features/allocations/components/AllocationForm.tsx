import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useClients } from '@/features/clients/hooks/useClients';
import { useWallets } from '@/features/wallets/hooks/useWallets';
import Textarea from '@/shared/components/ui/Textarea';
import Button from '@/shared/components/ui/Button';

/**
 * Zod validation schema for allocation form
 */
const allocationSchema = z
  .object({
    clientId: z.string().min(1, 'Client is required'),
    assetType: z.enum(['Wallet', 'TradFiAccount']),
    assetId: z.string().min(1, 'Asset is required'),
    allocationType: z.enum(['Percentage', 'FixedAmount']),
    allocationValue: z
      .number()
      .positive('Allocation value must be positive'),
    startDate: z.string().min(1, 'Start date is required'),
    notes: z.string().optional().or(z.literal('')),
  })
  .refine(
    (data) => {
      // If allocation type is Percentage, value must be between 0-100
      if (data.allocationType === 'Percentage') {
        return data.allocationValue > 0 && data.allocationValue <= 100;
      }
      return true;
    },
    {
      message: 'Percentage must be between 0 and 100',
      path: ['allocationValue'],
    }
  );

type AllocationFormData = z.infer<typeof allocationSchema>;

interface AllocationFormProps {
  onSubmit: (data: AllocationFormData) => void;
  onCancel: () => void;
  isLoading?: boolean;
}

const AllocationForm = ({
  onSubmit,
  onCancel,
  isLoading = false,
}: AllocationFormProps) => {
  const {
    register,
    handleSubmit,
    watch,
    control,
    formState: { errors },
  } = useForm<AllocationFormData>({
    resolver: zodResolver(allocationSchema),
    defaultValues: {
      clientId: '',
      assetType: 'Wallet',
      assetId: '',
      allocationType: 'Percentage',
      allocationValue: 0,
      startDate: new Date().toISOString().split('T')[0],
      notes: '',
    },
  });

  // Fetch clients and wallets for dropdowns
  const { data: clientsData, isLoading: clientsLoading } = useClients({
    pageNumber: 1,
    pageSize: 1000,
  });
  const { data: wallets, isLoading: walletsLoading } = useWallets();

  const assetType = watch('assetType');
  const allocationType = watch('allocationType');

  // Get available assets based on assetType
  const getAssetOptions = () => {
    if (assetType === 'Wallet') {
      return wallets || [];
    }
    // For TradFiAccount, we'd fetch accounts here
    return [];
  };

  const assets = getAssetOptions();

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      {/* Client Selection */}
      <div className="w-full">
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Client <span className="text-red-500">*</span>
        </label>
        <select
          className="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          disabled={clientsLoading || isLoading}
          {...register('clientId')}
        >
          <option value="">Select a client</option>
          {clientsData?.items.map((client) => (
            <option key={client.id} value={client.id}>
              {client.name} ({client.email})
            </option>
          ))}
        </select>
        {errors.clientId?.message && (
          <p className="mt-1 text-sm text-red-600">{errors.clientId.message}</p>
        )}
      </div>

      {/* Asset Type Selection */}
      <div className="w-full">
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Asset Type <span className="text-red-500">*</span>
        </label>
        <select
          className="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          disabled={isLoading}
          {...register('assetType')}
        >
          <option value="Wallet">Wallet (DeFi)</option>
          <option value="TradFiAccount" disabled>
            Traditional Account (Coming Soon)
          </option>
        </select>
        {errors.assetType?.message && (
          <p className="mt-1 text-sm text-red-600">{errors.assetType.message}</p>
        )}
      </div>

      {/* Asset Selection */}
      <div className="w-full">
        <label className="block text-sm font-medium text-gray-700 mb-1">
          {assetType === 'Wallet' ? 'Wallet' : 'Account'}{' '}
          <span className="text-red-500">*</span>
        </label>
        <select
          className="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          disabled={walletsLoading || isLoading}
          {...register('assetId')}
        >
          <option value="">
            Select a {assetType === 'Wallet' ? 'wallet' : 'account'}
          </option>
          {assetType === 'Wallet' &&
            assets.map((wallet) => (
              <option key={wallet.id} value={wallet.id}>
                {wallet.label} ({wallet.walletAddress.slice(0, 10)}...)
              </option>
            ))}
        </select>
        {errors.assetId?.message && (
          <p className="mt-1 text-sm text-red-600">{errors.assetId.message}</p>
        )}
      </div>

      {/* Allocation Type Selection */}
      <div className="w-full">
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Allocation Type <span className="text-red-500">*</span>
        </label>
        <select
          className="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          disabled={isLoading}
          {...register('allocationType')}
        >
          <option value="Percentage">Percentage</option>
          <option value="FixedAmount">Fixed Amount (USD)</option>
        </select>
        {errors.allocationType?.message && (
          <p className="mt-1 text-sm text-red-600">
            {errors.allocationType.message}
          </p>
        )}
      </div>

      {/* Allocation Value */}
      <div className="w-full">
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Allocation Value <span className="text-red-500">*</span>
        </label>
        <div className="relative">
          <Controller
            name="allocationValue"
            control={control}
            render={({ field }) => (
              <input
                type="number"
                step={allocationType === 'Percentage' ? '0.01' : '0.01'}
                min="0"
                max={allocationType === 'Percentage' ? '100' : undefined}
                className="w-full px-3 py-2 pr-12 border border-gray-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                disabled={isLoading}
                {...field}
                onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
              />
            )}
          />
          <div className="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
            <span className="text-gray-500 text-sm font-medium">
              {allocationType === 'Percentage' ? '%' : 'USD'}
            </span>
          </div>
        </div>
        {errors.allocationValue?.message && (
          <p className="mt-1 text-sm text-red-600">
            {errors.allocationValue.message}
          </p>
        )}
        <p className="mt-1 text-xs text-gray-500">
          {allocationType === 'Percentage'
            ? 'Percentage of total asset value (0-100%)'
            : 'Fixed amount in USD'}
        </p>
      </div>

      {/* Start Date */}
      <div className="w-full">
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Start Date <span className="text-red-500">*</span>
        </label>
        <input
          type="date"
          className="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          disabled={isLoading}
          {...register('startDate')}
        />
        {errors.startDate?.message && (
          <p className="mt-1 text-sm text-red-600">{errors.startDate.message}</p>
        )}
      </div>

      {/* Notes */}
      <Textarea
        label="Notes"
        placeholder="Additional notes about this allocation"
        rows={3}
        error={errors.notes?.message}
        helperText="Optional: Any additional information or context"
        {...register('notes')}
      />

      <div className="flex justify-end gap-3 pt-4">
        <Button type="button" variant="ghost" onClick={onCancel} disabled={isLoading}>
          Cancel
        </Button>
        <Button type="submit" isLoading={isLoading}>
          Create Allocation
        </Button>
      </div>
    </form>
  );
};

export default AllocationForm;
