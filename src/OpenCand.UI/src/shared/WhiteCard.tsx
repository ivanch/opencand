import React from 'react';

interface WhiteCardProps {
  children: React.ReactNode;
  className?: string;
  fullHeight?: boolean;
  padding?: string;
}

const WhiteCard: React.FC<WhiteCardProps> = ({
  children,
  className = '',
  fullHeight = false,
  padding = 'py-6 pl-6 pr-4',
}) => {
  return (
    <div className={`
      bg-white/95 border border-white/25 rounded-xl backdrop-blur-sm
      shadow-lg hover:shadow-xl
      ${fullHeight ? 'h-full' : ''}
      ${className}
    `}>
      <div className={`${padding} ${fullHeight ? 'h-full flex flex-col' : ''}`}>
        {children}
      </div>
    </div>
  );
};

export default WhiteCard;
