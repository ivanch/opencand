import React from 'react';

interface SectionHeadingProps {
  eyebrow?: string;
  title: React.ReactNode;
  subtitle?: React.ReactNode;
  align?: 'center' | 'left';
  /** h1 for page titles, h2 for sections of a page */
  as?: 'h1' | 'h2';
  className?: string;
}

/** Title block on the dark background: small eyebrow, big title, muted subtitle. */
const SectionHeading: React.FC<SectionHeadingProps> = ({
  eyebrow,
  title,
  subtitle,
  align = 'center',
  as: Heading = 'h2',
  className = '',
}) => {
  const isCenter = align === 'center';
  const titleSize = Heading === 'h1' ? 'text-4xl md:text-5xl' : 'text-3xl md:text-4xl';

  return (
    <div className={`${isCenter ? 'text-center mx-auto' : 'text-left'} max-w-3xl ${className}`}>
      {eyebrow && (
        <p className="mb-3 text-xs font-semibold uppercase tracking-[0.2em] text-indigo-300">
          {eyebrow}
        </p>
      )}
      <Heading className={`${titleSize} font-bold tracking-tight text-white text-balance`}>
        {title}
      </Heading>
      {subtitle && (
        <p className="mt-4 text-base md:text-lg text-slate-400 text-pretty">
          {subtitle}
        </p>
      )}
    </div>
  );
};

export default SectionHeading;
