import React from 'react';
import { Link, type LinkProps } from 'react-router-dom';

type ButtonVariant = 'ghost' | 'primary' | 'subtle';
type ButtonSize = 'sm' | 'md' | 'lg';

interface ButtonProps {
  children: React.ReactNode;
  className?: string;
  variant?: ButtonVariant;
  size?: ButtonSize;
  hasAnimation?: boolean;
  disableCursor?: boolean;
  disabled?: boolean;
  onClick?: () => void;
  /** Client-side route (react-router) */
  to?: LinkProps['to'];
  /** External or full-page link */
  href?: string;
  target?: string;
  rel?: string;
  type?: 'button' | 'submit' | 'reset';
  'aria-label'?: string;
}

const VARIANT_CLASSES: Record<ButtonVariant, string> = {
  ghost: 'bg-white/[0.06] text-slate-100 ring-1 ring-inset ring-white/10 hover:bg-white/[0.12] hover:ring-white/20 backdrop-blur-sm',
  primary: 'bg-gradient-to-r from-indigo-500 to-violet-600 text-white font-semibold shadow-lg shadow-indigo-900/40 hover:from-indigo-400 hover:to-violet-500',
  subtle: 'bg-slate-100 text-slate-700 hover:bg-slate-200 hover:text-slate-900',
};

const SIZE_CLASSES: Record<ButtonSize, string> = {
  sm: 'px-3 py-1.5 text-sm gap-1.5',
  md: 'px-4 py-2 text-sm gap-2',
  lg: 'px-6 py-3 text-base gap-2',
};

const Button: React.FC<ButtonProps> = ({
  children,
  className = '',
  variant = 'ghost',
  size = 'md',
  hasAnimation = true,
  disableCursor = false,
  disabled = false,
  onClick,
  to,
  href,
  target,
  rel,
  type = 'button',
  'aria-label': ariaLabel,
}) => {
  const animationClasses = hasAnimation ? 'hover:-translate-y-px active:translate-y-0' : '';
  const cursorClasses = disableCursor ? 'cursor-default' : 'cursor-pointer';

  const classes = [
    'inline-flex items-center justify-center rounded-full font-medium whitespace-nowrap',
    'transition-all duration-200 ease-out',
    'disabled:opacity-50 disabled:cursor-not-allowed disabled:translate-y-0',
    VARIANT_CLASSES[variant],
    SIZE_CLASSES[size],
    animationClasses,
    cursorClasses,
    className,
  ].join(' ');

  if (to !== undefined) {
    return (
      <Link to={to} className={classes} aria-label={ariaLabel}>
        {children}
      </Link>
    );
  }

  if (href) {
    return (
      <a href={href} target={target} rel={rel} className={classes} aria-label={ariaLabel}>
        {children}
      </a>
    );
  }

  return (
    <button type={type} onClick={onClick} className={classes} disabled={disabled} aria-label={ariaLabel}>
      {children}
    </button>
  );
};

export default Button;
