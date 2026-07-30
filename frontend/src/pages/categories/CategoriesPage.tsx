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
      title: 'Delete this category?',
      message: (
        <>
          <span className="font-semibold text-ink-800">"{category.name}"</span> will be removed, and trainings that
          reference it may be affected. This can't be undone.
        </>
      ),
      confirmLabel: 'Delete category',
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
          <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-200/60">
            <Icons.category size={17} />
          </span>
          <span className="font-bold text-ink-800">{c.name}</span>
        </div>
      ),
    },
    {
      key: 'description',
      header: 'Description',
      hideOnMobile: true,
      render: (c) => <span className="line-clamp-1 text-[13px] text-ink-500">{c.description || '—'}</span>,
    },
    {
      key: 'createdAt',
      header: 'Created',
      sortable: true,
      hideOnMobile: true,
      render: (c) => <span className="tnum text-[13px] text-ink-500">{formatDate(c.createdAt)}</span>,
    },
    {
      key: 'actions',
      header: '',
      className: 'text-right',
      render: (c) => (
        <div className="flex justify-end gap-1" onClick={(e) => e.stopPropagation()}>
          <Button variant="ghost" size="icon" className="h-9 w-9" onClick={() => openEdit(c)} aria-label="Edit category">
            <Icons.edit size={17} />
          </Button>
          {isAdmin && (
            <Button
              variant="ghost"
              size="icon"
              className="h-9 w-9 text-rose-500 hover:bg-rose-50 hover:text-rose-600"
              onClick={() => handleDelete(c)}
              aria-label="Delete category"
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
        eyebrow="Curriculum"
        title="Categories"
        description="Group your trainings so learners can find the right track quickly."
        actions={
          <Button leftIcon={<Icons.plus size={17} />} onClick={openCreate}>
            New category
          </Button>
        }
      />

      <div className="surface mb-5 p-3">
        <SearchInput
          value={list.search}
          onSearch={list.setSearch}
          placeholder="Search categories…"
          className="max-w-sm"
        />
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
          emptyDescription="Create your first category to give the catalog some structure."
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
        description="Categories are how learners filter the catalog."
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
            placeholder="What kind of trainings belong in here?"
            error={errors.description?.message}
            {...register('description')}
          />
        </form>
      </Modal>
    </div>
  );
}
