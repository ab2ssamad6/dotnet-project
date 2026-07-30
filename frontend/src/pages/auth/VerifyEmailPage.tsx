import { useEffect, useRef, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { Button, Card, Icons, Input, Spinner } from '@/components/ui';
import { authService } from '@/services';
import { toApiError } from '@/api/errors';

type Status = 'idle' | 'verifying' | 'success' | 'error';

export function VerifyEmailPage() {
  const [params] = useSearchParams();
  const [status, setStatus] = useState<Status>('idle');
  const [message, setMessage] = useState('');
  const [userId, setUserId] = useState(params.get('userId') ?? '');
  const [token, setToken] = useState(params.get('token') ?? '');
  const attempted = useRef(false);

  const verify = async (uid: string, tok: string) => {
    setStatus('verifying');
    try {
      await authService.verifyEmail({ userId: uid, token: tok });
      setStatus('success');
    } catch (err) {
      setStatus('error');
      setMessage(toApiError(err).message);
    }
  };

  useEffect(() => {
    const uid = params.get('userId');
    const tok = params.get('token');
    if (uid && tok && !attempted.current) {
      attempted.current = true;
      void verify(uid, tok);
    }
  }, [params]);

  return (
    <Card className="p-8 text-center shadow-raised sm:p-9">
      {status === 'verifying' && (
        <>
          <div className="mx-auto mb-5 flex h-14 w-14 items-center justify-center rounded-2xl bg-brand-gradient text-white shadow-raised">
            <Spinner size={24} />
          </div>
          <h1 className="font-display text-[23px] font-semibold tracking-[-0.02em] text-ink-900">
            Confirming your email…
          </h1>
          <p className="mt-2 text-sm text-ink-500">This only takes a moment.</p>
        </>
      )}

      {status === 'success' && (
        <>
          <div className="relative mx-auto mb-5 w-fit">
            <span className="absolute inset-0 -m-2 rounded-2xl bg-green-100/70 blur-md" aria-hidden />
            <span className="relative flex h-14 w-14 items-center justify-center rounded-2xl border border-green-200/70 bg-white text-green-600 shadow-card">
              <Icons.shield size={24} />
            </span>
          </div>
          <h1 className="font-display text-[23px] font-semibold tracking-[-0.02em] text-ink-900">Email confirmed</h1>
          <p className="mt-3 text-sm leading-relaxed text-ink-500">
            Your address is verified. Sign in to reach your dashboard.
          </p>
          <Link to="/login" className="mt-7 inline-block w-full">
            <Button fullWidth rightIcon={<Icons.arrowRight size={16} />}>
              Continue to sign in
            </Button>
          </Link>
        </>
      )}

      {(status === 'idle' || status === 'error') && (
        <div className="text-left">
          <p className="eyebrow text-center">Email verification</p>
          <h1 className="mt-2 text-center font-display text-[27px] font-semibold tracking-[-0.02em] text-ink-900">
            Verify your email
          </h1>
          <p className="mt-2 text-center text-sm text-ink-500">
            Copy the user id and token from your verification email.
          </p>
          {status === 'error' && (
            <div className="mt-5 flex items-start gap-2 rounded-xl border border-rose-200/70 bg-rose-50 px-3.5 py-3 text-[13px] text-rose-700">
              <Icons.alert size={16} className="mt-px shrink-0" />
              <p>{message}</p>
            </div>
          )}
          <form
            className="mt-6 space-y-4"
            onSubmit={(e) => {
              e.preventDefault();
              void verify(userId, token);
            }}
          >
            <Input label="User id" value={userId} onChange={(e) => setUserId(e.target.value)} required />
            <Input label="Verification token" value={token} onChange={(e) => setToken(e.target.value)} required />
            <Button type="submit" fullWidth size="lg" disabled={!userId || !token}>
              Verify email
            </Button>
          </form>
          <p className="mt-7 text-center text-sm">
            <Link to="/login" className="font-semibold text-brand-700 transition-colors hover:text-brand-800">
              Back to sign in
            </Link>
          </p>
        </div>
      )}
    </Card>
  );
}
