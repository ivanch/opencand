import React from 'react';

interface ButtonProps {
  children: React.ReactNode;
  className?: string;
  hasAnimation?: boolean;
  disableCursor?: boolean;
  disabled?: boolean;
  onClick?: () => void;
  href?: string;
  type?: 'button' | 'submit' | 'reset';
}

const Button: React.FC<ButtonProps> = ({ 
  children, 
  className = "", 
  hasAnimation = true, 
  disableCursor = false, 
  disabled = false,
  onClick,
  href,
  type = 'button'
}) => {
  const animationClasses = hasAnimation ? `hover:shadow-xl
                                           transform
                                           hover:scale-[1.01]`
                                        : "";
  const cursorClasses = disableCursor ? "hover:cursor-default"
                                        : "hover:cursor-pointer";
  
  const baseClasses = `bg-gray-800/30
                       px-4 py-2
                       rounded-full
                       backdrop-blur-xs
                       shadow-xl
                       transition-all
                       duration-200
                       hover:bg-gray-700/40
                       text-white
                       ${animationClasses} ${cursorClasses} ${className}`;
  
  if (href) {
    return (
      <a 
        href={href}
        className={baseClasses}
      >
        {children}
      </a>
    );
  }
  
  return (
    <button 
      type={type}
      onClick={onClick}
      className={baseClasses}
      disabled={disabled}
    >
      {children}
    </button>
  );
};

export default Button;
