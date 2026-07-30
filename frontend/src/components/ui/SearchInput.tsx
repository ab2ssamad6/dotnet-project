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
      <Input
        value={local}
        onChange={(e) => setLocal(e.target.value)}
        placeholder={placeholder}
        leftIcon={<Icons.search size={18} />}
        aria-label="Search"
      />
    </div>
  );
}
