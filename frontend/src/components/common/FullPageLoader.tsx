import { Spinner } from '@/components/ui';

export function FullPageLoader({ label = 'Loading…' }: { label?: string }) {
  return (
    <div className="flex h-full min-h-[60vh] w-full flex-col items-center justify-center gap-4">
      <div className="relative">
        <span className="absolute inset-0 -m-1.5 animate-halo rounded-2xl bg-brand-300/40 blur-md" aria-hidden />
        <div className="relative flex h-12 w-12 items-center justify-center rounded-2xl bg-brand-gradient text-white shadow-raised">
          <Spinner size={21} />
        </div>
      </div>
      <p className="text-[13px] font-semibold text-ink-500">{label}</p>
    </div>
  );
}
