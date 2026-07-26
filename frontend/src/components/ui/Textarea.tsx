import { forwardRef, useId, type TextareaHTMLAttributes } from 'react';
import { cn } from '@/utils/cn';

export interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string;
  error?: string;
  hint?: string;
}

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(function Textarea(
  { label, error, hint, className, id, rows = 4, ...props },
  ref,
) {
  const autoId = useId();
  const textId = id ?? autoId;
  return (
    <div className="w-full">
      {label && (
        <label htmlFor={textId} className="mb-1.5 block text-sm font-medium text-slate-700">
          {label}
          {props.required && <span className="ml-0.5 text-rose-500">*</span>}
        </label>
      )}
      <textarea
        ref={ref}
        id={textId}
        rows={rows}
        aria-invalid={!!error}
        className={cn(
          'focus-ring w-full rounded-lg border bg-white px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 transition-colors disabled:bg-slate-50',
          error ? 'border-rose-400 focus-visible:ring-rose-500' : 'border-slate-300 hover:border-slate-400',
          className,
        )}
        {...props}
      />
      {error ? (
        <p className="mt-1 text-xs text-rose-600">{error}</p>
      ) : hint ? (
        <p className="mt-1 text-xs text-slate-500">{hint}</p>
      ) : null}
    </div>
  );
});
