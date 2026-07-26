import { Link } from 'react-router-dom';
import { Button } from '@/components/ui';

function ErrorTemplate({
  code,
  title,
  description,
}: {
  code: string;
  title: string;
  description: string;
}) {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-slate-50 px-6 text-center">
      <p className="text-7xl font-black tracking-tight text-brand-600">{code}</p>
      <h1 className="mt-4 text-2xl font-bold text-slate-900">{title}</h1>
      <p className="mt-2 max-w-md text-sm text-slate-500">{description}</p>
      <div className="mt-8 flex gap-3">
        <Link to="/dashboard">
          <Button>Back to dashboard</Button>
        </Link>
        <Link to="/login">
          <Button variant="outline">Sign in</Button>
        </Link>
      </div>
    </div>
  );
}

export function NotFoundPage() {
  return (
    <ErrorTemplate
      code="404"
      title="Page not found"
      description="The page you're looking for doesn't exist or may have been moved."
    />
  );
}

export function ForbiddenPage() {
  return (
    <ErrorTemplate
      code="403"
      title="Access denied"
      description="You don't have permission to view this page. Contact an administrator if you believe this is an error."
    />
  );
}
