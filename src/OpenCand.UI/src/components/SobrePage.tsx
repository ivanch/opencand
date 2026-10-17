import React from 'react';
import { FaGithub, FaLinkedin } from 'react-icons/fa';

const SobrePage: React.FC = () => {
  return (
    <div className="flex justify-center items-center min-h-[80vh] py-12 px-4">
      <div
        className="backdrop-blur-md bg-white/90 rounded-2xl shadow-xl p-8 max-w-3xl w-full border border-white/30"
        style={{ boxShadow: '0 8px 32px 0 rgba(31, 38, 135, 0.2)' }}
      >
        <div className="space-y-4 text-gray-700">
          <p className="text-lg leading-relaxed">
            O <strong>OpenCand</strong> é uma plataforma que visa explorar de forma intuitiva e fácil os dados eleitorais brasileiros
            tendo o objetivo de ser uma alternativa para o acesso às informações públicas do Tribunal Superior Eleitoral (TSE).
          </p>

          <p className="leading-relaxed">
            O projeto foi desenvolvido por <strong className="inline-flex items-center gap-1">
              ivanch
              <a
                href="https://github.com/ivanch"
                target="_blank"
                rel="noopener noreferrer"
                className="align-middle inline-block hover:scale-110 transition-transform"
                aria-label="GitHub"
              >
                <FaGithub className="text-gray-700 hover:text-black" size={16} />
              </a>
              <a
                href="https://www.linkedin.com/in/joseivanch"
                target="_blank"
                rel="noopener noreferrer"
                className="align-middle inline-block hover:scale-110 transition-transform"
                aria-label="LinkedIn"
              >
                <FaLinkedin className="text-blue-700 hover:text-blue-900" size={16} />
              </a>
            </strong> como hobby, onde apenas o código do projeto de ETL (Extract-Transform-Load) e API estão disponíveis no <a href="https://github.com/ivanch/opencand" target="_blank" rel="noopener noreferrer">
              <strong>GitHub</strong></a>, juntamente com o banco de dados final.
          </p>

          <p className="leading-relaxed">
            A API também pode ser utilizada para acessar os dados de forma pública, porém ainda não está documentada.
          </p>

          <div className="w-32 h-1 bg-gradient-to-r from-indigo-400 to-purple-400 mx-auto mt-6 rounded-full"></div>

          <p className="leading-relaxed">
            Não nos responsabilizamos por quaisquer erros ou inconsistências nos dados apresentados, pois estes são de livre interpretação da plataforma e devem ser verificados com os dados oficiais do TSE.
            <br />
            A plataforma é uma iniciativa independente e não possui qualquer vínculo com o TSE ou órgãos governamentais.
          </p>

          <div className="w-32 h-1 bg-gradient-to-r from-indigo-400 to-purple-400 mx-auto mt-6 rounded-full"></div>

          <div className="mt-6 flex flex-col items-center">
            <p className="font-semibold text-gray-800 mb-2 text-center">Links úteis:</p>
            <ul className="space-y-2 w-full max-w-md">
              <li className="flex items-center justify-center">
                <span className="w-2 h-2 bg-indigo-500 rounded-full mr-3"></span>
                <a
                  href="https://divulgacandcontas.tse.jus.br/divulga/#/home"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-indigo-700 hover:underline hover:text-indigo-900 transition-colors text-center"
                >
                  DivulgaCand do TSE
                </a>
              </li>
              <li className="flex items-center justify-center">
                <span className="w-2 h-2 bg-indigo-500 rounded-full mr-3"></span>
                <a
                  href="https://sig.tse.jus.br/ords/dwapr/r/seai/sig-eleicao/home"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-indigo-700 hover:underline hover:text-indigo-900 transition-colors text-center"
                >
                  Estatísticas do TSE
                </a>
              </li>
              <li className="flex items-center justify-center">
                <span className="w-2 h-2 bg-indigo-500 rounded-full mr-3"></span>
                <a
                  href="https://dadosabertos.tse.jus.br/dataset/"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-indigo-700 hover:underline hover:text-indigo-900 transition-colors text-center"
                >
                  Dataset do TSE
                </a>
              </li>
            </ul>
          </div>

          <div className="w-32 h-1 bg-gradient-to-r from-indigo-400 to-purple-400 mx-auto mt-6 rounded-full"></div>

          <p className="leading-relaxed">
            Contato: {[
              'opencand',
              <span key="at" style={{ userSelect: 'text' }}>@</span>,
              'ivanch',
              <span key="dot" style={{ userSelect: 'text' }}>.</span>,
              'me'
            ]}
          </p>

          <div className="mt-6 pt-4 border-t border-gray-300">
            <p className="text-sm text-gray-600 text-center">
              Desenvolvido com .NET, PostgreSQL, React (TypeScript/Tailwind CSS) e dados abertos do TSE.<br />
              <span className="text-xs">
              </span>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SobrePage;
