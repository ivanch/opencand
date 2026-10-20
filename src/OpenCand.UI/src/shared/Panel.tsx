import React from 'react';

interface PanelProps {
  children: React.ReactNode;
  className?: string;
  /** Padding of the body; the header (if any) always uses the same horizontal padding */
  padding?: string;
  title?: React.ReactNode;
  subtitle?: React.ReactNode;
  icon?: React.ElementType;
  /** Classes for the icon badge, e.g. 'bg-blue-50 text-blue-600' */
  iconClassName?: string;
  /** Right side of the header (buttons, totals...) */
  actions?: React.ReactNode;
  fullHeight?: boolean;
  as?: 'div' | 'section';
}

/** White panel used for the data-heavy content (candidate, statistics, sobre). */
const Panel: React.FC<PanelProps> = ({
  children,
  className = '',
  padding = 'p-5 md:p-6',
  title,
  subtitle,
  icon: Icon,
  iconClassName = 'bg-indigo-50 text-indigo-600',
  actions,
  fullHeight = false,
  as: Tag = 'div',
}) => {
  return (
    <Tag
      className={`rounded-2xl bg-white text-slate-800 shadow-panel ring-1 ring-slate-900/5
        ${fullHeight ? 'h-full flex flex-col' : ''} ${className}`}
    >
      {title && (
        <header className="flex items-center justify-between gap-4 px-5 pt-5 md:px-6 md:pt-6">
          <div className="flex min-w-0 items-center gap-3">
            {Icon && (
              <span className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-xl ${iconClassName}`}>
                <Icon className="h-5 w-5" aria-hidden="true" />
              </span>
            )}
            <div className="min-w-0">
              <h2 className="text-lg font-semibold tracking-tight text-slate-900">{title}</h2>
              {subtitle && <p className="text-sm text-slate-500">{subtitle}</p>}
            </div>
          </div>
          {actions && <div className="shrink-0">{actions}</div>}
        </header>
      )}
      <div className={`${padding} ${fullHeight ? 'flex-1 flex flex-col' : ''}`}>
        {children}
      </div>
    </Tag>
  );
};

export default Panel;
