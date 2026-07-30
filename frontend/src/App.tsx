import { Suspense } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { AppLayout } from '@/layouts/AppLayout';
import { AuthLayout } from '@/layouts/AuthLayout';
import { ProtectedRoute } from '@/routes/ProtectedRoute';
import { RoleRoute } from '@/routes/RoleRoute';
import { FullPageLoader } from '@/components/common/FullPageLoader';
import { lazyPage } from '@/utils/lazyWithRetry';
import { Role } from '@/types';

// Auth pages are small and needed first — import eagerly.
import { LoginPage } from '@/pages/auth/LoginPage';
import { RegisterPage } from '@/pages/auth/RegisterPage';
import { ForgotPasswordPage } from '@/pages/auth/ForgotPasswordPage';
import { ResetPasswordPage } from '@/pages/auth/ResetPasswordPage';
import { VerifyEmailPage } from '@/pages/auth/VerifyEmailPage';
import { ForbiddenPage, NotFoundPage } from '@/pages/ErrorPages';

// App pages are code-split so each route loads on demand. lazyPage (not React's bare lazy) retries
// the import and falls back to one reload, so a network blip or a redeploy mid-session cannot take
// the whole app down through the error boundary.
const DashboardPage = lazyPage(() => import('@/pages/dashboard/DashboardPage'), 'DashboardPage');
const CategoriesPage = lazyPage(() => import('@/pages/categories/CategoriesPage'), 'CategoriesPage');
const TrainingsPage = lazyPage(() => import('@/pages/trainings/TrainingsPage'), 'TrainingsPage');
const TrainingDetailPage = lazyPage(() => import('@/pages/trainings/TrainingDetailPage'), 'TrainingDetailPage');
const TrainersPage = lazyPage(() => import('@/pages/trainers/TrainersPage'), 'TrainersPage');
const StudentsPage = lazyPage(() => import('@/pages/dashboard/StudentsPage'), 'StudentsPage');
const CatalogPage = lazyPage(() => import('@/pages/student/CatalogPage'), 'CatalogPage');
const MyLearningPage = lazyPage(() => import('@/pages/student/MyLearningPage'), 'MyLearningPage');
const LearningPage = lazyPage(() => import('@/pages/student/LearningPage'), 'LearningPage');
const AITrainerPage = lazyPage(() => import('@/pages/ai/AITrainerPage'), 'AITrainerPage');
const SettingsPage = lazyPage(() => import('@/pages/settings/SettingsPage'), 'SettingsPage');
const ProfilePage = lazyPage(() => import('@/pages/settings/ProfilePage'), 'ProfilePage');

const CONTENT = [Role.Administrator, Role.Trainer];

export default function App() {
  return (
    <Routes>
      {/* Public auth routes */}
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />
        <Route path="/verify-email" element={<VerifyEmailPage />} />
      </Route>

      {/* Authenticated app */}
      <Route element={<ProtectedRoute />}>
        <Route
          element={
            <AppLayout />
          }
        >
          <Route
            path="/"
            element={<Navigate to="/dashboard" replace />}
          />
          <Route path="/dashboard" element={<Lazy><DashboardPage /></Lazy>} />
          <Route path="/profile" element={<Lazy><ProfilePage /></Lazy>} />
          <Route path="/settings" element={<Lazy><SettingsPage /></Lazy>} />
          <Route path="/ai-trainer" element={<Lazy><AITrainerPage /></Lazy>} />

          {/* Student portal */}
          <Route element={<RoleRoute roles={[Role.Student]} />}>
            <Route path="/catalog" element={<Lazy><CatalogPage /></Lazy>} />
            <Route path="/my-learning" element={<Lazy><MyLearningPage /></Lazy>} />
            <Route path="/my-learning/:trainingId" element={<Lazy><LearningPage /></Lazy>} />
          </Route>

          {/* Content management (Admin + Trainer) */}
          <Route element={<RoleRoute roles={CONTENT} />}>
            <Route path="/categories" element={<Lazy><CategoriesPage /></Lazy>} />
            <Route path="/trainings" element={<Lazy><TrainingsPage /></Lazy>} />
            <Route path="/trainings/:id" element={<Lazy><TrainingDetailPage /></Lazy>} />
            <Route path="/trainers" element={<Lazy><TrainersPage /></Lazy>} />
          </Route>

          {/* Admin only */}
          <Route element={<RoleRoute roles={[Role.Administrator]} />}>
            <Route path="/students" element={<Lazy><StudentsPage /></Lazy>} />
          </Route>
        </Route>
      </Route>

      {/* Errors */}
      <Route path="/403" element={<ForbiddenPage />} />
      <Route path="/404" element={<NotFoundPage />} />
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}

/** Suspense wrapper for lazily-loaded route pages. */
function Lazy({ children }: { children: React.ReactNode }) {
  return <Suspense fallback={<FullPageLoader />}>{children}</Suspense>;
}
