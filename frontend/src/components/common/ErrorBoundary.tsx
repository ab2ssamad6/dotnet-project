import { Component, type ErrorInfo, type ReactNode } from 'react';
import { Button } from '@/components/ui';

interface Props {
  children: ReactNode;
}
interface State {
  error: Error | null;
}

/** App-level error boundary so a render crash shows a recoverable screen, not a blank page. */
export class ErrorBoundary extends Component<Props, State> {
  state: State = { error: null };

  static getDerivedStateFromError(error: Error): State {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    // In production this is where you'd forward to an error-tracking service.
    console.error('Unhandled UI error:', error, info);
  }

  render() {
    if (this.state.error) {
      // A failed dynamic import is a network/stale-build problem, not a bug — say so, because the
      // generic "something went wrong" wording sends people hunting for a crash that never happened.
      const isLoadFailure = /dynamically imported module|Importing a module script failed|Loading chunk/i.test(
        this.state.error.message,
      );
      return (
        <div className="flex min-h-screen flex-col items-center justify-center bg-slate-50 px-6 text-center">
          <h1 className="text-3xl font-bold text-slate-900">
            {isLoadFailure ? "Couldn't load this page" : 'Something went wrong'}
          </h1>
          <p className="mt-2 max-w-md text-sm text-slate-500">
            {isLoadFailure
              ? 'Part of the app failed to download. Check your connection and reload.'
              : 'An unexpected error occurred. Reloading the page usually fixes it.'}
          </p>
          <Button className="mt-6" onClick={() => window.location.reload()}>
            Reload page
          </Button>
        </div>
      );
    }
    return this.props.children;
  }
}
