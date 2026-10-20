import React, { useState } from 'react';
import SearchBar from './SearchBar';
import RandomCandButton from '../shared/RandomCandButton';

// [verb, highlighted part] — the highlighted part gets the accent gradient
const HEADERS: Array<[string, string]> = [
  ['Explore', 'Dados Eleitorais'],
  ['Analise', 'Dados Eleitorais'],
  ['Consulte', 'Dados Eleitorais'],
  ['Verifique', 'Dados Eleitorais'],
  ['Descubra', 'Dados Eleitorais'],
  ['Acesse', 'Informações Eleitorais'],
  ['Verifique', 'Candidatos'],
  ['Descubra', 'Candidatos'],
  ['Pesquise', 'Candidaturas'],
  ['Consulte', 'Candidatos'],
  ['Navegue pelos', 'Dados do TSE'],
];

const HeroSection: React.FC = () => {
  const [[verb, highlight]] = useState(() => HEADERS[Math.floor(Math.random() * HEADERS.length)]);

  return (
    <section className="relative -mt-16 flex min-h-[92vh] flex-col items-center justify-center px-4 pb-24 pt-32 text-center text-white">
      {/* Background photo, darkened and faded into the page background */}
      <div
        aria-hidden="true"
        className="absolute inset-0 bg-cover bg-center bg-no-repeat"
        style={{ backgroundImage: "url('/assets/Congresso_Nacional_hero.jpg')" }}
      />
      <div aria-hidden="true" className="absolute inset-0 bg-gradient-to-b from-surface-950/80 via-surface-950/60 to-surface-950" />
      <div aria-hidden="true" className="absolute inset-0 bg-[radial-gradient(ellipse_at_center,rgb(79_70_229/0.25),transparent_60%)]" />

      <div className="relative z-20 w-full max-w-4xl animate-fade-in-up">
        <span className="mb-6 inline-flex items-center gap-2 rounded-full bg-white/5 px-4 py-1.5 text-xs font-medium text-indigo-200 ring-1 ring-inset ring-white/15 backdrop-blur-sm">
          <span className="h-1.5 w-1.5 rounded-full bg-emerald-400 shadow-[0_0_8px] shadow-emerald-400" />
          Dados abertos do Tribunal Superior Eleitoral
        </span>

        <h1 className="text-5xl font-extrabold tracking-tight text-balance md:text-7xl">
          {verb}{' '}
          <span className="bg-gradient-to-r from-indigo-300 via-violet-300 to-sky-300 bg-clip-text text-transparent">
            {highlight}
          </span>
        </h1>
        <p className="mx-auto mb-10 mt-6 max-w-2xl text-lg text-slate-300 text-pretty md:text-xl">
          OpenCand oferece acesso fácil e visualizações intuitivas de dados abertos do Tribunal Superior Eleitoral (TSE).
        </p>

        <SearchBar />

        <div className="mt-6 flex flex-wrap items-center justify-center gap-3 text-sm text-slate-400">
          <span>ou</span>
          <RandomCandButton />
        </div>
      </div>

      {/* Image credit */}
      <span className="absolute bottom-2 left-4 z-10 select-none rounded bg-black/30 px-1.5 py-0.5 text-[10px] text-slate-300 opacity-60">
        Edilson Rodrigues/Agência Senado. Senado Federal,
        <a
          href="https://creativecommons.org/licenses/by/2.0"
          target="_blank"
          rel="noopener noreferrer"
          className="ml-1 text-slate-200 hover:text-white"
        >
          CC BY 2.0
        </a>, via Wikimedia Commons
      </span>
    </section>
  );
};

export default HeroSection;
