import React from 'react';
import { Link, type LinkProps } from 'react-router-dom';

interface GradientButtonProps extends React.PropsWithChildren {
  className?: string;
  to?: LinkProps['to'];
  href?: string;
  [x: string]: unknown; // To allow other props like target, rel, type, etc.
}

const GradientButton: React.FC<GradientButtonProps> = ({
  children,
  className = '',
  to,
  href,
  ...props
}) => {
  const baseClasses =
    'bg-gradient-to-r from-indigo-500 to-purple-600 text-white font-semibold rounded-lg hover:from-indigo-600 hover:to-purple-700 transition-all duration-300 transform shadow-lg hover:shadow-xl';

  const combinedClassName = `${baseClasses} ${className}`;

  if (to) {
    return (
      <Link to={to} {...props} className={combinedClassName}>
        {children}
      </Link>
    );
  }

  if (href) {
    return (
      <a href={href} {...props} className={combinedClassName}>
        {children}
      </a>
    );
  }

  return (
    <button type="button" {...props} className={combinedClassName}>
      {children}
    </button>
  );
};

export default GradientButton;