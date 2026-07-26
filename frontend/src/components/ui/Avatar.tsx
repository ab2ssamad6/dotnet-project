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
  xs: 'h-7 w-7 text-[10px]',
  sm: 'h-9 w-9 text-xs',
  md: 'h-11 w-11 text-sm',
  lg: 'h-16 w-16 text-lg',
};

export function Avatar({ src, firstName, lastName, name, size = 'md', className }: AvatarProps) {
  const label = name ? name.split(' ') : [firstName, lastName];
  const text = toInitials(label[0], label[1]);
  if (src) {
    return (
      <img
        src={src}
        alt={name ?? `${firstName ?? ''} ${lastName ?? ''}`.trim()}
        className={cn('rounded-full object-cover ring-2 ring-white', sizes[size], className)}
      />
    );
  }
  return (
    <span
      className={cn(
        'inline-flex items-center justify-center rounded-full bg-brand-100 font-semibold text-brand-700 ring-2 ring-white',
        sizes[size],
        className,
      )}
      aria-hidden
    >
      {text}
    </span>
  );
}
