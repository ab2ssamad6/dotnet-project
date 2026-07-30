import { lazy, type ComponentType } from 'react';

type Factory<T> = () => Promise<{ default: T }>;

/** Session flag so a missing chunk can trigger at most one reload — never a reload loop. */
const RELOAD_FLAG = 'lms:chunk-reloaded';

/**
 * A dynamic import fails for two mundane reasons that both used to take the whole app down through
 * the error boundary:
 *
 *  - a transient network blip (`ERR_NAME_NOT_RESOLVED`, offline for a moment, flaky mobile link) —
 *    retrying a moment later just works;
 *  - a redeploy while the tab was open: `index.html` in memory still points at the previous build's
 *    hashed filenames, which no longer exist, so every retry 404s. Only a reload fixes that, and it
 *    fixes it permanently.
 *
 * So: retry a few times with backoff, then fall back to a single guarded reload.
 */
export function lazyWithRetry<T extends ComponentType<unknown>>(factory: Factory<T>, retries = 2) {
  return lazy(async () => {
    for (let attempt = 0; ; attempt++) {
      try {
        const mod = await factory();
        // Loaded fine — clear the guard so a later redeploy may reload again.
        sessionStorage.removeItem(RELOAD_FLAG);
        return mod;
      } catch (error) {
        if (attempt < retries) {
          await new Promise((resolve) => setTimeout(resolve, 300 * 2 ** attempt));
          continue;
        }
        // Out of retries: assume a stale document and reload once.
        if (!sessionStorage.getItem(RELOAD_FLAG)) {
          sessionStorage.setItem(RELOAD_FLAG, '1');
          window.location.reload();
          // Block until the reload takes effect so React never renders the failure.
          await new Promise(() => {});
        }
        throw error;
      }
    }
  });
}

/** `lazyWithRetry` for the named-export pages in this codebase. */
export function lazyPage<K extends string>(
  loader: () => Promise<Record<K, ComponentType<unknown>>>,
  name: K,
) {
  return lazyWithRetry(async () => ({ default: (await loader())[name] }));
}
