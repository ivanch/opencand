import React from 'react';
import Button from '../shared/Button';
import NavbarMatrixBackground from './NavbarMatrixBackground';

const Navbar: React.FC = () => {
  return (
    <>
      {/* Matrix background stays at top of page (not sticky) */}
      <div className="relative bg-gray-900/15 backdrop-blur-sm text-white overflow-hidden border-b border-blue-500/20">
        <NavbarMatrixBackground />
        <div className="relative z-10 p-6 md:p-4">
          <div className="container mx-auto flex flex-col md:flex-row justify-between items-center">
            <a href="/" className="flex items-center gap-2 text-xl md:text-2xl font-bold text-indigo-400 hover:text-indigo-300 transition-colors">
              <img src="/assets/opencand-line.png" alt="OpenCand logo" className="h-8 w-auto" />
              <span>OpenCand</span>
            </a>
            {/* Desktop/Tablet navigation (buttons) */}
            <div className="hidden md:flex md:space-x-4">
              <Button href="/dados-disponiveis">Dados Disponíveis</Button>
              <Button href="/estatisticas">Estatíscas</Button>
              <Button href="/#recursos">Recursos</Button>
              <Button href="/sobre">Sobre</Button>
            </div>
            {/* Mobile navigation (plain text spans) */}
            <nav className="mt-4 w-full md:hidden">
              <div className="flex items-center gap-4">
                <span
                  role="link"
                  tabIndex={0}
                  onClick={() => (window.location.href = '/dados-disponiveis')}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      window.location.href = '/dados-disponiveis';
                    }
                  }}
                >
                  Dados Disponíveis
                </span>
                <span
                  role="link"
                  tabIndex={0}
                  onClick={() => (window.location.href = '/estatisticas')}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      window.location.href = '/estatisticas';
                    }
                  }}
                >
                  Estatíscas
                </span>
                <span
                  role="link"
                  tabIndex={0}
                  onClick={() => (window.location.href = '/#recursos')}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      window.location.href = '/#recursos';
                    }
                  }}
                >
                  Recursos
                </span>
                <span
                  role="link"
                  tabIndex={0}
                  onClick={() => (window.location.href = '/sobre')}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      window.location.href = '/sobre';
                    }
                  }}
                >
                  Sobre
                </span>
              </div>
            </nav>
          </div>
        </div>
      </div>
    </>
  );
};

export default Navbar;
