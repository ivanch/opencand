import React from 'react';
import { DocumentTextIcon } from '@heroicons/react/24/outline';
import { type Election } from '../../api';
import Tooltip from '../../shared/Tooltip';

interface ElectionsComponentProps {
  elections: Election[] | null;
  isLoading: boolean;
}

const ElectionsComponent: React.FC<ElectionsComponentProps> = ({ elections, isLoading }) => {
  const getResultadoHelper = (resultado: string | null) => {
    resultado = resultado?.toLowerCase() || '';
    if (resultado == 'eleito') return 'Eleito!';
    if (resultado == 'não eleito') return 'Não Eleito';
    if (resultado == 'eleito por qp') return 'Eleito por Quociente Partidário';
    if (resultado.includes('nulo')) return 'Dado não disponível';
    if (resultado == '2º turno') return 'Foi para o segundo Turno';
    if (resultado == 'suplente') return 'Foi eleito como suplente';
    return '';
  }


  return (
    <div className="bg-white/95 backdrop-blur-sm rounded-xl shadow-lg hover:shadow-xl transform hover:scale-[1.01] transition-all duration-200 p-6">
      <div className="flex items-center mb-6">
        <DocumentTextIcon className="h-8 w-8 text-green-600 mr-3" />
        <h2 className="text-2xl font-bold text-gray-900">Histórico de Eleições</h2>
      </div>

      {isLoading ? (
        <div className="flex items-center justify-center h-32">
          <div className="animate-spin rounded-full h-8 w-8 border-4 border-green-600 border-t-transparent"></div>
        </div>
      ) : elections && elections.length > 0 ? (
        <div className="overflow-x-auto">
          <table className="w-full table-auto">
            <thead>
              <tr className="border-b border-gray-200">
                <th className="text-left py-3 px-2 font-semibold text-gray-700">Ano</th>
                <th className="text-left py-3 px-2 font-semibold text-gray-700">Tipo</th>
                <th className="text-left py-3 px-2 font-semibold text-gray-700">Cargo</th>
                <th className="text-left py-3 px-2 font-semibold text-gray-700">UF</th>
                <th className="text-left py-3 px-2 font-semibold text-gray-700">Localidade</th>
                <th className="text-left py-3 px-2 font-semibold text-gray-700">Partido</th>
                <th className="text-left py-3 px-2 font-semibold text-gray-700">Número</th>
                <th className="text-left py-3 px-2 font-semibold text-gray-700">Resultado</th>
              </tr>
            </thead>
            <tbody>
              {elections.map((election: Election, index: number) => (
                <tr key={election.sqCandidato || index} className="border-b border-gray-100 hover:bg-gray-50 transition-colors">
                  <td className="py-3 px-2 text-gray-900">{election.ano}</td>
                  <td className="py-3 px-2 text-gray-900">{election.tipoEleicao}</td>
                  <td className="py-3 px-2 text-gray-900">{election.cargo}</td>
                  <td className="py-3 px-2 text-gray-900">{election.siglaUF}</td>
                  <td className="py-3 px-2 text-gray-900">{election.nomeUE}</td>
                  <td className="py-3 px-2 text-gray-900">{election.partido.sigla}</td>
                  <td className="py-3 px-2 text-gray-900">{election.nrCandidato}</td>
                  <td className="py-3 px-2">
                    <Tooltip 
                      content={getResultadoHelper(election.resultado)}
                      position="top"
                      delay={200}
                    >
                      <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                        election.resultado?.toLowerCase() === 'eleito'
                          ? 'bg-green-100 text-green-800' 
                          : 'bg-gray-100 text-gray-800'
                      }`}>
                        {election.resultado}
                      </span>
                    </Tooltip>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <div className="text-center text-gray-500 py-8">
          Nenhum histórico de eleições encontrado
        </div>
      )}
    </div>
  );
};

export default ElectionsComponent;
