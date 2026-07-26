import { useForm } from 'react-hook-form';
import { z } from 'zod';
import {
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
import { categoryService } from '@/services';
import { notify } from '@/utils/toast';
import { formatDate } from '@/utils/format';
import { useAuth } from '@/hooks/useAuth';
import type { CategoryDto } from '@/types';

const schema = z.object({
  name: z.string().min(1, 'Name is required.').max(120),
  description: z.string().max(500).optional(),
});
type FormValues = z.infer<typeof schema>;

export function CategoriesPage() {
  const { isAdmin } = useAuth();
  const list = usePagedList(categoryService.list, { initialSort: { key: 'name', direction: 'asc' } });
  const modal = useDisclosure<CategoryDto>();
  const confirm = useConfirm();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  const openCreate = () => {
    reset({ name: '', description: '' });
    modal.open();
  };

  const openEdit = (category: CategoryDto) => {
    reset({ name: category.name, description: category.description ?? '' });
    modal.open(category);
  };

  const onSubmit = async (values: FormValues) => {
    try {
      const payload = { name: values.name, description: values.description || null };
      if (modal.payload) {
        await categoryService.update(modal.payload.id, payload);
        notify.success('Category updated.');
      } else {
        await categoryService.create(payload);
        notify.success('Category created.');
      }
      modal.close();
      list.refetch();
    } catch (err) {
      notify.apiError(err);
    }
  };

  const handleDelete = async (category: CategoryDto) => {
    const ok = await confirm({
      title: 'Delete category',
      message: (
        <>
          Delete <span className="font-medium">"{category.name}"</span>? Trainings referencing it may be affected. This
          action cannot be undone.
        </>
      ),
      confirmLabel: 'Delete',
    });
    if (!ok) return;
    try {
      await categoryService.remove(category.id);
      notify.success('Category deleted.');
      list.refetch();
    } catch (err) {
      notify.apiError(err);
    }
  };

  const columns: Column<CategoryDto>[] = [
    {
      key: 'name',
      header: 'Name',
      sortable: true,
      render: (c) => (
        <div className="flex items-center gap-3">
          <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-brand-50 text-brand-600">
            <Icons.category size={18} />
          </span>
          <span className="font-medium text-slate-800">{c.name}</span>
        </div>
      ),
    },
    {
      key: 'description',
      header: 'Description',
      hideOnMobile: true,
      render: (c) => <span className="line-clamp-1 text-slate-500">{c.description || '—'}</span>,
    },
    {
      key: 'createdAt',
      header: 'Created',
      sortable: true,
      hideOnMobile: true,
      render: (c) => <span className="text-slate-500">{formatDate(c.createdAt)}</span>,
    },
    {
      key: 'actions',
      header: '',
      className: 'text-right',
      render: (c) => (
        <div className="flex justify-end gap-1" onClick={(e) => e.stopPropagation()}>
          <Button variant="ghost" size="icon" onClick={() => openEdit(c)} aria-label="Edit">
            <Icons.edit size={17} />
          </Button>
          {isAdmin && (
            <Button
              variant="ghost"
              size="icon"
              className="text-rose-500 hover:bg-rose-50"
              onClick={() => handleDelete(c)}
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
        title="Categories"
        description="Organize trainings into categories."
        actions={
          <Button leftIcon={<Icons.plus size={18} />} onClick={openCreate}>
            New category
          </Button>
        }
      />

      <div className="mb-4 max-w-sm">
        <SearchInput value={list.search} onSearch={list.setSearch} placeholder="Search categories…" />
      </div>

      {list.error ? (
        <ErrorState error={list.error} onRetry={list.refetch} />
      ) : (
        <DataTable
          columns={columns}
          data={list.items}
          rowKey={(c) => c.id}
          loading={list.loading}
          sort={list.sort}
          onSortChange={list.setSort}
          emptyTitle="No categories yet"
          emptyDescription="Create your first category to group trainings."
          emptyAction={<Button onClick={openCreate}>New category</Button>}
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
        title={modal.payload ? 'Edit category' : 'New category'}
        closeOnBackdrop={!isSubmitting}
        footer={
          <>
            <Button variant="outline" onClick={modal.close} disabled={isSubmitting}>
              Cancel
            </Button>
            <Button onClick={handleSubmit(onSubmit)} loading={isSubmitting}>
              {modal.payload ? 'Save changes' : 'Create category'}
            </Button>
          </>
        }
      >
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Input label="Name" placeholder="e.g. Web Development" error={errors.name?.message} required {...register('name')} />
          <Textarea
            label="Description"
            placeholder="What kind of trainings belong here?"
            error={errors.description?.message}
            {...register('description')}
          />
        </form>
      </Modal>
    </div>
  );
}
