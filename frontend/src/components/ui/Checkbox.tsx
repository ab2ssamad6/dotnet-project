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
    <div className="flex items-start gap-2.5">
      <input
        ref={ref}
        id={cbId}
        type="checkbox"
        className={cn(
          'focus-ring mt-0.5 h-4 w-4 rounded border-slate-300 text-brand-600 accent-brand-600',
          className,
        )}
        {...props}
      />
      {(label || description) && (
        <div className="text-sm leading-tight">
          {label && (
            <label htmlFor={cbId} className="font-medium text-slate-700">
              {label}
            </label>
          )}
          {description && <p className="text-xs text-slate-500">{description}</p>}
        </div>
      )}
    </div>
  );
});
