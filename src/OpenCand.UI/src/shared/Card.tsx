import React from 'react';

interface CardProps {
  children: React.ReactNode;
  className?: string;
  hasAnimation?: boolean;
}

/** Dark translucent "glass" card used on the dark pages (home, dados disponíveis). */
const Card: React.FC<CardProps> = ({ children, className = '', hasAnimation = false }) => {
  const animationClasses = hasAnimation
    ? 'transition-all duration-300 ease-out hover:-translate-y-1 hover:bg-white/[0.06] hover:ring-indigo-400/40 hover:shadow-glow'
    : '';

  return (
    <div
      className={`relative rounded-2xl bg-white/[0.04] p-6 ring-1 ring-inset ring-white/10 backdrop-blur-md shadow-xl shadow-black/20
        before:pointer-events-none before:absolute before:inset-x-6 before:top-0 before:h-px before:bg-gradient-to-r before:from-transparent before:via-white/25 before:to-transparent
        ${animationClasses} ${className}`}
    >
      {children}
    </div>
  );
};

export default Card;
