import { forwardRef, useId, useState, type InputHTMLAttributes, type ReactNode } from 'react';
import { cn } from '@/utils/cn';
import { Icons } from './Icon';
import { fieldBase, fieldError, fieldHint, fieldLabel, fieldTone } from './field';

export interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  hint?: string;
  leftIcon?: ReactNode;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input(
  { label, error, hint, leftIcon, className, id, type = 'text', ...props },
  ref,
) {
  const autoId = useId();
  const inputId = id ?? autoId;
  const [show, setShow] = useState(false);
  const isPassword = type === 'password';
  const resolvedType = isPassword && show ? 'text' : type;

  return (
    <div className="w-full">
      {label && (
        <label htmlFor={inputId} className={fieldLabel}>
          {label}
          {props.required && <span className="ml-0.5 text-brand-600">*</span>}
        </label>
      )}
      <div className="relative">
        {leftIcon && (
          <span className="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-ink-400">{leftIcon}</span>
        )}
        <input
          ref={ref}
          id={inputId}
          type={resolvedType}
          aria-invalid={!!error}
          className={cn(
            fieldBase,
            fieldTone(!!error),
            'h-11 px-3.5',
            leftIcon ? 'pl-10' : undefined,
            isPassword && 'pr-11',
            className,
          )}
          {...props}
        />
        {isPassword && (
          <button
            type="button"
            onClick={() => setShow((s) => !s)}
            className="focus-ring absolute right-2 top-1/2 -translate-y-1/2 rounded-md p-1.5 text-ink-400 transition-colors hover:bg-ink-100 hover:text-ink-700"
            tabIndex={-1}
            aria-label={show ? 'Hide password' : 'Show password'}
          >
            {show ? <Icons.eyeOff size={18} /> : <Icons.eye size={18} />}
          </button>
        )}
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
