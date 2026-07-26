import { useState } from 'react';
import { AnimatePresence, motion } from 'framer-motion';
import { Button, Icons, Spinner, useConfirm } from '@/components/ui';
import { ActivityTypeBadge } from '@/components/common/StatusBadge';
import { useAsync } from '@/hooks/useAsync';
import { useDisclosure } from '@/hooks/useDisclosure';
import { useClickOutside } from '@/hooks/useClickOutside';
import { activityService } from '@/services';
import { notify } from '@/utils/toast';
import { ActivityType, type ActivityDto } from '@/types';
import { ActivityFormModal } from './ActivityFormModal';

const ADD_OPTIONS: { type: ActivityType; label: string; icon: keyof typeof Icons }[] = [
  { type: ActivityType.Lesson, label: 'Lesson', icon: 'book' },
  { type: ActivityType.Exercise, label: 'Exercise', icon: 'edit' },
  { type: ActivityType.Quiz, label: 'Quiz', icon: 'list' },
  { type: ActivityType.Exam, label: 'Exam', icon: 'award' },
];

export function ActivitiesPanel({ moduleId }: { moduleId: string }) {
  const { data, loading, refetch } = useAsync(() => activityService.listByModule(moduleId), [moduleId]);
  const modal = useDisclosure<ActivityType>();
  const confirm = useConfirm();
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useClickOutside<HTMLDivElement>(() => setMenuOpen(false));

  const activities = [...(data ?? [])].sort((a, b) => a.order - b.order);
  const nextOrder = activities.length ? Math.max(...activities.map((a) => a.order)) + 1 : 1;

  const handleDelete = async (activity: ActivityDto) => {
    const ok = await confirm({
      title: 'Delete activity',
      message: (
        <>
          Delete <span className="font-medium">"{activity.title}"</span>?
        </>
      ),
      confirmLabel: 'Delete',
    });
    if (!ok) return;
    try {
      await activityService.remove(activity.id);
      notify.success('Activity deleted.');
      refetch();
    } catch (err) {
      notify.apiError(err);
    }
  };

  return (
    <div>
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-sm font-semibold text-slate-700">Activities ({activities.length})</h3>
        <div ref={menuRef} className="relative">
          <Button size="sm" variant="outline" leftIcon={<Icons.plus size={15} />} onClick={() => setMenuOpen((o) => !o)}>
            Add activity
          </Button>
          <AnimatePresence>
            {menuOpen && (
              <motion.div
                initial={{ opacity: 0, y: -6 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: -6 }}
                className="absolute right-0 z-10 mt-1 w-44 overflow-hidden rounded-lg border border-slate-200 bg-white p-1 shadow-lg"
              >
                {ADD_OPTIONS.map((opt) => {
                  const Icon = Icons[opt.icon];
                  return (
                    <button
                      key={opt.type}
                      onClick={() => {
                        setMenuOpen(false);
                        modal.open(opt.type);
                      }}
                      className="flex w-full items-center gap-2.5 rounded-md px-3 py-2 text-left text-sm text-slate-600 hover:bg-slate-100"
                    >
                      <Icon size={16} /> {opt.label}
                    </button>
                  );
                })}
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      </div>

      {loading ? (
        <div className="flex justify-center py-8 text-slate-400">
          <Spinner />
        </div>
      ) : activities.length === 0 ? (
        <p className="rounded-lg border border-dashed border-slate-200 py-6 text-center text-sm text-slate-400">
          No activities yet. Add a lesson, exercise, quiz or exam.
        </p>
      ) : (
        <ul className="space-y-2">
          {activities.map((a) => (
            <li
              key={a.id}
              className="flex items-center gap-3 rounded-lg border border-slate-200 bg-white px-3 py-2.5"
            >
              <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-md bg-slate-100 text-xs font-semibold text-slate-500">
                {a.order}
              </span>
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium text-slate-800">{a.title}</p>
                {a.questions && a.questions.length > 0 && (
                  <p className="text-xs text-slate-400">
                    {a.questions.length} question{a.questions.length > 1 ? 's' : ''}
                    {a.passingScore != null && ` · pass ${a.passingScore}%`}
                    {a.durationMinutes != null && ` · ${a.durationMinutes} min`}
                  </p>
                )}
              </div>
              <ActivityTypeBadge value={a.type} />
              <Button
                variant="ghost"
                size="icon"
                className="text-rose-500 hover:bg-rose-50"
                onClick={() => handleDelete(a)}
                aria-label="Delete activity"
              >
                <Icons.trash size={16} />
              </Button>
            </li>
          ))}
        </ul>
      )}

      {modal.payload !== undefined && (
        <ActivityFormModal
          open={modal.isOpen}
          onClose={modal.close}
          moduleId={moduleId}
          type={modal.payload}
          nextOrder={nextOrder}
          onSaved={refetch}
        />
      )}
    </div>
  );
}
