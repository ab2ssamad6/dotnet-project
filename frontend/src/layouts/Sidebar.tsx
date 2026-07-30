import { Link, NavLink } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Icons } from '@/components/ui';
import { Logo } from '@/components/common/Logo';
import { visibleSections } from '@/constants/navigation';
import { useAuth } from '@/hooks/useAuth';
import { cn } from '@/utils/cn';

interface SidebarProps {
  mobileOpen: boolean;
  onCloseMobile: () => void;
}

const itemBase =
  'group relative flex items-center gap-3 rounded-lg px-3 py-2.5 text-[13.5px] font-semibold transition-colors duration-150';

export function Sidebar({ mobileOpen, onCloseMobile }: SidebarProps) {
  const { roles } = useAuth();
  const sections = visibleSections(roles);

  const nav = (
    <nav className="flex-1 space-y-7 overflow-y-auto px-4 py-6">
      {sections.map((section, i) => (
        <div key={section.heading ?? i}>
          {section.heading && (
            <p className="mb-2 px-3 text-[10.5px] font-bold uppercase tracking-[0.16em] text-white/35">
              {section.heading}
            </p>
          )}
          <div className="space-y-1">
            {section.items.map((item) => {
              const Icon = Icons[item.icon];
              return (
                <NavLink
                  key={item.to}
                  to={item.to}
                  onClick={onCloseMobile}
                  className={({ isActive }) =>
                    cn(
                      itemBase,
                      isActive
                        ? 'bg-white/[0.09] text-white shadow-[inset_0_1px_0_0_rgb(255_255_255_/_0.06)]'
                        : 'text-shell-muted hover:bg-white/[0.05] hover:text-white',
                    )
                  }
                >
                  {({ isActive }) => (
                    <>
                      <span
                        className={cn(
                          'absolute left-0 top-1/2 h-5 w-[3px] -translate-y-1/2 rounded-r-full bg-gold-300 transition-opacity',
                          isActive ? 'opacity-100' : 'opacity-0',
                        )}
                        aria-hidden
                      />
                      <Icon
                        size={18}
                        className={cn(
                          'shrink-0 transition-colors',
                          isActive ? 'text-brand-300' : 'text-white/45 group-hover:text-white/70',
                        )}
                      />
                      {item.label}
                    </>
                  )}
                </NavLink>
              );
            })}
          </div>
        </div>
      ))}
    </nav>
  );

  const footer = (
    <div className="border-t border-white/[0.07] p-4">
      <Link
        to="/ai-trainer"
        onClick={onCloseMobile}
        className="group flex items-start gap-3 rounded-xl bg-white/[0.05] p-3.5 ring-1 ring-inset ring-white/[0.08] transition-colors hover:bg-white/[0.08]"
      >
        <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-gold-gradient text-ink-900">
          <Icons.sparkle size={17} />
        </span>
        <span className="min-w-0">
          <span className="block text-[13px] font-bold text-white">Ask the AI Trainer</span>
          <span className="mt-0.5 block text-[11.5px] leading-snug text-white/50">
            Live avatar tutoring on any course
          </span>
        </span>
      </Link>
    </div>
  );

  const shell = 'relative flex flex-col bg-shell';
  const glow = (
    <span
      className="pointer-events-none absolute inset-x-0 top-0 h-64 opacity-70"
      style={{
        background:
          'radial-gradient(24rem 14rem at 22% 0%, rgb(63 173 164 / 0.22), transparent 70%), radial-gradient(18rem 12rem at 90% 4%, rgb(235 196 104 / 0.12), transparent 70%)',
      }}
      aria-hidden
    />
  );

  return (
    <>
      <aside className={cn(shell, 'hidden w-[268px] shrink-0 lg:flex')}>
        {glow}
        <div className="relative flex h-[68px] items-center px-6">
          <Logo tone="light" />
        </div>
        <div className="relative flex min-h-0 flex-1 flex-col">
          {nav}
          {footer}
        </div>
      </aside>

      {mobileOpen && (
        <div className="fixed inset-0 z-40 lg:hidden">
          <div className="absolute inset-0 bg-ink-950/55 backdrop-blur-sm" onClick={onCloseMobile} />
          <motion.aside
            initial={{ x: -300 }}
            animate={{ x: 0 }}
            exit={{ x: -300 }}
            transition={{ type: 'spring', bounce: 0, duration: 0.3 }}
            className={cn(shell, 'absolute left-0 top-0 h-full w-[286px] shadow-pop')}
          >
            {glow}
            <div className="relative flex h-[68px] items-center justify-between px-5">
              <Logo tone="light" />
              <button
                onClick={onCloseMobile}
                className="focus-ring rounded-lg p-2 text-white/60 transition-colors hover:bg-white/10 hover:text-white"
                aria-label="Close navigation"
              >
                <Icons.close size={19} />
              </button>
            </div>
            <div className="relative flex min-h-0 flex-1 flex-col">
              {nav}
              {footer}
            </div>
          </motion.aside>
        </div>
      )}
    </>
  );
}
