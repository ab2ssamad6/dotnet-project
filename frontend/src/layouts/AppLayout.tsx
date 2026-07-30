import { useState } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Sidebar } from './Sidebar';
import { Topbar } from './Topbar';

export function AppLayout() {
  const [mobileOpen, setMobileOpen] = useState(false);
  const location = useLocation();

  return (
    <div className="flex h-full min-h-screen bg-canvas">
      <Sidebar mobileOpen={mobileOpen} onCloseMobile={() => setMobileOpen(false)} />
      <div className="canvas-glow flex min-w-0 flex-1 flex-col">
        <Topbar onOpenSidebar={() => setMobileOpen(true)} />
        <main className="flex-1 overflow-x-hidden">
          <motion.div
            key={location.pathname}
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.24, ease: [0.16, 1, 0.3, 1] }}
            className="mx-auto w-full max-w-[1360px] px-4 py-7 sm:px-6 lg:px-10 lg:py-9"
          >
            <Outlet />
          </motion.div>
        </main>
        <footer className="mx-auto w-full max-w-[1360px] px-4 pb-8 sm:px-6 lg:px-10">
          <p className="border-t border-ink-200/70 pt-5 text-[11.5px] text-ink-400">
            © {new Date().getFullYear()} LMS Learning Studio — built for trainers and their learners.
          </p>
        </footer>
      </div>
    </div>
  );
}
