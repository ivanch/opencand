import React, { useState } from 'react';
import SearchBar from './SearchBar';
import RandomCandButton from '../shared/RandomCandButton';

const HeroSection: React.FC = () => {
  const headers = [
    "Explore Dados Eleitorais",
    "Analise Dados Eleitorais",
    "Consulte Dados Eleitorais",
    "Verifique Dados Eleitorais",
    "Descubra Dados Eleitorais",
    "Acesse Informações Eleitorais",
    "Verifique Candidatos",
    "Descubra Candidatos",
    "Pesquise Candidaturas",
    "Consulte Candidatos",
    "Navegue pelos Dados do TSE"
  ];
  const [header] = useState(headers[Math.floor(Math.random() * headers.length)]);

  return (
    <section 
      className="min-h-screen flex flex-col justify-center items-center text-white bg-cover bg-center bg-no-repeat bg-gray-900 relative"
      style={{ backgroundImage: "url('/assets/Congresso_Nacional_hero.jpg')" }}
    >
      <div className="absolute inset-0 bg-black/60"></div>
      <div className="absolute bottom-0 left-0 right-0 h-24 bg-gradient-to-t from-[#0a0f1a] to-transparent"></div>
      <div className="relative z-10 text-center max-w-6xl">
        <h1 className="text-5xl md:text-7xl font-bold mb-6">
          {header}
        </h1>
        <p className="text-lg md:text-xl mb-10 text-gray-300">
          OpenCand oferece acesso fácil e visualizações intuitivas de dados abertos do Tribunal Superior Eleitoral (TSE).
        </p>
        
        <SearchBar />
        
        <div className="mt-6 flex justify-center">
          <RandomCandButton 
            className='hover:bg-white/15 hover:scale-[1.01] transition-all duration-200 duration-300 ease-in-out bg-white/5 backdrop-blur-sm' 
            hasAnimation={false}
          />
        </div>
      </div>
      {/* Image credit */}
      <span className="absolute left-4 bottom-2 text-[10px] text-gray-200 bg-black/30 px-1.5 py-0.5 rounded select-none pointer-events-none opacity-55">
        Edilson Rodrigues/Agência Senado. Senado Federal, 
        <a
          href="https://creativecommons.org/licenses/by/2.0"
          target="_blank"
          rel="noopener noreferrer"
          className="ml-1 text-gray-200 hover:text-white"
        >
          CC BY 2.0
        </a>, via Wikimedia Commons
      </span>
    </section>
  );
};

export default HeroSection;
