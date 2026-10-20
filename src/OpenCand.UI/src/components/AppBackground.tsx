import React from 'react';

/**
 * Static page background: dark base, two soft glows and a faint grid that fades toward the edges.
 * Pure CSS (replaces the former canvas animations), so it costs nothing while scrolling.
 */
const AppBackground: React.FC = () => (
  <div aria-hidden="true" className="pointer-events-none fixed inset-0 -z-10 overflow-hidden bg-surface-950">
    <div className="absolute -top-48 left-1/2 h-[42rem] w-[72rem] -translate-x-1/2 rounded-full bg-indigo-600/20 blur-[140px]" />
    <div className="absolute top-1/3 -right-40 h-[32rem] w-[32rem] rounded-full bg-violet-600/15 blur-[120px]" />
    <div className="absolute -bottom-40 -left-40 h-[28rem] w-[28rem] rounded-full bg-sky-500/10 blur-[120px]" />
    <div
      className="absolute inset-0 opacity-[0.35]"
      style={{
        backgroundImage:
          'linear-gradient(to right, rgb(148 163 184 / 0.08) 1px, transparent 1px), linear-gradient(to bottom, rgb(148 163 184 / 0.08) 1px, transparent 1px)',
        backgroundSize: '56px 56px',
        maskImage: 'radial-gradient(ellipse 80% 60% at 50% 0%, black 30%, transparent 100%)',
        WebkitMaskImage: 'radial-gradient(ellipse 80% 60% at 50% 0%, black 30%, transparent 100%)',
      }}
    />
  </div>
);

export default AppBackground;
