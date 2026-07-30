import { NavLink } from 'react-router-dom';
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

export function Sidebar({ mobileOpen, onCloseMobile }: SidebarProps) {
  const { roles } = useAuth();
  const sections = visibleSections(roles);

  const nav = (
    <nav className="flex-1 space-y-6 overflow-y-auto px-3 py-4">
      {sections.map((section, i) => (
        <div key={section.heading ?? i}>
          {section.heading && (
            <p className="mb-1.5 px-3 text-[11px] font-semibold uppercase tracking-wider text-slate-400">
              {section.heading}
            </p>
          )}
          <div className="space-y-0.5">
            {section.items.map((item) => {
              const Icon = Icons[item.icon];
              return (
                <NavLink
                  key={item.to}
                  to={item.to}
                  onClick={onCloseMobile}
                  className={({ isActive }) =>
                    cn(
                      'group flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                      isActive
                        ? 'bg-brand-50 text-brand-700'
                        : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900',
                    )
                  }
                >
                  <Icon size={19} />
                  {item.label}
                </NavLink>
              );
            })}
          </div>
        </div>
      ))}
    </nav>
  );

  return (
    <>
      <aside className="hidden w-64 shrink-0 flex-col border-r border-slate-200 bg-white lg:flex">
        <div className="flex h-16 items-center border-b border-slate-100 px-5">
          <Logo />
        </div>
        {nav}
      </aside>

      {mobileOpen && (
        <div className="fixed inset-0 z-40 lg:hidden">
          <div className="absolute inset-0 bg-slate-900/50 backdrop-blur-sm" onClick={onCloseMobile} />
          <motion.aside
            initial={{ x: -280 }}
            animate={{ x: 0 }}
            exit={{ x: -280 }}
            transition={{ type: 'spring', bounce: 0, duration: 0.3 }}
            className="absolute left-0 top-0 flex h-full w-72 flex-col bg-white shadow-xl"
          >
            <div className="flex h-16 items-center justify-between border-b border-slate-100 px-5">
              <Logo />
              <button onClick={onCloseMobile} className="rounded-lg p-1.5 text-slate-400 hover:bg-slate-100">
                <Icons.close size={20} />
              </button>
            </div>
            {nav}
          </motion.aside>
        </div>
      )}
    </>
  );
}
