import React from 'react';

interface CardProps {
  children: React.ReactNode;
  className?: string;
  hasAnimation?: boolean;
  disableCursor?: boolean;
  height?: number;
  width?: number;
}

const Card: React.FC<CardProps> = ({ children, className = "", hasAnimation = false, disableCursor = false, height, width }) => {
  const animationClasses = hasAnimation ? "hover:shadow-xl transform hover:scale-[1.01] transition-all duration-200 hover:ring-indigo-300" : "";
  const cursorClasses = disableCursor ? "hover:cursor-default" : "";
  
  const sizeStyles = {
    ...(height && { height: `${height}rem` }),
    ...(width && { width: `${width}rem` })
  };
  
  return (
    <div 
      className={`bg-gray-800/30 p-6 rounded-lg backdrop-blur-xs shadow-xl ring-1 ring-gray-700 max-w-md ${animationClasses} ${cursorClasses} ${className}`}
      style={sizeStyles}
    >
      {children}
    </div>
  );
};

export default Card;
