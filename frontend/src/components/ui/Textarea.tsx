import { forwardRef, useId, type TextareaHTMLAttributes } from 'react';
import { cn } from '@/utils/cn';
import { Icons } from './Icon';
import { fieldBase, fieldError, fieldHint, fieldLabel, fieldTone } from './field';

export interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string;
  error?: string;
  hint?: string;
}

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(function Textarea(
  { label, error, hint, className, id, rows = 4, ...props },
  ref,
) {
  const autoId = useId();
  const textId = id ?? autoId;
  return (
    <div className="w-full">
      {label && (
        <label htmlFor={textId} className={fieldLabel}>
          {label}
          {props.required && <span className="ml-0.5 text-brand-600">*</span>}
        </label>
      )}
      <textarea
        ref={ref}
        id={textId}
        rows={rows}
        aria-invalid={!!error}
        className={cn(fieldBase, fieldTone(!!error), 'resize-y px-3.5 py-2.5 leading-relaxed', className)}
        {...props}
      />
      {error ? (
        <p className={fieldError}>
          <Icons.alert size={13} /> {error}
        </p>
      ) : hint ? (
        <p className={fieldHint}>{hint}</p>
      ) : null}
    </div>
  );
});
