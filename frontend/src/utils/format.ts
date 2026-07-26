/** Formatting helpers shared across the app. */

export function formatDate(value?: string | null, opts?: Intl.DateTimeFormatOptions): string {
  if (!value) return '—';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '—';
  return date.toLocaleDateString(undefined, opts ?? { year: 'numeric', month: 'short', day: 'numeric' });
}

export function formatDateTime(value?: string | null): string {
  if (!value) return '—';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '—';
  return date.toLocaleString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

/** Relative time e.g. "3 hours ago". */
export function timeAgo(value?: string | null): string {
  if (!value) return '';
  const date = new Date(value).getTime();
  if (Number.isNaN(date)) return '';
  const seconds = Math.round((Date.now() - date) / 1000);
  const ranges: [number, Intl.RelativeTimeFormatUnit][] = [
    [60, 'second'],
    [3600, 'minute'],
    [86400, 'hour'],
    [604800, 'day'],
    [2629800, 'week'],
    [31557600, 'month'],
    [Infinity, 'year'],
  ];
  const rtf = new Intl.RelativeTimeFormat(undefined, { numeric: 'auto' });
  let unitSeconds = 1;
  for (const [limit, unit] of ranges) {
    if (Math.abs(seconds) < limit) {
      const value = Math.round(-seconds / unitSeconds);
      return rtf.format(value, unit);
    }
    unitSeconds = limit;
  }
  return '';
}

/** Convert minutes to a readable duration like "1h 30m". */
export function formatDuration(minutes?: number | null): string {
  if (!minutes || minutes <= 0) return '—';
  const h = Math.floor(minutes / 60);
  const m = minutes % 60;
  if (h === 0) return `${m}m`;
  if (m === 0) return `${h}h`;
  return `${h}h ${m}m`;
}

export function fullName(first?: string | null, last?: string | null): string {
  return [first, last].filter(Boolean).join(' ').trim() || '—';
}

export function initials(first?: string | null, last?: string | null): string {
  const f = (first ?? '').trim()[0] ?? '';
  const l = (last ?? '').trim()[0] ?? '';
  return (f + l).toUpperCase() || '?';
}
