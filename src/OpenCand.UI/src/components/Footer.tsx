import React from 'react';

const Footer: React.FC = () => {
  return (
    <footer className="bg-gray-800/30 text-gray-400 py-4 text-center backdrop-blur-xs shadow-lg ring-1 ring-gray-800">
      <div className="container mx-auto">
        <p className="mb-2 text-md">
          OpenCand
        </p>
        <p className="text-xs">
          Não nos responsabilizamos por quaisquer erros ou inconsistências nos dados apresentados, pois estes são de livre interpretação da plataforma e devem ser verificados com os dados oficiais do TSE.
          <br />
          Logos desenhados por Freepik.
        </p>
      </div>
    </footer>
  );
};

export default Footer;
