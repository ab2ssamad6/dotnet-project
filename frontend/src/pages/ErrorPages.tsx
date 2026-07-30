import { Link } from 'react-router-dom';
import { Button, Icons } from '@/components/ui';
import { Logo } from '@/components/common/Logo';

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
    <div className="canvas-glow flex min-h-screen flex-col items-center justify-center bg-canvas px-6 text-center">
      <div className="mb-10">
        <Logo />
      </div>
      <p className="font-display text-[88px] font-semibold leading-none tracking-[-0.04em] text-brand-700/90">
        {code}
      </p>
      <h1 className="mt-5 font-display text-[26px] font-semibold tracking-[-0.02em] text-ink-900">{title}</h1>
      <p className="mt-3 max-w-md text-sm leading-relaxed text-ink-500">{description}</p>
      <div className="mt-8 flex flex-wrap justify-center gap-3">
        <Link to="/dashboard">
          <Button rightIcon={<Icons.arrowRight size={16} />}>Back to dashboard</Button>
        </Link>
        <Link to="/login">
          <Button variant="outline">Sign in again</Button>
        </Link>
      </div>
    </div>
  );
}

export function NotFoundPage() {
  return (
    <ErrorTemplate
      code="404"
      title="We can't find that page"
      description="The link may be outdated, or the page has been moved somewhere else in the workspace."
    />
  );
}

export function ForbiddenPage() {
  return (
    <ErrorTemplate
      code="403"
      title="You don't have access here"
      description="This area is limited to another role. Ask an administrator to grant you access if you think that's a mistake."
    />
  );
}
