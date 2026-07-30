import { cn } from '@/utils/cn';
import { initials as toInitials } from '@/utils/format';

interface AvatarProps {
  src?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  name?: string | null;
  size?: 'xs' | 'sm' | 'md' | 'lg';
  className?: string;
}

const sizes = {
  xs: 'h-7 w-7 text-[10px] rounded-lg',
  sm: 'h-9 w-9 text-[11px] rounded-lg',
  md: 'h-11 w-11 text-sm rounded-xl',
  lg: 'h-16 w-16 text-lg rounded-2xl',
};

export function Avatar({ src, firstName, lastName, name, size = 'md', className }: AvatarProps) {
  const label = name ? name.split(' ') : [firstName, lastName];
  const text = toInitials(label[0], label[1]);
  if (src) {
    return (
      <img
        src={src}
        alt={name ?? `${firstName ?? ''} ${lastName ?? ''}`.trim()}
        className={cn('object-cover ring-1 ring-ink-900/10', sizes[size], className)}
      />
    );
  }
  return (
    <span
      className={cn(
        'inline-flex shrink-0 items-center justify-center bg-brand-gradient font-bold uppercase tracking-wide text-white ring-1 ring-ink-900/10',
        sizes[size],
        className,
      )}
      aria-hidden
    >
      {text}
    </span>
  );
}
