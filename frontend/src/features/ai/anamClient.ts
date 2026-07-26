/**
 * Thin, defensively-typed loader for the Anam.ai browser SDK. The SDK is loaded
 * at runtime from the CDN (kept out of the app bundle) exactly like the original
 * prototype in frontend/script.js. All interaction stays client-side; the backend
 * only mints the streaming session token.
 */

export interface AnamClient {
  streamToVideoElement: (elementId: string) => Promise<void>;
  stopStreaming: () => void;
  addListener: (event: string, handler: (payload: unknown) => void) => void;
  talk?: (text: string) => Promise<void> | void;
  muteInputAudio?: () => void;
  unmuteInputAudio?: () => void;
  [key: string]: unknown;
}

interface AnamModule {
  createClient: (sessionToken: string) => AnamClient;
  AnamEvent?: Record<string, string>;
}

const SDK_URL = 'https://esm.sh/@anam-ai/js-sdk@latest';

let modulePromise: Promise<AnamModule> | null = null;

export async function loadAnam(): Promise<AnamModule> {
  modulePromise ??= import(/* @vite-ignore */ SDK_URL) as Promise<AnamModule>;
  return modulePromise;
}

/** Common event names, resolved from the SDK when available with sensible fallbacks. */
export function resolveEvents(mod: AnamModule) {
  const e = mod.AnamEvent ?? {};
  return {
    SESSION_READY: e.SESSION_READY ?? 'SESSION_READY',
    CONNECTION_CLOSED: e.CONNECTION_CLOSED ?? 'CONNECTION_CLOSED',
    MESSAGE_HISTORY_UPDATED: e.MESSAGE_HISTORY_UPDATED ?? 'MESSAGE_HISTORY_UPDATED',
    MESSAGE_STREAM_EVENT_RECEIVED: e.MESSAGE_STREAM_EVENT_RECEIVED ?? 'MESSAGE_STREAM_EVENT_RECEIVED',
  };
}
