import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Input from '@/shared/components/ui/Input';
import Textarea from '@/shared/components/ui/Textarea';
import Button from '@/shared/components/ui/Button';
import type { ClientDto } from '@/shared/types/api.types';

/**
 * Zod validation schema for client form
 */
const clientSchema = z.object({
  name: z
    .string()
    .min(1, 'Name is required')
    .max(200, 'Name must be less than 200 characters'),
  email: z
    .string()
    .min(1, 'Email is required')
    .email('Invalid email address')
    .max(200, 'Email must be less than 200 characters'),
  document: z
    .string()
    .max(50, 'Document must be less than 50 characters')
    .optional()
    .or(z.literal('')),
  phoneNumber: z
    .string()
    .max(20, 'Phone number must be less than 20 characters')
    .optional()
    .or(z.literal('')),
  status: z.enum(['Active', 'Inactive', 'Suspended']).optional(),
  notes: z.string().optional().or(z.literal('')),
});

type ClientFormData = z.infer<typeof clientSchema>;

interface ClientFormProps {
  onSubmit: (data: ClientFormData) => void;
  onCancel: () => void;
  initialData?: ClientDto;
  isLoading?: boolean;
}

const ClientForm = ({
  onSubmit,
  onCancel,
  initialData,
  isLoading = false,
}: ClientFormProps) => {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ClientFormData>({
    resolver: zodResolver(clientSchema),
    defaultValues: initialData
      ? {
          name: initialData.name || '',
          email: initialData.email || '',
          document: initialData.document || '',
          phoneNumber: initialData.phoneNumber || '',
          status: (initialData.status as 'Active' | 'Inactive' | 'Suspended') || 'Active',
          notes: initialData.notes || '',
        }
      : {
          name: '',
          email: '',
          document: '',
          phoneNumber: '',
          status: 'Active' as const,
          notes: '',
        },
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <Input
        label="Name"
        placeholder="John Doe"
        error={errors.name?.message}
        required
        {...register('name')}
      />

      <Input
        label="Email"
        type="email"
        placeholder="john@example.com"
        error={errors.email?.message}
        required
        {...register('email')}
      />

      <Input
        label="Document"
        placeholder="CPF, CNPJ, or other identification"
        error={errors.document?.message}
        helperText="Optional: Tax ID or other identification number"
        {...register('document')}
      />

      <Input
        label="Phone Number"
        type="tel"
        placeholder="+1 (555) 123-4567"
        error={errors.phoneNumber?.message}
        {...register('phoneNumber')}
      />

      {initialData && (
        <div className="w-full">
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Status
          </label>
          <select
            className="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            {...register('status')}
          >
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
            <option value="Suspended">Suspended</option>
          </select>
          {errors.status?.message && (
            <p className="mt-1 text-sm text-red-600">{errors.status.message}</p>
          )}
        </div>
      )}

      <Textarea
        label="Notes"
        placeholder="Additional notes or comments about this client"
        rows={4}
        error={errors.notes?.message}
        {...register('notes')}
      />

      <div className="flex justify-end gap-3 pt-4">
        <Button type="button" variant="ghost" onClick={onCancel} disabled={isLoading}>
          Cancel
        </Button>
        <Button type="submit" isLoading={isLoading}>
          {initialData ? 'Update Client' : 'Create Client'}
        </Button>
      </div>
    </form>
  );
};

export default ClientForm;
