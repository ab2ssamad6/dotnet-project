import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { AnimatePresence, motion } from 'framer-motion';
import { Avatar, Badge, Button, Icons } from '@/components/ui';
import { useAuth } from '@/hooks/useAuth';
import { useClickOutside } from '@/hooks/useClickOutside';
import { useNotifications } from '@/features/notifications/NotificationsContext';
import { fullName } from '@/utils/format';
import { timeAgo } from '@/utils/format';
import { notify } from '@/utils/toast';
import { cn } from '@/utils/cn';

export function Topbar({ onOpenSidebar }: { onOpenSidebar: () => void }) {
  const { user, roles, logout } = useAuth();
  const navigate = useNavigate();
  const [profileOpen, setProfileOpen] = useState(false);
  const [notifOpen, setNotifOpen] = useState(false);
  const { notifications, unreadCount, markAllRead, clear } = useNotifications();

  const profileRef = useClickOutside<HTMLDivElement>(() => setProfileOpen(false));
  const notifRef = useClickOutside<HTMLDivElement>(() => setNotifOpen(false));

  const handleLogout = async () => {
    await logout();
    notify.success('You have been signed out.');
    navigate('/login', { replace: true });
  };

  const primaryRole = roles[0] ?? 'User';

  return (
    <header className="sticky top-0 z-30 flex h-16 items-center gap-3 border-b border-slate-200 bg-white/90 px-4 backdrop-blur lg:px-6">
      <button
        onClick={onOpenSidebar}
        className="rounded-lg p-2 text-slate-500 hover:bg-slate-100 lg:hidden"
        aria-label="Open navigation"
      >
        <Icons.menu size={22} />
      </button>

      <div className="flex-1" />

      {/* Notifications */}
      <div ref={notifRef} className="relative">
        <button
          onClick={() => {
            setNotifOpen((o) => !o);
            setProfileOpen(false);
          }}
          className="relative rounded-lg p-2 text-slate-500 hover:bg-slate-100"
          aria-label="Notifications"
        >
          <Icons.bell size={20} />
          {unreadCount > 0 && (
            <span className="absolute right-1.5 top-1.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-rose-500 px-1 text-[10px] font-bold text-white">
              {unreadCount}
            </span>
          )}
        </button>
        <AnimatePresence>
          {notifOpen && (
            <motion.div
              initial={{ opacity: 0, y: -8 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -8 }}
              className="absolute right-0 mt-2 w-80 overflow-hidden rounded-xl border border-slate-200 bg-white shadow-xl"
            >
              <div className="flex items-center justify-between border-b border-slate-100 px-4 py-3">
                <p className="text-sm font-semibold text-slate-800">Notifications</p>
                {notifications.length > 0 && (
                  <button onClick={markAllRead} className="text-xs font-medium text-brand-600 hover:underline">
                    Mark all read
                  </button>
                )}
              </div>
              <div className="max-h-80 overflow-y-auto">
                {notifications.length === 0 ? (
                  <p className="px-4 py-8 text-center text-sm text-slate-400">You're all caught up.</p>
                ) : (
                  notifications.map((n) => (
                    <div
                      key={n.id}
                      className={cn(
                        'flex gap-3 border-b border-slate-50 px-4 py-3 last:border-0',
                        !n.read && 'bg-brand-50/40',
                      )}
                    >
                      <span
                        className={cn(
                          'mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-full',
                          n.type === 'success'
                            ? 'bg-emerald-100 text-emerald-600'
                            : n.type === 'warning'
                              ? 'bg-amber-100 text-amber-600'
                              : 'bg-sky-100 text-sky-600',
                        )}
                      >
                        <Icons.info size={16} />
                      </span>
                      <div className="min-w-0">
                        <p className="text-sm font-medium text-slate-800">{n.title}</p>
                        {n.message && <p className="text-xs text-slate-500">{n.message}</p>}
                        <p className="mt-0.5 text-[11px] text-slate-400">{timeAgo(n.createdAt)}</p>
                      </div>
                    </div>
                  ))
                )}
              </div>
              {notifications.length > 0 && (
                <button
                  onClick={clear}
                  className="w-full border-t border-slate-100 py-2.5 text-center text-xs font-medium text-slate-500 hover:bg-slate-50"
                >
                  Clear all
                </button>
              )}
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Profile menu */}
      <div ref={profileRef} className="relative">
        <button
          onClick={() => {
            setProfileOpen((o) => !o);
            setNotifOpen(false);
          }}
          className="flex items-center gap-2 rounded-lg p-1 pr-2 hover:bg-slate-100"
        >
          <Avatar firstName={user?.firstName} lastName={user?.lastName} size="sm" />
          <div className="hidden text-left sm:block">
            <p className="text-sm font-medium leading-tight text-slate-800">{fullName(user?.firstName, user?.lastName)}</p>
            <p className="text-[11px] leading-tight text-slate-400">{primaryRole}</p>
          </div>
          <Icons.chevronDown size={16} className="hidden text-slate-400 sm:block" />
        </button>
        <AnimatePresence>
          {profileOpen && (
            <motion.div
              initial={{ opacity: 0, y: -8 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -8 }}
              className="absolute right-0 mt-2 w-60 overflow-hidden rounded-xl border border-slate-200 bg-white shadow-xl"
            >
              <div className="border-b border-slate-100 px-4 py-3">
                <p className="truncate text-sm font-semibold text-slate-800">{fullName(user?.firstName, user?.lastName)}</p>
                <p className="truncate text-xs text-slate-500">{user?.email}</p>
                <div className="mt-2 flex flex-wrap gap-1">
                  {roles.map((r) => (
                    <Badge key={r} className="bg-brand-50 text-brand-700">
                      {r}
                    </Badge>
                  ))}
                </div>
              </div>
              <div className="p-1.5">
                <Link
                  to="/profile"
                  onClick={() => setProfileOpen(false)}
                  className="flex items-center gap-2.5 rounded-lg px-3 py-2 text-sm text-slate-600 hover:bg-slate-100"
                >
                  <Icons.profile size={18} /> My Profile
                </Link>
                <Link
                  to="/settings"
                  onClick={() => setProfileOpen(false)}
                  className="flex items-center gap-2.5 rounded-lg px-3 py-2 text-sm text-slate-600 hover:bg-slate-100"
                >
                  <Icons.settings size={18} /> Settings
                </Link>
              </div>
              <div className="border-t border-slate-100 p-1.5">
                <Button variant="ghost" fullWidth className="justify-start text-rose-600 hover:bg-rose-50" onClick={handleLogout}>
                  <Icons.logout size={18} /> Sign out
                </Button>
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </header>
  );
}
