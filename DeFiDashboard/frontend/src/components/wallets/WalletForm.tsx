import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Input from '../ui/Input';
import Textarea from '../ui/Textarea';
import Button from '../ui/Button';

/**
 * Available blockchain networks
 */
const SUPPORTED_CHAINS = [
  'ethereum',
  'bsc',
  'polygon',
  'arbitrum',
  'optimism',
  'avalanche',
  'fantom',
  'base',
] as const;

/**
 * Zod validation schema for wallet form
 */
const walletSchema = z.object({
  walletAddress: z
    .string()
    .min(1, 'Wallet address is required')
    .regex(/^0x[a-fA-F0-9]{40}$/, 'Invalid Ethereum wallet address format'),
  label: z
    .string()
    .min(2, 'Label must be at least 2 characters')
    .max(100, 'Label must be less than 100 characters'),
  supportedChains: z
    .array(z.string())
    .min(1, 'Select at least one blockchain network'),
  notes: z.string().optional().or(z.literal('')),
});

type WalletFormData = z.infer<typeof walletSchema>;

interface WalletFormProps {
  onSubmit: (data: WalletFormData) => void;
  onCancel: () => void;
  isLoading?: boolean;
}

const WalletForm = ({ onSubmit, onCancel, isLoading = false }: WalletFormProps) => {
  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors },
  } = useForm<WalletFormData>({
    resolver: zodResolver(walletSchema),
    defaultValues: {
      walletAddress: '',
      label: '',
      supportedChains: [],
      notes: '',
    },
  });

  const selectedChains = watch('supportedChains') || [];

  const toggleChain = (chain: string) => {
    const currentChains = selectedChains;
    const newChains = currentChains.includes(chain)
      ? currentChains.filter((c) => c !== chain)
      : [...currentChains, chain];
    setValue('supportedChains', newChains, { shouldValidate: true });
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <Input
        label="Wallet Address"
        placeholder="0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb"
        error={errors.walletAddress?.message}
        helperText="Must be a valid Ethereum address (0x...)"
        required
        {...register('walletAddress')}
      />

      <Input
        label="Label"
        placeholder="Main Custody Wallet"
        error={errors.label?.message}
        helperText="A friendly name to identify this wallet"
        required
        {...register('label')}
      />

      <div className="w-full">
        <label className="block text-sm font-medium text-gray-700 mb-2">
          Supported Chains <span className="text-red-500">*</span>
        </label>
        <div className="flex flex-wrap gap-2">
          {SUPPORTED_CHAINS.map((chain) => {
            const isSelected = selectedChains.includes(chain);
            return (
              <button
                key={chain}
                type="button"
                onClick={() => toggleChain(chain)}
                className={`px-3 py-1.5 text-sm font-medium rounded-full transition-colors ${
                  isSelected
                    ? 'bg-blue-600 text-white hover:bg-blue-700'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                }`}
              >
                {chain.charAt(0).toUpperCase() + chain.slice(1)}
              </button>
            );
          })}
        </div>
        {errors.supportedChains?.message && (
          <p className="mt-2 text-sm text-red-600">
            {errors.supportedChains.message}
          </p>
        )}
        <p className="mt-2 text-xs text-gray-500">
          Select all blockchain networks this wallet will monitor
        </p>
      </div>

      <Textarea
        label="Notes"
        placeholder="Additional notes about this wallet"
        rows={4}
        error={errors.notes?.message}
        helperText="Optional: Any additional information or context"
        {...register('notes')}
      />

      <div className="flex justify-end gap-3 pt-4">
        <Button type="button" variant="ghost" onClick={onCancel} disabled={isLoading}>
          Cancel
        </Button>
        <Button type="submit" isLoading={isLoading}>
          Add Wallet
        </Button>
      </div>
    </form>
  );
};

export default WalletForm;
