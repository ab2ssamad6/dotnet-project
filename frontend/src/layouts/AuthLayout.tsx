import type { ReactNode } from 'react';
import { Outlet } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Icons } from '@/components/ui';
import { Logo } from '@/components/common/Logo';

export function AuthLayout() {
  return (
    <div className="flex min-h-screen bg-canvas">
      <aside className="relative hidden w-[46%] max-w-[620px] flex-col justify-between overflow-hidden bg-shell p-12 text-white lg:flex">
        <div
          className="pointer-events-none absolute inset-0"
          style={{
            background:
              'radial-gradient(32rem 22rem at 8% 4%, rgb(63 173 164 / 0.30), transparent 65%), radial-gradient(28rem 20rem at 95% 92%, rgb(235 196 104 / 0.16), transparent 65%)',
          }}
          aria-hidden
        />
        <div className="pointer-events-none absolute inset-0 bg-grain opacity-[0.06] mix-blend-overlay" aria-hidden />

        <div className="relative">
          <Logo tone="light" />
        </div>

        <div className="relative">
          <p className="text-[11px] font-bold uppercase tracking-[0.2em] text-gold-300">Learning Studio</p>
          <h1 className="mt-5 max-w-lg font-display text-[44px] font-semibold leading-[1.08] tracking-[-0.02em]">
            Build training people actually finish.
          </h1>
          <p className="mt-5 max-w-md text-[15px] leading-relaxed text-white/65">
            Author curricula, publish to a polished catalog, track every learner's progress and let an AI avatar tutor
            fill the gaps between sessions.
          </p>

          <ul className="mt-10 space-y-4">
            <Feature icon={<Icons.layers size={17} />} title="Structured curricula">
              Trainings, modules and activities in one place
            </Feature>
            <Feature icon={<Icons.target size={17} />} title="Assessments that score themselves">
              Timed quizzes and exams with instant results
            </Feature>
            <Feature icon={<Icons.sparkle size={17} />} title="AI avatar trainer">
              Conversational tutoring on any published course
            </Feature>
          </ul>
        </div>

        <p className="relative text-xs text-white/40">© {new Date().getFullYear()} LMS Learning Studio</p>
      </aside>

      <div className="canvas-glow flex w-full flex-col items-center justify-center px-4 py-12 lg:flex-1">
        <div className="mb-8 lg:hidden">
          <Logo />
        </div>
        <motion.div
          initial={{ opacity: 0, y: 14 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.34, ease: [0.16, 1, 0.3, 1] }}
          className="w-full max-w-[440px]"
        >
          <Outlet />
        </motion.div>
        <p className="mt-8 max-w-sm text-center text-[11.5px] leading-relaxed text-ink-400">
          Need a hand? Reach out to your workspace administrator to have an account created or reset.
        </p>
      </div>
    </div>
  );
}

function Feature({ icon, title, children }: { icon: ReactNode; title: string; children: ReactNode }) {
  return (
    <li className="flex items-start gap-3.5">
      <span className="mt-0.5 flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-white/[0.08] text-brand-300 ring-1 ring-inset ring-white/10">
        {icon}
      </span>
      <span>
        <span className="block text-sm font-bold text-white">{title}</span>
        <span className="mt-0.5 block text-[13px] leading-snug text-white/55">{children}</span>
      </span>
    </li>
  );
}
