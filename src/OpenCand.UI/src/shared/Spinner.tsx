import React from 'react';

interface SpinnerProps {
  size?: 'sm' | 'md' | 'lg';
  /** Border color class of the moving arc, e.g. 'border-indigo-500' */
  colorClassName?: string;
  label?: string;
  className?: string;
}

const SIZE_CLASSES = {
  sm: 'h-4 w-4 border-2',
  md: 'h-6 w-6 border-[3px]',
  lg: 'h-10 w-10 border-4',
};

const Spinner: React.FC<SpinnerProps> = ({
  size = 'md',
  colorClassName = 'border-indigo-500',
  label = 'Carregando',
  className = '',
}) => (
  <span role="status" aria-label={label} className={`inline-block ${className}`}>
    <span
      className={`block animate-spin rounded-full ${SIZE_CLASSES[size]} ${colorClassName}`}
      style={{ borderTopColor: 'transparent' }}
    />
  </span>
);

/** Spinner centered inside a block of the given height */
export const LoadingBlock: React.FC<SpinnerProps & { heightClassName?: string; text?: string }> = ({
  heightClassName = 'h-32',
  text,
  ...spinnerProps
}) => (
  <div className={`flex flex-col items-center justify-center gap-3 ${heightClassName}`}>
    <Spinner size="lg" {...spinnerProps} />
    {text && <p className="text-sm text-slate-500">{text}</p>}
  </div>
);

export default Spinner;
