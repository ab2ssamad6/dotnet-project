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
    <div className="surface group flex flex-col overflow-hidden transition-all duration-200 hover:-translate-y-0.5 hover:border-ink-300/80 hover:shadow-raised">
      <button
        onClick={onOpen}
        className="relative block h-36 w-full overflow-hidden bg-ink-100 text-left"
        aria-label={`Open ${training.title}`}
      >
        {training.thumbnail ? (
          <img
            src={training.thumbnail}
            alt=""
            className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-[1.04]"
          />
        ) : (
          <div className="relative flex h-full w-full items-center justify-center bg-brand-gradient text-white/90">
            <span className="absolute inset-0 bg-grain opacity-[0.08] mix-blend-overlay" aria-hidden />
            <Icons.training size={34} />
          </div>
        )}
        <span className="absolute inset-x-0 bottom-0 h-16 bg-gradient-to-t from-ink-950/50 to-transparent" aria-hidden />
        <span className="absolute left-3 top-3">
          <TrainingStatusBadge value={training.status} />
        </span>
        <span className="absolute bottom-3 left-3 text-[11px] font-bold uppercase tracking-[0.12em] text-white/85">
          {training.categoryName ?? 'Uncategorized'}
        </span>
      </button>

      <div className="flex flex-1 flex-col p-4">
        <div className="mb-2.5 flex flex-wrap items-center gap-2">
          <DifficultyBadge value={training.difficulty} />
          <Badge tone="neutral">
            <Icons.clock size={11} /> {formatDuration(training.duration)}
          </Badge>
        </div>
        <button onClick={onOpen} className="focus-ring rounded text-left">
          <h3 className="line-clamp-1 text-[15px] font-bold tracking-[-0.01em] text-ink-900 transition-colors group-hover:text-brand-800">
            {training.title}
          </h3>
        </button>
        <p className="mt-1.5 line-clamp-2 flex-1 text-[13px] leading-relaxed text-ink-500">{training.description}</p>

        <div className="mt-3.5 flex items-center gap-4 text-[11.5px] font-medium text-ink-400">
          <span className="inline-flex items-center gap-1.5">
            <Icons.layers size={13} /> {training.moduleCount} modules
          </span>
          <span className="inline-flex min-w-0 items-center gap-1.5">
            <Icons.trainer size={13} /> <span className="truncate">{training.trainerName ?? 'Unassigned'}</span>
          </span>
        </div>

        <div className="mt-4 flex items-center gap-1 border-t border-ink-100 pt-3.5">
          <Button variant="outline" size="sm" className="flex-1" onClick={onOpen}>
            Manage
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={onTogglePublish}
            title={training.published ? 'Unpublish from catalog' : 'Publish to catalog'}
          >
            {training.published ? <Icons.eyeOff size={16} /> : <Icons.eye size={16} />}
          </Button>
          <Button variant="ghost" size="icon" className="h-9 w-9" onClick={onEdit} aria-label="Edit training">
            <Icons.edit size={16} />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            className="h-9 w-9 text-rose-500 hover:bg-rose-50 hover:text-rose-600"
            onClick={onDelete}
            aria-label="Delete training"
          >
            <Icons.trash size={16} />
          </Button>
        </div>
      </div>
    </div>
  );
}
