import React from 'react';
import { FaGithub, FaLinkedin } from 'react-icons/fa';
import { ArrowTopRightOnSquareIcon, EnvelopeIcon, ExclamationTriangleIcon } from '@heroicons/react/24/outline';
import Panel from '../shared/Panel';
import SectionHeading from '../shared/SectionHeading';
import { usePageTitle } from '../hooks/usePageTitle';

const USEFUL_LINKS = [
  { href: 'https://divulgacandcontas.tse.jus.br/divulga/#/home', label: 'DivulgaCand do TSE', description: 'Consulta oficial de candidaturas e prestação de contas' },
  { href: 'https://sig.tse.jus.br/ords/dwapr/r/seai/sig-eleicao/home', label: 'Estatísticas do TSE', description: 'Painéis estatísticos oficiais das eleições' },
  { href: 'https://dadosabertos.tse.jus.br/dataset/', label: 'Dataset do TSE', description: 'Portal de dados abertos usado pelo OpenCand' },
];

const TECH_STACK = ['.NET', 'PostgreSQL', 'React', 'TypeScript', 'Tailwind CSS', 'Dados abertos do TSE'];

const SectionTitle: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <h2 className="mb-3 text-xs font-semibold uppercase tracking-wider text-indigo-600">{children}</h2>
);

const SobrePage: React.FC = () => {
  usePageTitle('Sobre');

  return (
    <div className="mx-auto max-w-3xl px-4 pb-8 pt-10 sm:px-6 md:pt-14">
      <SectionHeading
        as="h1"
        eyebrow="Sobre o projeto"
        title="Dados eleitorais abertos, fáceis de explorar"
        className="mb-10"
      />

      <Panel padding="p-6 md:p-10" className="space-y-0">
        <div className="space-y-10 text-slate-600 leading-relaxed">
          <section>
            <SectionTitle>O projeto</SectionTitle>
            <p className="text-lg text-slate-800">
              O <strong className="text-slate-900">OpenCand</strong> é uma plataforma que visa explorar de forma intuitiva e fácil os dados eleitorais brasileiros,
              tendo o objetivo de ser uma alternativa para o acesso às informações públicas do Tribunal Superior Eleitoral (TSE).
            </p>
            <p className="mt-4">
              O projeto foi desenvolvido por{' '}
              <strong className="inline-flex items-center gap-1.5 text-slate-900">
                ivanch
                <a
                  href="https://github.com/ivanch"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-slate-500 transition-colors hover:text-slate-900"
                  aria-label="GitHub de ivanch"
                >
                  <FaGithub size={16} />
                </a>
                <a
                  href="https://www.linkedin.com/in/joseivanch"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-blue-600 transition-colors hover:text-blue-800"
                  aria-label="LinkedIn de ivanch"
                >
                  <FaLinkedin size={16} />
                </a>
              </strong>{' '}
              como hobby, onde apenas o código do projeto de ETL (Extract-Transform-Load) e API estão disponíveis no{' '}
              <a href="https://github.com/ivanch/opencand" target="_blank" rel="noopener noreferrer" className="font-medium text-indigo-600 underline-offset-4 hover:underline">
                GitHub
              </a>
              , juntamente com o banco de dados final.
            </p>
            <p className="mt-4">
              A API também pode ser utilizada para acessar os dados de forma pública, porém ainda não está documentada.
            </p>
          </section>

          <section className="rounded-xl bg-amber-50/70 p-5 ring-1 ring-inset ring-amber-200/60">
            <h2 className="mb-2 flex items-center gap-2 text-sm font-semibold text-amber-800">
              <ExclamationTriangleIcon className="h-5 w-5" aria-hidden="true" />
              Aviso
            </h2>
            <p className="text-sm text-amber-900/80">
              Não nos responsabilizamos por quaisquer erros ou inconsistências nos dados apresentados, pois estes são de livre interpretação da plataforma e devem ser verificados com os dados oficiais do TSE.
              A plataforma é uma iniciativa independente e não possui qualquer vínculo com o TSE ou órgãos governamentais.
            </p>
          </section>

          <section>
            <SectionTitle>Links úteis</SectionTitle>
            <ul className="grid gap-3 sm:grid-cols-3">
              {USEFUL_LINKS.map(link => (
                <li key={link.href}>
                  <a
                    href={link.href}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="group flex h-full flex-col rounded-xl p-4 ring-1 ring-inset ring-slate-200 transition-all hover:-translate-y-0.5 hover:bg-indigo-50/40 hover:ring-indigo-200"
                  >
                    <span className="flex items-center justify-between gap-2 font-semibold text-slate-900 group-hover:text-indigo-700">
                      {link.label}
                      <ArrowTopRightOnSquareIcon className="h-4 w-4 shrink-0 text-slate-400 group-hover:text-indigo-500" aria-hidden="true" />
                    </span>
                    <span className="mt-1 text-sm text-slate-500">{link.description}</span>
                  </a>
                </li>
              ))}
            </ul>
          </section>

          <section>
            <SectionTitle>Contato</SectionTitle>
            <p className="flex items-center gap-2 text-slate-800">
              <EnvelopeIcon className="h-5 w-5 text-slate-400" aria-hidden="true" />
              <span>
                {[
                  'opencand',
                  <span key="at" style={{ userSelect: 'text' }}>@</span>,
                  'ivanch',
                  <span key="dot" style={{ userSelect: 'text' }}>.</span>,
                  'me'
                ]}
              </span>
            </p>
          </section>

          <section className="border-t border-slate-100 pt-8">
            <SectionTitle>Desenvolvido com</SectionTitle>
            <ul className="flex flex-wrap gap-2">
              {TECH_STACK.map(tech => (
                <li key={tech} className="rounded-full bg-slate-100 px-3 py-1 text-sm font-medium text-slate-700">
                  {tech}
                </li>
              ))}
            </ul>
          </section>
        </div>
      </Panel>
    </div>
  );
};

export default SobrePage;
