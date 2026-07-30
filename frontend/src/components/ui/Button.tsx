import { forwardRef, type ButtonHTMLAttributes, type ReactNode } from 'react';
import { cn } from '@/utils/cn';
import { Spinner } from './Spinner';

type Variant = 'primary' | 'secondary' | 'outline' | 'ghost' | 'soft' | 'danger';
type Size = 'sm' | 'md' | 'lg' | 'icon';

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  size?: Size;
  loading?: boolean;
  leftIcon?: ReactNode;
  rightIcon?: ReactNode;
  fullWidth?: boolean;
}

const variants: Record<Variant, string> = {
  primary:
    'bg-brand-700 text-white shadow-sm hover:bg-brand-800 active:bg-brand-900 disabled:bg-brand-700/40 disabled:shadow-none',
  secondary:
    'bg-ink-900 text-ink-50 shadow-sm hover:bg-ink-800 active:bg-ink-950 disabled:bg-ink-900/40 disabled:shadow-none',
  outline:
    'border border-ink-200 bg-white text-ink-700 shadow-card hover:border-ink-300 hover:bg-ink-50 active:bg-ink-100 disabled:opacity-55',
  ghost: 'text-ink-600 hover:bg-ink-100 hover:text-ink-900 active:bg-ink-200/70 disabled:opacity-55',
  soft: 'bg-brand-50 text-brand-800 hover:bg-brand-100 active:bg-brand-200/70 disabled:opacity-55',
  danger: 'bg-rose-600 text-white shadow-sm hover:bg-rose-700 active:bg-rose-800 disabled:bg-rose-600/40',
};

const sizes: Record<Size, string> = {
  sm: 'h-9 px-3 text-[13px] gap-1.5',
  md: 'h-10 px-4 text-sm gap-2',
  lg: 'h-12 px-6 text-[15px] gap-2.5',
  icon: 'h-10 w-10 justify-center',
};

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(function Button(
  { variant = 'primary', size = 'md', loading, leftIcon, rightIcon, fullWidth, className, children, disabled, ...props },
  ref,
) {
  return (
    <button
      ref={ref}
      disabled={disabled || loading}
      className={cn(
        'focus-ring inline-flex select-none items-center whitespace-nowrap rounded-lg font-semibold tracking-[-0.01em]',
        'transition-[background-color,border-color,color,box-shadow,transform] duration-150',
        'active:translate-y-px disabled:cursor-not-allowed disabled:active:translate-y-0',
        variants[variant],
        sizes[size],
        fullWidth && 'w-full justify-center',
        className,
      )}
      {...props}
    >
      {loading ? <Spinner size={size === 'lg' ? 20 : 16} /> : leftIcon}
      {children}
      {!loading && rightIcon}
    </button>
  );
});
