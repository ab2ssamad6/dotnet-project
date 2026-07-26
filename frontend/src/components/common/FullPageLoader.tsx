import { Spinner } from '@/components/ui';

export function FullPageLoader({ label = 'Loading…' }: { label?: string }) {
  return (
    <div className="flex h-full min-h-[60vh] w-full flex-col items-center justify-center gap-3 text-slate-500">
      <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-brand-600 text-white shadow-lg">
        <Spinner size={22} />
      </div>
      <p className="text-sm font-medium">{label}</p>
    </div>
  );
}
