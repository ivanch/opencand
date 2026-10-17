import React, { useState, useRef, useEffect } from 'react';
import { createPortal } from 'react-dom';

interface TooltipProps {
  content: string;
  children: React.ReactNode;
  position?: 'top' | 'bottom' | 'left' | 'right';
  delay?: number;
  className?: string;
}

const Tooltip: React.FC<TooltipProps> = ({ 
  content, 
  children, 
  position = 'top', 
  delay = 300,
  className = ''
}) => {
  const [isVisible, setIsVisible] = useState(false);
  const [showTooltip, setShowTooltip] = useState(false);
  const [tooltipPosition, setTooltipPosition] = useState({ top: 0, left: 0 });
  const timeoutRef = useRef<number | null>(null);
  const tooltipRef = useRef<HTMLDivElement>(null);
  const triggerRef = useRef<HTMLDivElement>(null);

  const calculateTooltipPosition = () => {
    if (!triggerRef.current) return { top: 0, left: 0 };
    
    const triggerRect = triggerRef.current.getBoundingClientRect();
    
    let top = 0;
    let left = 0;
    
    switch (position) {
      case 'top':
        top = triggerRect.top - 8; // 8px offset for arrow and spacing
        left = triggerRect.left + triggerRect.width / 2;
        break;
      case 'bottom':
        top = triggerRect.bottom + 8;
        left = triggerRect.left + triggerRect.width / 2;
        break;
      case 'left':
        top = triggerRect.top + triggerRect.height / 2;
        left = triggerRect.left - 8;
        break;
      case 'right':
        top = triggerRect.top + triggerRect.height / 2;
        left = triggerRect.right + 8;
        break;
    }
    
    return { top, left };
  };

  const showTooltipWithDelay = () => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }
    
    timeoutRef.current = window.setTimeout(() => {
      const position = calculateTooltipPosition();
      setTooltipPosition(position);
      setIsVisible(true);
      // Small delay to allow for positioning before showing
      setTimeout(() => setShowTooltip(true), 10);
    }, delay);
  };

  const hideTooltip = () => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }
    
    setShowTooltip(false);
    setTimeout(() => setIsVisible(false), 150); // Match transition duration
  };

  // Update tooltip position on scroll or resize
  useEffect(() => {
    if (!isVisible) return;

    const updatePosition = () => {
      const position = calculateTooltipPosition();
      setTooltipPosition(position);
    };

    const handleScroll = () => updatePosition();
    const handleResize = () => updatePosition();

    window.addEventListener('scroll', handleScroll, { passive: true });
    window.addEventListener('resize', handleResize, { passive: true });

    return () => {
      window.removeEventListener('scroll', handleScroll);
      window.removeEventListener('resize', handleResize);
    };
  }, [isVisible, position]);

  useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, []);

  const getTooltipClasses = () => {
    const baseClasses = `
      fixed z-[9999] px-3 py-2 text-sm font-medium text-white bg-gray-900 
      rounded-lg shadow-xl backdrop-blur-sm border border-gray-700
      transition-all duration-150 ease-in-out pointer-events-none
      max-w-xs break-words
    `;

    const transformClasses = {
      top: '-translate-x-1/2 -translate-y-full',
      bottom: '-translate-x-1/2',
      left: '-translate-x-full -translate-y-1/2',
      right: '-translate-y-1/2'
    };

    const visibilityClasses = showTooltip 
      ? 'opacity-100 scale-100' 
      : 'opacity-0 scale-95';

    return `${baseClasses} ${transformClasses[position]} ${visibilityClasses}`;
  };

  const getArrowClasses = () => {
    const baseClasses = 'absolute w-2 h-2 bg-gray-900 border border-gray-700 transform rotate-45';
    
    const arrowPositions = {
      top: 'bottom-0 left-1/2 transform -translate-x-1/2 translate-y-1/2 border-t-0 border-l-0',
      bottom: 'top-0 left-1/2 transform -translate-x-1/2 -translate-y-1/2 border-b-0 border-r-0',
      left: 'right-0 top-1/2 transform translate-x-1/2 -translate-y-1/2 border-l-0 border-b-0',
      right: 'left-0 top-1/2 transform -translate-x-1/2 -translate-y-1/2 border-r-0 border-t-0'
    };

    return `${baseClasses} ${arrowPositions[position]}`;
  };

  if (!content) {
    return <>{children}</>;
  }

  return (
    <>
      <div 
        ref={triggerRef}
        className={`relative inline-block ${className}`}
        onMouseEnter={showTooltipWithDelay}
        onMouseLeave={hideTooltip}
        onFocus={showTooltipWithDelay}
        onBlur={hideTooltip}
      >
        {children}
      </div>
      
      {isVisible && createPortal(
        <div 
          ref={tooltipRef}
          className={getTooltipClasses()}
          style={{
            top: tooltipPosition.top,
            left: tooltipPosition.left,
          }}
          role="tooltip"
          aria-hidden={!showTooltip}
        >
          {content}
          <div className={getArrowClasses()} />
        </div>,
        document.body
      )}
    </>
  );
};

export default Tooltip;
