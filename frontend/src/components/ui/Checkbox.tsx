import { forwardRef, useId, type InputHTMLAttributes } from 'react';
import { cn } from '@/utils/cn';

export interface CheckboxProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  description?: string;
}

export const Checkbox = forwardRef<HTMLInputElement, CheckboxProps>(function Checkbox(
  { label, description, className, id, ...props },
  ref,
) {
  const autoId = useId();
  const cbId = id ?? autoId;
  return (
    <div className="flex items-start gap-3">
      <input
        ref={ref}
        id={cbId}
        type="checkbox"
        className={cn(
          'focus-ring mt-px h-[18px] w-[18px] shrink-0 cursor-pointer rounded-[5px] border-ink-300 text-brand-700 accent-brand-700 transition-colors',
          className,
        )}
        {...props}
      />
      {(label || description) && (
        <div className="text-sm leading-snug">
          {label && (
            <label htmlFor={cbId} className="cursor-pointer font-semibold text-ink-800">
              {label}
            </label>
          )}
          {description && <p className="mt-0.5 text-xs leading-relaxed text-ink-500">{description}</p>}
        </div>
      )}
    </div>
  );
});
