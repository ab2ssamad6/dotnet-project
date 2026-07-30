import { lazy, type ComponentType } from 'react';

type Factory<T> = () => Promise<{ default: T }>;

const RELOAD_FLAG = 'lms:chunk-reloaded';

export function lazyWithRetry<T extends ComponentType<unknown>>(factory: Factory<T>, retries = 2) {
  return lazy(async () => {
    for (let attempt = 0; ; attempt++) {
      try {
        const mod = await factory();
        sessionStorage.removeItem(RELOAD_FLAG);
        return mod;
      } catch (error) {
        if (attempt < retries) {
          await new Promise((resolve) => setTimeout(resolve, 300 * 2 ** attempt));
          continue;
        }
        if (!sessionStorage.getItem(RELOAD_FLAG)) {
          sessionStorage.setItem(RELOAD_FLAG, '1');
          window.location.reload();
          await new Promise(() => {});
        }
        throw error;
      }
    }
  });
}

export function lazyPage<K extends string>(
  loader: () => Promise<Record<K, ComponentType<unknown>>>,
  name: K,
) {
  return lazyWithRetry(async () => ({ default: (await loader())[name] }));
}
