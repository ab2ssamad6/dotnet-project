import { Badge, Button, Icons } from '@/components/ui';
import { DifficultyBadge, TrainingStatusBadge } from '@/components/common/StatusBadge';
import { formatDuration } from '@/utils/format';
import type { TrainingDto } from '@/types';

interface Props {
  training: TrainingDto;
  onOpen: () => void;
  onEdit: () => void;
  onDelete: () => void;
  onTogglePublish: () => void;
}

export function TrainingCard({ training, onOpen, onEdit, onDelete, onTogglePublish }: Props) {
  return (
    <div className="group flex flex-col overflow-hidden rounded-xl border border-slate-200 bg-white shadow-card transition-shadow hover:shadow-md">
      <button onClick={onOpen} className="relative block h-32 w-full overflow-hidden bg-slate-100 text-left">
        {training.thumbnail ? (
          <img
            src={training.thumbnail}
            alt={training.title}
            className="h-full w-full object-cover transition-transform group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center bg-gradient-to-br from-brand-500 to-brand-700 text-white">
            <Icons.training size={36} />
          </div>
        )}
        <div className="absolute left-3 top-3">
          <TrainingStatusBadge value={training.status} />
        </div>
      </button>

      <div className="flex flex-1 flex-col p-4">
        <div className="mb-2 flex items-center gap-2">
          <DifficultyBadge value={training.difficulty} />
          <Badge className="bg-slate-100 text-slate-600">{training.categoryName ?? 'Uncategorized'}</Badge>
        </div>
        <button onClick={onOpen} className="text-left">
          <h3 className="line-clamp-1 font-semibold text-slate-900 group-hover:text-brand-700">{training.title}</h3>
        </button>
        <p className="mt-1 line-clamp-2 flex-1 text-sm text-slate-500">{training.description}</p>

        <div className="mt-3 flex items-center gap-4 text-xs text-slate-400">
          <span className="inline-flex items-center gap-1">
            <Icons.clock size={14} /> {formatDuration(training.duration)}
          </span>
          <span className="inline-flex items-center gap-1">
            <Icons.layers size={14} /> {training.moduleCount} modules
          </span>
          <span className="inline-flex items-center gap-1 truncate">
            <Icons.trainer size={14} /> {training.trainerName ?? '—'}
          </span>
        </div>

        <div className="mt-4 flex items-center gap-1 border-t border-slate-100 pt-3">
          <Button variant="outline" size="sm" className="flex-1" onClick={onOpen}>
            Manage
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={onTogglePublish}
            title={training.published ? 'Unpublish' : 'Publish'}
          >
            {training.published ? <Icons.eyeOff size={16} /> : <Icons.eye size={16} />}
          </Button>
          <Button variant="ghost" size="icon" onClick={onEdit} aria-label="Edit">
            <Icons.edit size={16} />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            className="text-rose-500 hover:bg-rose-50"
            onClick={onDelete}
            aria-label="Delete"
          >
            <Icons.trash size={16} />
          </Button>
        </div>
      </div>
    </div>
  );
}
