import '@testing-library/jest-dom/vitest';

// jsdom lacks crypto.randomUUID in some environments; provide a fallback.
if (!('randomUUID' in crypto)) {
  Object.defineProperty(crypto, 'randomUUID', {
    value: () => '00000000-0000-4000-8000-000000000000',
  });
}
