import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';

export function RoleRoute({ roles }: { roles: string[] }) {
  const { hasRole } = useAuth();
  if (!hasRole(...roles)) return <Navigate to="/403" replace />;
  return <Outlet />;
}
