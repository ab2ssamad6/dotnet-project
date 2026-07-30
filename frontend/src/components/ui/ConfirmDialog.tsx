import { createContext, useCallback, useContext, useMemo, useRef, useState, type ReactNode } from 'react';
import { Modal } from './Modal';
import { Button } from './Button';
import { Icons } from './Icon';

interface ConfirmOptions {
  title: string;
  message: ReactNode;
  confirmLabel?: string;
  cancelLabel?: string;
  variant?: 'danger' | 'primary';
}

type ConfirmFn = (options: ConfirmOptions) => Promise<boolean>;

const ConfirmContext = createContext<ConfirmFn | null>(null);

export function ConfirmProvider({ children }: { children: ReactNode }) {
  const [open, setOpen] = useState(false);
  const [options, setOptions] = useState<ConfirmOptions | null>(null);
  const [busy, setBusy] = useState(false);
  const resolver = useRef<((value: boolean) => void) | null>(null);

  const confirm = useCallback<ConfirmFn>((opts) => {
    setOptions(opts);
    setOpen(true);
    return new Promise<boolean>((resolve) => {
      resolver.current = resolve;
    });
  }, []);

  const close = useCallback((result: boolean) => {
    resolver.current?.(result);
    resolver.current = null;
    setOpen(false);
    setBusy(false);
  }, []);

  const value = useMemo(() => confirm, [confirm]);

  return (
    <ConfirmContext.Provider value={value}>
      {children}
      <Modal
        open={open}
        onClose={() => !busy && close(false)}
        size="sm"
        closeOnBackdrop={!busy}
        footer={
          <>
            <Button variant="outline" onClick={() => close(false)} disabled={busy}>
              {options?.cancelLabel ?? 'Cancel'}
            </Button>
            <Button
              variant={options?.variant ?? 'danger'}
              loading={busy}
              onClick={() => {
                setBusy(true);
                close(true);
              }}
            >
              {options?.confirmLabel ?? 'Confirm'}
            </Button>
          </>
        }
      >
        <div className="flex gap-4">
          <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-rose-100 text-rose-600">
            <Icons.alert size={22} />
          </div>
          <div>
            <h2 className="text-base font-semibold text-slate-900">{options?.title}</h2>
            <div className="mt-1 text-sm text-slate-600">{options?.message}</div>
          </div>
        </div>
      </Modal>
    </ConfirmContext.Provider>
  );
}

export function useConfirm(): ConfirmFn {
  const ctx = useContext(ConfirmContext);
  if (!ctx) throw new Error('useConfirm must be used within a ConfirmProvider');
  return ctx;
}
