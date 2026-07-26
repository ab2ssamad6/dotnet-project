import { useForm } from 'react-hook-form';
import { z } from 'zod';
import {
  Avatar,
  Button,
  DataTable,
  Icons,
  Input,
  Modal,
  SearchInput,
  Textarea,
  useConfirm,
  type Column,
} from '@/components/ui';
import { PageHeader } from '@/components/common/PageHeader';
import { ErrorState } from '@/components/common/ErrorState';
import { usePagedList } from '@/hooks/usePagedList';
import { useDisclosure } from '@/hooks/useDisclosure';
import { zodResolver } from '@/utils/zodResolver';
import { trainerService } from '@/services';
import { notify } from '@/utils/toast';
import { fullName } from '@/utils/format';
import { useAuth } from '@/hooks/useAuth';
import type { TrainerDto } from '@/types';

const schema = z.object({
  firstName: z.string().min(1, 'First name is required.').max(100),
  lastName: z.string().min(1, 'Last name is required.').max(100),
  email: z.string().min(1, 'Email is required.').email('Enter a valid email.'),
  phone: z.string().max(40).optional(),
  expertise: z.string().max(200).optional(),
  avatar: z.string().url('Enter a valid URL.').optional().or(z.literal('')),
  biography: z.string().max(1000).optional(),
});
type FormValues = z.infer<typeof schema>;

export function TrainersPage() {
  const { isAdmin } = useAuth();
  const list = usePagedList(trainerService.list, { initialSort: { key: 'firstName', direction: 'asc' } });
  const modal = useDisclosure<TrainerDto>();
  const confirm = useConfirm();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  const openCreate = () => {
    reset({ firstName: '', lastName: '', email: '', phone: '', expertise: '', avatar: '', biography: '' });
    modal.open();
  };

  const openEdit = (t: TrainerDto) => {
    reset({
      firstName: t.firstName,
      lastName: t.lastName,
      email: t.email,
      phone: t.phone ?? '',
      expertise: t.expertise ?? '',
      avatar: t.avatar ?? '',
      biography: t.biography ?? '',
    });
    modal.open(t);
  };

  const onSubmit = async (values: FormValues) => {
    const payload = {
      firstName: values.firstName,
      lastName: values.lastName,
      email: values.email,
      phone: values.phone || null,
      expertise: values.expertise || null,
      avatar: values.avatar || null,
      biography: values.biography || null,
    };
    try {
      if (modal.payload) {
        await trainerService.update(modal.payload.id, payload);
        notify.success('Trainer updated.');
      } else {
        await trainerService.create(payload);
        notify.success('Trainer created.');
      }
      modal.close();
      list.refetch();
    } catch (err) {
      notify.apiError(err);
    }
  };

  const handleDelete = async (t: TrainerDto) => {
    const ok = await confirm({
      title: 'Delete trainer',
      message: (
        <>
          Delete <span className="font-medium">{fullName(t.firstName, t.lastName)}</span>? Trainings assigned to this
          trainer may be affected.
        </>
      ),
      confirmLabel: 'Delete',
    });
    if (!ok) return;
    try {
      await trainerService.remove(t.id);
      notify.success('Trainer deleted.');
      list.refetch();
    } catch (err) {
      notify.apiError(err);
    }
  };

  const columns: Column<TrainerDto>[] = [
    {
      key: 'firstName',
      header: 'Trainer',
      sortable: true,
      render: (t) => (
        <div className="flex items-center gap-3">
          <Avatar src={t.avatar} firstName={t.firstName} lastName={t.lastName} size="sm" />
          <div>
            <p className="font-medium text-slate-800">{fullName(t.firstName, t.lastName)}</p>
            <p className="text-xs text-slate-400">{t.email}</p>
          </div>
        </div>
      ),
    },
    {
      key: 'expertise',
      header: 'Expertise',
      hideOnMobile: true,
      render: (t) => <span className="text-slate-500">{t.expertise || '—'}</span>,
    },
    {
      key: 'phone',
      header: 'Phone',
      hideOnMobile: true,
      render: (t) => <span className="text-slate-500">{t.phone || '—'}</span>,
    },
    {
      key: 'actions',
      header: '',
      className: 'text-right',
      render: (t) => (
        <div className="flex justify-end gap-1">
          <Button variant="ghost" size="icon" onClick={() => openEdit(t)} aria-label="Edit">
            <Icons.edit size={17} />
          </Button>
          {isAdmin && (
            <Button
              variant="ghost"
              size="icon"
              className="text-rose-500 hover:bg-rose-50"
              onClick={() => handleDelete(t)}
              aria-label="Delete"
            >
              <Icons.trash size={17} />
            </Button>
          )}
        </div>
      ),
    },
  ];

  return (
    <div>
      <PageHeader
        title="Trainers"
        description="Manage instructor profiles."
        actions={
          isAdmin && (
            <Button leftIcon={<Icons.plus size={18} />} onClick={openCreate}>
              New trainer
            </Button>
          )
        }
      />

      <div className="mb-4 max-w-sm">
        <SearchInput value={list.search} onSearch={list.setSearch} placeholder="Search trainers…" />
      </div>

      {list.error ? (
        <ErrorState error={list.error} onRetry={list.refetch} />
      ) : (
        <DataTable
          columns={columns}
          data={list.items}
          rowKey={(t) => t.id}
          loading={list.loading}
          sort={list.sort}
          onSortChange={list.setSort}
          emptyTitle="No trainers yet"
          emptyDescription={isAdmin ? 'Add your first trainer profile.' : 'No trainers to display.'}
          emptyAction={isAdmin ? <Button onClick={openCreate}>New trainer</Button> : undefined}
          pagination={{
            page: list.page,
            totalPages: list.totalPages,
            totalCount: list.totalCount,
            pageSize: list.pageSize,
            onPageChange: list.setPage,
          }}
        />
      )}

      <Modal
        open={modal.isOpen}
        onClose={modal.close}
        size="lg"
        title={modal.payload ? 'Edit trainer' : 'New trainer'}
        closeOnBackdrop={!isSubmitting}
        footer={
          <>
            <Button variant="outline" onClick={modal.close} disabled={isSubmitting}>
              Cancel
            </Button>
            <Button onClick={handleSubmit(onSubmit)} loading={isSubmitting}>
              {modal.payload ? 'Save changes' : 'Create trainer'}
            </Button>
          </>
        }
      >
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="grid gap-4 sm:grid-cols-2">
            <Input label="First name" error={errors.firstName?.message} required {...register('firstName')} />
            <Input label="Last name" error={errors.lastName?.message} required {...register('lastName')} />
          </div>
          <div className="grid gap-4 sm:grid-cols-2">
            <Input label="Email" type="email" error={errors.email?.message} required {...register('email')} />
            <Input label="Phone" placeholder="+1-555-0100" error={errors.phone?.message} {...register('phone')} />
          </div>
          <Input label="Expertise" placeholder="e.g. .NET, Cloud, Data" error={errors.expertise?.message} {...register('expertise')} />
          <Input
            label="Avatar URL"
            placeholder="https://…/photo.jpg"
            hint="Link to a profile image."
            error={errors.avatar?.message}
            {...register('avatar')}
          />
          <Textarea label="Biography" rows={4} error={errors.biography?.message} {...register('biography')} />
        </form>
      </Modal>
    </div>
  );
}
