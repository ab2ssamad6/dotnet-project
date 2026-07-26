import { describe, expect, it } from 'vitest';
import { formatDuration, fullName, initials } from './format';

describe('formatDuration', () => {
  it('formats minutes under an hour', () => {
    expect(formatDuration(45)).toBe('45m');
  });
  it('formats whole hours', () => {
    expect(formatDuration(120)).toBe('2h');
  });
  it('formats hours and minutes', () => {
    expect(formatDuration(90)).toBe('1h 30m');
  });
  it('handles empty values', () => {
    expect(formatDuration(0)).toBe('—');
    expect(formatDuration(null)).toBe('—');
  });
});

describe('fullName', () => {
  it('joins first and last', () => {
    expect(fullName('Jane', 'Doe')).toBe('Jane Doe');
  });
  it('falls back to a dash', () => {
    expect(fullName(null, null)).toBe('—');
  });
});

describe('initials', () => {
  it('builds uppercase initials', () => {
    expect(initials('jane', 'doe')).toBe('JD');
  });
  it('falls back to a question mark', () => {
    expect(initials('', '')).toBe('?');
  });
});
