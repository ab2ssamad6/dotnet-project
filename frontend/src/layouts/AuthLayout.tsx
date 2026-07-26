import { Outlet } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Logo } from '@/components/common/Logo';

/** Split-screen shell for unauthenticated auth pages. */
export function AuthLayout() {
  return (
    <div className="flex min-h-screen">
      {/* Brand panel */}
      <div className="relative hidden w-1/2 flex-col justify-between overflow-hidden bg-brand-700 p-12 text-white lg:flex">
        <div className="absolute inset-0 opacity-20">
          <div className="absolute -left-20 -top-20 h-96 w-96 rounded-full bg-brand-400 blur-3xl" />
          <div className="absolute bottom-0 right-0 h-96 w-96 rounded-full bg-brand-900 blur-3xl" />
        </div>
        <div className="relative flex items-center gap-2.5">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-white/15">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
              <path d="M12 4 3 8l9 4 7-3.11V14h2V8L12 4z" fill="white" />
            </svg>
          </div>
          <span className="text-lg font-bold">LMS Platform</span>
        </div>
        <div className="relative">
          <h1 className="max-w-md text-4xl font-bold leading-tight">
            Learn, train and grow — all in one place.
          </h1>
          <p className="mt-4 max-w-md text-brand-100">
            A modern Learning Management System with training catalogs, progress tracking, assessments and an
            AI-powered trainer.
          </p>
          <div className="mt-8 flex gap-8">
            <Stat value="Trainings" label="Rich course catalog" />
            <Stat value="Assessments" label="Quizzes & exams" />
            <Stat value="AI Trainer" label="Interactive avatar" />
          </div>
        </div>
        <p className="relative text-sm text-brand-200">© {new Date().getFullYear()} LMS Platform</p>
      </div>

      {/* Form panel */}
      <div className="flex w-full flex-col items-center justify-center bg-slate-50 px-4 py-10 lg:w-1/2">
        <div className="mb-8 lg:hidden">
          <Logo />
        </div>
        <motion.div
          initial={{ opacity: 0, y: 12 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3 }}
          className="w-full max-w-md"
        >
          <Outlet />
        </motion.div>
      </div>
    </div>
  );
}

function Stat({ value, label }: { value: string; label: string }) {
  return (
    <div>
      <p className="text-lg font-semibold">{value}</p>
      <p className="text-xs text-brand-200">{label}</p>
    </div>
  );
}
