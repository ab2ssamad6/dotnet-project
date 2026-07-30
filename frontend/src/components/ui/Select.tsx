import { forwardRef, useId, type SelectHTMLAttributes } from 'react';
import { cn } from '@/utils/cn';
import { Icons } from './Icon';
import { fieldBase, fieldError, fieldHint, fieldLabel, fieldTone } from './field';

export interface SelectOption {
  value: string | number;
  label: string;
}

export interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  error?: string;
  hint?: string;
  options: SelectOption[];
  placeholder?: string;
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(function Select(
  { label, error, hint, options, placeholder, className, id, ...props },
  ref,
) {
  const autoId = useId();
  const selectId = id ?? autoId;
  return (
    <div className="w-full">
      {label && (
        <label htmlFor={selectId} className={fieldLabel}>
          {label}
          {props.required && <span className="ml-0.5 text-brand-600">*</span>}
        </label>
      )}
      <div className="relative">
        <select
          ref={ref}
          id={selectId}
          aria-invalid={!!error}
          className={cn(fieldBase, fieldTone(!!error), 'h-11 cursor-pointer appearance-none pl-3.5 pr-10', className)}
          {...props}
        >
          {placeholder && <option value="">{placeholder}</option>}
          {options.map((o) => (
            <option key={o.value} value={o.value}>
              {o.label}
            </option>
          ))}
        </select>
        <span className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-ink-400">
          <Icons.chevronDown size={17} />
        </span>
      </div>
      {error ? (
        <p className={fieldError}>
          <Icons.alert size={13} /> {error}
        </p>
      ) : hint ? (
        <p className={fieldHint}>{hint}</p>
      ) : null}
    </div>
  );
});
