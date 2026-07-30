import { useEffect, useState } from 'react';
import { Input } from './Input';
import { Icons } from './Icon';

export function SearchInput({
  value,
  onSearch,
  placeholder = 'Search…',
  delay = 350,
  className,
}: {
  value: string;
  onSearch: (value: string) => void;
  placeholder?: string;
  delay?: number;
  className?: string;
}) {
  const [local, setLocal] = useState(value);

  useEffect(() => {
    setLocal(value);
  }, [value]);

  useEffect(() => {
    if (local === value) return;
    const t = setTimeout(() => onSearch(local), delay);
    return () => clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [local, delay]);

  return (
    <div className={className}>
      <div className="relative">
        <Input
          value={local}
          onChange={(e) => setLocal(e.target.value)}
          placeholder={placeholder}
          leftIcon={<Icons.search size={17} />}
          aria-label="Search"
          className={local ? 'pr-10' : undefined}
        />
        {local && (
          <button
            type="button"
            onClick={() => setLocal('')}
            className="focus-ring absolute right-2 top-1/2 -translate-y-1/2 rounded-md p-1.5 text-ink-400 transition-colors hover:bg-ink-100 hover:text-ink-700"
            aria-label="Clear search"
          >
            <Icons.close size={15} />
          </button>
        )}
      </div>
    </div>
  );
}
