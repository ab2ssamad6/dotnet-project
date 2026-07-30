import { useCallback, useState } from 'react';

export function useDisclosure<T = undefined>() {
  const [isOpen, setIsOpen] = useState(false);
  const [payload, setPayload] = useState<T | undefined>(undefined);

  const open = useCallback((data?: T) => {
    setPayload(data);
    setIsOpen(true);
  }, []);

  const close = useCallback(() => setIsOpen(false), []);

  return { isOpen, payload, open, close };
}
