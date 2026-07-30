import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { AnimatePresence, motion } from 'framer-motion';
import { Avatar, Badge, Icons } from '@/components/ui';
import { useAuth } from '@/hooks/useAuth';
import { useClickOutside } from '@/hooks/useClickOutside';
import { useNotifications } from '@/features/notifications/NotificationsContext';
import { fullName } from '@/utils/format';
import { timeAgo } from '@/utils/format';
import { notify } from '@/utils/toast';
import { cn } from '@/utils/cn';

const iconButton =
  'focus-ring relative rounded-lg p-2.5 text-ink-500 transition-colors hover:bg-ink-100 hover:text-ink-800';

const panel =
  'absolute right-0 mt-2.5 overflow-hidden rounded-2xl border border-ink-200/70 bg-white shadow-pop';

const menuItem =
  'flex items-center gap-2.5 rounded-lg px-3 py-2.5 text-[13.5px] font-medium text-ink-600 transition-colors hover:bg-ink-100 hover:text-ink-900';

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
    notify.success('Signed out — see you soon.');
    navigate('/login', { replace: true });
  };

  const primaryRole = roles[0] ?? 'Member';

  return (
    <header className="sticky top-0 z-30 flex h-[68px] items-center gap-2 border-b border-ink-200/70 bg-white/80 px-4 backdrop-blur-xl lg:px-8">
      <button onClick={onOpenSidebar} className={cn(iconButton, 'lg:hidden')} aria-label="Open navigation">
        <Icons.menu size={21} />
      </button>

      <div className="hidden items-center gap-2 lg:flex">
        <span className="eyebrow">Workspace</span>
        <span className="h-1 w-1 rounded-full bg-ink-300" />
        <span className="text-[13px] font-semibold text-ink-600">{primaryRole}</span>
      </div>

      <div className="flex-1" />

      <div ref={notifRef} className="relative">
        <button
          onClick={() => {
            setNotifOpen((o) => !o);
            setProfileOpen(false);
          }}
          className={iconButton}
          aria-label="Notifications"
        >
          <Icons.bell size={19} />
          {unreadCount > 0 && (
            <span className="tnum absolute right-1 top-1 flex h-[17px] min-w-[17px] items-center justify-center rounded-full bg-rose-500 px-1 text-[10px] font-bold text-white ring-2 ring-white">
              {unreadCount}
            </span>
          )}
        </button>
        <AnimatePresence>
          {notifOpen && (
            <motion.div
              initial={{ opacity: 0, y: -6, scale: 0.98 }}
              animate={{ opacity: 1, y: 0, scale: 1 }}
              exit={{ opacity: 0, y: -6, scale: 0.98 }}
              transition={{ duration: 0.16 }}
              className={cn(panel, 'w-[336px]')}
            >
              <div className="flex items-center justify-between border-b border-ink-100 px-4 py-3.5">
                <div>
                  <p className="text-sm font-bold text-ink-900">Notifications</p>
                  <p className="text-[11.5px] text-ink-400">
                    {unreadCount > 0 ? `${unreadCount} unread` : 'Nothing new'}
                  </p>
                </div>
                {notifications.length > 0 && (
                  <button
                    onClick={markAllRead}
                    className="rounded-md px-2 py-1 text-[12px] font-semibold text-brand-700 transition-colors hover:bg-brand-50"
                  >
                    Mark all read
                  </button>
                )}
              </div>
              <div className="max-h-[336px] overflow-y-auto">
                {notifications.length === 0 ? (
                  <div className="px-4 py-10 text-center">
                    <span className="mx-auto mb-3 flex h-11 w-11 items-center justify-center rounded-xl bg-ink-100 text-ink-400">
                      <Icons.bell size={19} />
                    </span>
                    <p className="text-sm font-semibold text-ink-700">You're all caught up</p>
                    <p className="mt-0.5 text-xs text-ink-400">New activity will land here.</p>
                  </div>
                ) : (
                  notifications.map((n) => (
                    <div
                      key={n.id}
                      className={cn(
                        'flex gap-3 border-b border-ink-100/70 px-4 py-3.5 last:border-0',
                        !n.read && 'bg-brand-50/40',
                      )}
                    >
                      <span
                        className={cn(
                          'mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-lg ring-1 ring-inset',
                          n.type === 'success'
                            ? 'bg-green-50 text-green-600 ring-green-200/70'
                            : n.type === 'warning'
                              ? 'bg-amber-50 text-amber-600 ring-amber-200/70'
                              : 'bg-sky-50 text-sky-600 ring-sky-200/70',
                        )}
                      >
                        {n.type === 'success' ? (
                          <Icons.check size={15} />
                        ) : n.type === 'warning' ? (
                          <Icons.alert size={15} />
                        ) : (
                          <Icons.info size={15} />
                        )}
                      </span>
                      <div className="min-w-0">
                        <p className="text-[13.5px] font-semibold text-ink-800">{n.title}</p>
                        {n.message && <p className="mt-0.5 text-xs leading-relaxed text-ink-500">{n.message}</p>}
                        <p className="mt-1 text-[11px] font-medium text-ink-400">{timeAgo(n.createdAt)}</p>
                      </div>
                    </div>
                  ))
                )}
              </div>
              {notifications.length > 0 && (
                <button
                  onClick={clear}
                  className="w-full border-t border-ink-100 py-3 text-center text-[12px] font-semibold text-ink-500 transition-colors hover:bg-ink-50 hover:text-ink-800"
                >
                  Clear all
                </button>
              )}
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      <div ref={profileRef} className="relative">
        <button
          onClick={() => {
            setProfileOpen((o) => !o);
            setNotifOpen(false);
          }}
          className="focus-ring flex items-center gap-2.5 rounded-xl p-1 pr-2.5 transition-colors hover:bg-ink-100"
        >
          <Avatar firstName={user?.firstName} lastName={user?.lastName} size="sm" />
          <div className="hidden text-left sm:block">
            <p className="text-[13px] font-bold leading-tight text-ink-800">
              {fullName(user?.firstName, user?.lastName)}
            </p>
            <p className="text-[11px] font-medium leading-tight text-ink-400">{user?.email}</p>
          </div>
          <Icons.chevronDown size={15} className="hidden text-ink-400 sm:block" />
        </button>
        <AnimatePresence>
          {profileOpen && (
            <motion.div
              initial={{ opacity: 0, y: -6, scale: 0.98 }}
              animate={{ opacity: 1, y: 0, scale: 1 }}
              exit={{ opacity: 0, y: -6, scale: 0.98 }}
              transition={{ duration: 0.16 }}
              className={cn(panel, 'w-[264px]')}
            >
              <div className="flex items-center gap-3 border-b border-ink-100 bg-ink-50/60 px-4 py-4">
                <Avatar firstName={user?.firstName} lastName={user?.lastName} size="md" />
                <div className="min-w-0">
                  <p className="truncate text-sm font-bold text-ink-900">
                    {fullName(user?.firstName, user?.lastName)}
                  </p>
                  <p className="truncate text-xs text-ink-500">{user?.email}</p>
                </div>
              </div>
              <div className="flex flex-wrap gap-1.5 border-b border-ink-100 px-4 py-3">
                {roles.map((r) => (
                  <Badge key={r} tone="brand">
                    {r}
                  </Badge>
                ))}
              </div>
              <div className="p-2">
                <Link to="/profile" onClick={() => setProfileOpen(false)} className={menuItem}>
                  <Icons.profile size={17} /> My profile
                </Link>
                <Link to="/settings" onClick={() => setProfileOpen(false)} className={menuItem}>
                  <Icons.settings size={17} /> Account settings
                </Link>
              </div>
              <div className="border-t border-ink-100 p-2">
                <button
                  onClick={handleLogout}
                  className={cn(menuItem, 'w-full text-rose-600 hover:bg-rose-50 hover:text-rose-700')}
                >
                  <Icons.logout size={17} /> Sign out
                </button>
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </header>
  );
}
