import React, { useEffect, useState } from 'react';
import { Link, NavLink, useLocation } from 'react-router-dom';
import { Bars3Icon, XMarkIcon } from '@heroicons/react/24/outline';

const NAV_ITEMS = [
  { to: '/dados-disponiveis', label: 'Dados Disponíveis' },
  { to: '/estatisticas', label: 'Estatísticas' },
  { to: '/#recursos', label: 'Recursos' },
  { to: '/sobre', label: 'Sobre' },
];

const linkClasses = (isActive: boolean) =>
  `rounded-full px-4 py-2 text-sm font-medium transition-colors duration-200 ${
    isActive
      ? 'bg-white/10 text-white ring-1 ring-inset ring-white/15'
      : 'text-slate-300 hover:bg-white/5 hover:text-white'
  }`;

const Navbar: React.FC = () => {
  const location = useLocation();
  const [menuOpen, setMenuOpen] = useState(false);
  const [scrolled, setScrolled] = useState(false);

  // Close the mobile menu after navigating
  useEffect(() => {
    setMenuOpen(false);
  }, [location.pathname, location.hash]);

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 8);
    onScroll();
    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  }, []);

  // A hash link ("/#recursos") is never "active" as a page; NavLink would otherwise match "/"
  const isActive = (to: string) => !to.includes('#') && location.pathname.startsWith(to);

  return (
    <header
      className={`sticky top-0 z-40 border-b transition-colors duration-300 ${
        scrolled || menuOpen
          ? 'border-white/10 bg-surface-950/75 backdrop-blur-xl'
          : 'border-transparent bg-transparent'
      }`}
    >
      <nav className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6" aria-label="Principal">
        <Link to="/" className="flex items-center gap-2.5 text-lg font-bold tracking-tight text-white">
          <img src="/assets/opencand-line.png" alt="" className="h-8 w-auto" />
          <span>
            Open<span className="text-indigo-400">Cand</span>
          </span>
        </Link>

        {/* Desktop/Tablet navigation */}
        <div className="hidden items-center gap-1 md:flex">
          {NAV_ITEMS.map(item => (
            <NavLink key={item.to} to={item.to} className={() => linkClasses(isActive(item.to))}>
              {item.label}
            </NavLink>
          ))}
        </div>

        {/* Mobile menu toggle */}
        <button
          type="button"
          className="inline-flex h-10 w-10 items-center justify-center rounded-full text-slate-200 hover:bg-white/10 md:hidden"
          onClick={() => setMenuOpen(open => !open)}
          aria-expanded={menuOpen}
          aria-controls="mobile-menu"
          aria-label={menuOpen ? 'Fechar menu' : 'Abrir menu'}
        >
          {menuOpen ? <XMarkIcon className="h-6 w-6" /> : <Bars3Icon className="h-6 w-6" />}
        </button>
      </nav>

      {/* Mobile navigation */}
      {menuOpen && (
        <div id="mobile-menu" className="border-t border-white/10 px-4 pb-4 pt-2 md:hidden animate-fadeIn">
          <div className="flex flex-col gap-1">
            {NAV_ITEMS.map(item => (
              <NavLink
                key={item.to}
                to={item.to}
                className={() => `${linkClasses(isActive(item.to))} block rounded-xl py-3`}
              >
                {item.label}
              </NavLink>
            ))}
          </div>
        </div>
      )}
    </header>
  );
};

export default Navbar;
