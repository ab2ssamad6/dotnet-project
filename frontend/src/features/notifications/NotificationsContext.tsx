import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react';

export interface AppNotification {
  id: string;
  title: string;
  message?: string;
  type: 'info' | 'success' | 'warning';
  createdAt: string;
  read: boolean;
}

interface NotificationsContextValue {
  notifications: AppNotification[];
  unreadCount: number;
  push: (n: Omit<AppNotification, 'id' | 'createdAt' | 'read'>) => void;
  markAllRead: () => void;
  clear: () => void;
}

const NotificationsContext = createContext<NotificationsContextValue | null>(null);

const seed: AppNotification[] = [
  {
    id: 'welcome',
    title: 'Welcome to AI Tutor',
    message: 'Explore trainings, track progress, and chat with the AI Trainer.',
    type: 'info',
    createdAt: new Date().toISOString(),
    read: false,
  },
];

export function NotificationsProvider({ children }: { children: ReactNode }) {
  const [notifications, setNotifications] = useState<AppNotification[]>(seed);

  const push = useCallback<NotificationsContextValue['push']>((n) => {
    setNotifications((prev) => [
      { ...n, id: crypto.randomUUID(), createdAt: new Date().toISOString(), read: false },
      ...prev,
    ]);
  }, []);

  const markAllRead = useCallback(() => {
    setNotifications((prev) => prev.map((n) => ({ ...n, read: true })));
  }, []);

  const clear = useCallback(() => setNotifications([]), []);

  const value = useMemo<NotificationsContextValue>(
    () => ({
      notifications,
      unreadCount: notifications.filter((n) => !n.read).length,
      push,
      markAllRead,
      clear,
    }),
    [notifications, push, markAllRead, clear],
  );

  return <NotificationsContext.Provider value={value}>{children}</NotificationsContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useNotifications() {
  const ctx = useContext(NotificationsContext);
  if (!ctx) throw new Error('useNotifications must be used within a NotificationsProvider');
  return ctx;
}
