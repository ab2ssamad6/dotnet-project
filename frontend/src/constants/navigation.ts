import type { IconName } from '@/components/ui';
import { Role } from '@/types';

export interface NavItem {
  label: string;
  to: string;
  icon: IconName;
  /** Roles allowed to see this item. Empty = all authenticated users. */
  roles?: string[];
}

export interface NavSection {
  heading?: string;
  items: NavItem[];
}

/**
 * Role-aware navigation. The sidebar filters items by the current user's roles.
 * Students get a learning-focused menu; Admins/Trainers get management menus.
 */
export const NAV_SECTIONS: NavSection[] = [
  {
    items: [
      { label: 'Dashboard', to: '/dashboard', icon: 'dashboard' },
      { label: 'Browse Catalog', to: '/catalog', icon: 'grid', roles: [Role.Student] },
      { label: 'My Learning', to: '/my-learning', icon: 'book', roles: [Role.Student] },
    ],
  },
  {
    heading: 'Content',
    items: [
      { label: 'Trainings', to: '/trainings', icon: 'training', roles: [Role.Administrator, Role.Trainer] },
      { label: 'Categories', to: '/categories', icon: 'category', roles: [Role.Administrator, Role.Trainer] },
      { label: 'Trainers', to: '/trainers', icon: 'trainer', roles: [Role.Administrator, Role.Trainer] },
    ],
  },
  {
    heading: 'People',
    items: [{ label: 'Students', to: '/students', icon: 'users', roles: [Role.Administrator] }],
  },
  {
    heading: 'Tools',
    items: [
      { label: 'AI Trainer', to: '/ai-trainer', icon: 'ai' },
      { label: 'Settings', to: '/settings', icon: 'settings' },
    ],
  },
];

export function visibleSections(roles: string[]): NavSection[] {
  return NAV_SECTIONS.map((section) => ({
    ...section,
    items: section.items.filter((item) => !item.roles || item.roles.some((r) => roles.includes(r))),
  })).filter((section) => section.items.length > 0);
}
