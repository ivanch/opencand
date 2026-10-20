import React from 'react';
import { Link } from 'react-router-dom';
import { FaGithub } from 'react-icons/fa';
import { analyticsAvailable } from '../config/firebase';
import { OPEN_CONSENT_EVENT } from '../config/consent';

const linkClass = 'text-slate-400 hover:text-white transition-colors';

const Footer: React.FC = () => {
  return (
    <footer className="mt-16 border-t border-white/10 bg-surface-950/60 backdrop-blur-sm">
      <div className="mx-auto grid max-w-7xl gap-10 px-4 py-12 sm:px-6 md:grid-cols-[2fr_1fr_1fr]">
        <div className="max-w-md">
          <Link to="/" className="flex items-center gap-2 text-lg font-bold text-white">
            <img src="/assets/opencand-line.png" alt="" className="h-7 w-auto" />
            <span>
              Open<span className="text-indigo-400">Cand</span>
            </span>
          </Link>
          <p className="mt-4 text-sm leading-relaxed text-slate-400">
            Não nos responsabilizamos por quaisquer erros ou inconsistências nos dados apresentados, pois estes são de livre
            interpretação da plataforma e devem ser verificados com os dados oficiais do TSE.
          </p>
        </div>

        <div>
          <h2 className="text-xs font-semibold uppercase tracking-wider text-slate-300">Navegação</h2>
          <ul className="mt-4 space-y-2.5 text-sm">
            <li><Link to="/" className={linkClass}>Início</Link></li>
            <li><Link to="/estatisticas" className={linkClass}>Estatísticas</Link></li>
            <li><Link to="/dados-disponiveis" className={linkClass}>Dados Disponíveis</Link></li>
            <li><Link to="/sobre" className={linkClass}>Sobre</Link></li>
          </ul>
        </div>

        <div>
          <h2 className="text-xs font-semibold uppercase tracking-wider text-slate-300">Projeto</h2>
          <ul className="mt-4 space-y-2.5 text-sm">
            <li>
              <a href="https://github.com/ivanch/opencand" target="_blank" rel="noopener noreferrer" className={`${linkClass} inline-flex items-center gap-2`}>
                <FaGithub aria-hidden="true" /> GitHub
              </a>
            </li>
            <li>
              <a href="https://dadosabertos.tse.jus.br/dataset/" target="_blank" rel="noopener noreferrer" className={linkClass}>
                Dados abertos do TSE
              </a>
            </li>
            {analyticsAvailable && (
              <li>
                <button
                  type="button"
                  onClick={() => window.dispatchEvent(new Event(OPEN_CONSENT_EVENT))}
                  className={`${linkClass} cursor-pointer`}
                >
                  Preferências de privacidade
                </button>
              </li>
            )}
          </ul>
        </div>
      </div>

      <div className="border-t border-white/5">
        <div className="mx-auto flex max-w-7xl flex-col gap-2 px-4 py-5 text-xs text-slate-500 sm:flex-row sm:items-center sm:justify-between sm:px-6">
          <p>© {new Date().getFullYear()} OpenCand · Iniciativa independente, sem vínculo com o TSE.</p>
          <p>Logos desenhados por Freepik.</p>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
