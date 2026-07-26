import { Badge } from '@/components/ui';
import {
  activityTypeOption,
  difficultyOption,
  enrollmentStatusOption,
  trainingStatusOption,
} from '@/constants/enums';
import type { ActivityType, DifficultyLevel, EnrollmentStatus, TrainingStatus } from '@/types';

export function DifficultyBadge({ value }: { value: DifficultyLevel }) {
  const o = difficultyOption(value);
  return <Badge className={o.badge}>{o.label}</Badge>;
}

export function TrainingStatusBadge({ value }: { value: TrainingStatus }) {
  const o = trainingStatusOption(value);
  return (
    <Badge dot className={o.badge}>
      {o.label}
    </Badge>
  );
}

export function EnrollmentStatusBadge({ value }: { value: EnrollmentStatus }) {
  const o = enrollmentStatusOption(value);
  return (
    <Badge dot className={o.badge}>
      {o.label}
    </Badge>
  );
}

export function ActivityTypeBadge({ value }: { value: ActivityType }) {
  const o = activityTypeOption(value);
  return <Badge className={o.badge}>{o.label}</Badge>;
}
