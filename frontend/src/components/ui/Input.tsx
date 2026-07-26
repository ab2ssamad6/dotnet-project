import { forwardRef, useId, useState, type InputHTMLAttributes, type ReactNode } from 'react';
import { cn } from '@/utils/cn';
import { Icons } from './Icon';

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
        <label htmlFor={inputId} className="mb-1.5 block text-sm font-medium text-slate-700">
          {label}
          {props.required && <span className="ml-0.5 text-rose-500">*</span>}
        </label>
      )}
      <div className="relative">
        {leftIcon && (
          <span className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400">{leftIcon}</span>
        )}
        <input
          ref={ref}
          id={inputId}
          type={resolvedType}
          aria-invalid={!!error}
          className={cn(
            'focus-ring h-10 w-full rounded-lg border bg-white px-3 text-sm text-slate-900 placeholder:text-slate-400 transition-colors disabled:cursor-not-allowed disabled:bg-slate-50',
            leftIcon ? 'pl-9' : '',
            isPassword && 'pr-10',
            error ? 'border-rose-400 focus-visible:ring-rose-500' : 'border-slate-300 hover:border-slate-400',
            className,
          )}
          {...props}
        />
        {isPassword && (
          <button
            type="button"
            onClick={() => setShow((s) => !s)}
            className="absolute right-2 top-1/2 -translate-y-1/2 rounded p-1 text-slate-400 hover:text-slate-600"
            tabIndex={-1}
            aria-label={show ? 'Hide password' : 'Show password'}
          >
            {show ? <Icons.eyeOff size={18} /> : <Icons.eye size={18} />}
          </button>
        )}
      </div>
      {error ? (
        <p className="mt-1 text-xs text-rose-600">{error}</p>
      ) : hint ? (
        <p className="mt-1 text-xs text-slate-500">{hint}</p>
      ) : null}
    </div>
  );
});
