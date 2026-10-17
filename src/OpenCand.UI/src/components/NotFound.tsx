import React from 'react';
import ErrorPage from './ErrorPage';

const NotFound: React.FC = () => {
  return (
    <ErrorPage
      title="404"
      description="A página que você está procurando não existe ou foi movida para outro local."
      helperText="Ou tente uma dessas páginas:"
    />
  );
};

export default NotFound;
