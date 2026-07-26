import { Button, Icons } from '@/components/ui';
import type { ApiError } from '@/api/errors';

export function ErrorState({ error, onRetry }: { error: ApiError | Error | null; onRetry?: () => void }) {
  const message = error?.message ?? 'Something went wrong.';
  return (
    <div className="flex flex-col items-center justify-center rounded-xl border border-slate-200 bg-white px-6 py-14 text-center">
      <div className="mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-rose-100 text-rose-600">
        <Icons.alert size={26} />
      </div>
      <h3 className="text-base font-semibold text-slate-800">Unable to load data</h3>
      <p className="mt-1 max-w-sm text-sm text-slate-500">{message}</p>
      {onRetry && (
        <Button variant="outline" className="mt-5" leftIcon={<Icons.refresh size={16} />} onClick={onRetry}>
          Try again
        </Button>
      )}
    </div>
  );
}
