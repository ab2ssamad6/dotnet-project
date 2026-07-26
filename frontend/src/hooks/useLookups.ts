import { useAsync } from './useAsync';
import { categoryService, trainerService } from '@/services';
import { fullName } from '@/utils/format';
import type { SelectOption } from '@/components/ui';

/** Fetches categories + trainers as select options for forms (single request each). */
export function useLookups() {
  const { data, loading, error } = useAsync(async () => {
    const [categories, trainers] = await Promise.all([
      categoryService.list({ page: 1, pageSize: 100 }),
      trainerService.list({ page: 1, pageSize: 100 }),
    ]);
    return {
      categories: categories.items,
      trainers: trainers.items,
      categoryOptions: categories.items.map<SelectOption>((c) => ({ value: c.id, label: c.name })),
      trainerOptions: trainers.items.map<SelectOption>((t) => ({
        value: t.id,
        label: fullName(t.firstName, t.lastName),
      })),
    };
  }, []);

  return {
    loading,
    error,
    categories: data?.categories ?? [],
    trainers: data?.trainers ?? [],
    categoryOptions: data?.categoryOptions ?? [],
    trainerOptions: data?.trainerOptions ?? [],
  };
}
