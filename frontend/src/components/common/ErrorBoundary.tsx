import { Component, type ErrorInfo, type ReactNode } from 'react';
import { Button, Icons } from '@/components/ui';

interface Props {
  children: ReactNode;
}
interface State {
  error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
  state: State = { error: null };

  static getDerivedStateFromError(error: Error): State {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('Unhandled UI error:', error, info);
  }

  render() {
    if (this.state.error) {
      const isLoadFailure = /dynamically imported module|Importing a module script failed|Loading chunk/i.test(
        this.state.error.message,
      );
      return (
        <div className="canvas-glow flex min-h-screen flex-col items-center justify-center bg-canvas px-6 text-center">
          <span className="mb-6 flex h-14 w-14 items-center justify-center rounded-2xl border border-ink-200/70 bg-white text-amber-600 shadow-card">
            <Icons.alert size={24} />
          </span>
          <h1 className="font-display text-[30px] font-semibold tracking-[-0.02em] text-ink-900">
            {isLoadFailure ? 'This page failed to load' : 'Something went wrong'}
          </h1>
          <p className="mt-3 max-w-md text-sm leading-relaxed text-ink-500">
            {isLoadFailure
              ? 'Part of the app never finished downloading. Check your connection, then reload to try again.'
              : 'An unexpected error interrupted the app. Reloading the page usually clears it.'}
          </p>
          <Button className="mt-7" leftIcon={<Icons.refresh size={16} />} onClick={() => window.location.reload()}>
            Reload page
          </Button>
        </div>
      );
    }
    return this.props.children;
  }
}
