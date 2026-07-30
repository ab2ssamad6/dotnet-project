import { Button, Icons } from '@/components/ui';
import type { ApiError } from '@/api/errors';

export function ErrorState({ error, onRetry }: { error: ApiError | Error | null; onRetry?: () => void }) {
  const message = error?.message ?? 'Something went wrong on our side.';
  return (
    <div className="surface flex flex-col items-center justify-center px-6 py-16 text-center">
      <div className="relative mb-5">
        <span className="absolute inset-0 -m-2 rounded-2xl bg-rose-100/60 blur-md" aria-hidden />
        <span className="relative flex h-14 w-14 items-center justify-center rounded-2xl border border-rose-200/70 bg-white text-rose-600 shadow-card">
          <Icons.alert size={24} />
        </span>
      </div>
      <h3 className="text-base font-bold tracking-[-0.01em] text-ink-900">We couldn't load this</h3>
      <p className="mt-1.5 max-w-sm text-sm leading-relaxed text-ink-500">{message}</p>
      {onRetry && (
        <Button variant="outline" className="mt-6" leftIcon={<Icons.refresh size={16} />} onClick={onRetry}>
          Try again
        </Button>
      )}
    </div>
  );
}
