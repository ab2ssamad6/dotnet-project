import { useAuth } from '@/hooks/useAuth';
import { AdminDashboard } from './AdminDashboard';
import { TrainerDashboard } from './TrainerDashboard';
import { StudentDashboard } from './StudentDashboard';

export function DashboardPage() {
  const { user, isAdmin, isTrainer } = useAuth();
  const firstName = user?.firstName;

  if (isAdmin) return <AdminDashboard firstName={firstName} />;
  if (isTrainer) return <TrainerDashboard firstName={firstName} />;
  return <StudentDashboard firstName={firstName} />;
}
