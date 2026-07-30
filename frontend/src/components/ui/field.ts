/** Shared visual language for form controls (input, textarea, select). */
export const fieldLabel = 'mb-1.5 block text-[13px] font-semibold tracking-[-0.01em] text-ink-700';

export const fieldBase =
  'focus-ring w-full rounded-lg border bg-white text-sm text-ink-900 shadow-[inset_0_1px_2px_rgb(29_27_24_/_0.04)] ' +
  'transition-colors placeholder:text-ink-400 disabled:cursor-not-allowed disabled:bg-ink-50 disabled:text-ink-400';

export const fieldTone = (hasError?: boolean) =>
  hasError
    ? 'border-rose-300 bg-rose-50/40 focus-visible:ring-rose-400/70'
    : 'border-ink-200 hover:border-ink-300';

export const fieldHint = 'mt-1.5 text-xs text-ink-500';
export const fieldError = 'mt-1.5 flex items-center gap-1 text-xs font-medium text-rose-600';
