import React from 'react';
import { Link } from 'react-router-dom';
import Button from '../shared/Button';
import { usePageTitle } from '../hooks/usePageTitle';

interface ErrorPageProps {
  title: string;
  description: string;
  helperText?: string;
}

const ErrorPage: React.FC<ErrorPageProps> = ({ title, description, helperText }) => {
  usePageTitle(title);

  return (
    <div className="relative flex min-h-[70vh] items-center justify-center px-4 py-16">
      {/* Decorative glow */}
      <div aria-hidden="true" className="pointer-events-none absolute left-1/2 top-1/2 -z-10 h-96 w-96 -translate-x-1/2 -translate-y-1/2 rounded-full bg-gradient-to-r from-indigo-500/20 to-violet-500/20 blur-3xl" />

      <div className="max-w-lg text-center animate-fade-in-up">
        <h1 className="mb-4 bg-gradient-to-r from-indigo-300 via-indigo-400 to-violet-400 bg-clip-text text-8xl font-extrabold tracking-tight text-transparent md:text-9xl">
          {title}
        </h1>
        <p className="mx-auto mb-8 max-w-md text-lg text-slate-300 text-pretty">
          {description}
        </p>

        <Button to="/" variant="primary" size="lg">
          Voltar para a página inicial
        </Button>

        {helperText && (
          <div className="mt-8">
            <p className="text-sm text-slate-400">{helperText}</p>
            <div className="mt-3 flex justify-center gap-4 text-sm">
              <Link to="/" className="text-indigo-300 transition-colors hover:text-indigo-200">
                Início
              </Link>
              <span className="text-slate-600" aria-hidden="true">•</span>
              <Link to="/estatisticas" className="text-indigo-300 transition-colors hover:text-indigo-200">
                Estatísticas
              </Link>
              <span className="text-slate-600" aria-hidden="true">•</span>
              <Link to="/dados-disponiveis" className="text-indigo-300 transition-colors hover:text-indigo-200">
                Dados Disponíveis
              </Link>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ErrorPage;
