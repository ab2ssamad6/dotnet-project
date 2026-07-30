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
    <Card className="p-8 text-center">
      {status === 'verifying' && (
        <>
          <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-brand-100 text-brand-600">
            <Spinner size={26} />
          </div>
          <h1 className="text-xl font-bold text-slate-900">Verifying your email…</h1>
        </>
      )}

      {status === 'success' && (
        <>
          <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-emerald-100 text-emerald-600">
            <Icons.check size={28} />
          </div>
          <h1 className="text-xl font-bold text-slate-900">Email verified</h1>
          <p className="mt-2 text-sm text-slate-500">Your email address has been confirmed. You can now sign in.</p>
          <Link to="/login" className="mt-6 inline-block w-full">
            <Button fullWidth>Continue to sign in</Button>
          </Link>
        </>
      )}

      {(status === 'idle' || status === 'error') && (
        <div className="text-left">
          <h1 className="text-center text-2xl font-bold text-slate-900">Verify email</h1>
          <p className="mt-1 text-center text-sm text-slate-500">
            Enter the user id and token from your verification email.
          </p>
          {status === 'error' && (
            <div className="mt-4 rounded-lg bg-rose-50 px-3 py-2 text-sm text-rose-600">{message}</div>
          )}
          <form
            className="mt-6 space-y-4"
            onSubmit={(e) => {
              e.preventDefault();
              void verify(userId, token);
            }}
          >
            <Input label="User id" value={userId} onChange={(e) => setUserId(e.target.value)} required />
            <Input label="Token" value={token} onChange={(e) => setToken(e.target.value)} required />
            <Button type="submit" fullWidth size="lg" disabled={!userId || !token}>
              Verify email
            </Button>
          </form>
          <p className="mt-6 text-center text-sm text-slate-500">
            <Link to="/login" className="font-medium text-brand-600 hover:underline">
              Back to sign in
            </Link>
          </p>
        </div>
      )}
    </Card>
  );
}
