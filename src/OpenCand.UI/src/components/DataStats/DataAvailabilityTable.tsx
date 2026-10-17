import React from 'react';
import type { OpenCandDataAvailabilityStats } from '../../api/apiModels';

interface DataAvailabilityTableProps {
  stats: OpenCandDataAvailabilityStats;
  sortedYears: number[];
}

const dataTypes = [
  { key: 'candidatos', label: 'Candidatos', icon: '👤' },
  { key: 'bemCandidatos', label: 'Bens de Candidatos', icon: '💰' },
  { key: 'despesaCandidatos', label: 'Despesas de Candidatos', icon: '💸' },
  { key: 'receitaCandidatos', label: 'Receitas de Candidatos', icon: '💵' },
  { key: 'redeSocialCandidatos', label: 'Redes Sociais', icon: '📱' },
  { key: 'fotosCandidatos', label: 'Fotos de Candidatos (API)', icon: '📸' },
];

const DataAvailabilityTable: React.FC<DataAvailabilityTableProps> = ({ stats, sortedYears }) => {
  return (
    <div className="bg-gray-800/10 backdrop-blur-xs rounded-xl shadow-lg hover:shadow-xl transform hover:scale-[1.005] transition-all duration-200 overflow-hidden ring-1 ring-gray-700 hover:ring-indigo-300">
      <div className="p-6 border-b border-gray-700/30 bg-gray-800/10">
        <h2 className="text-2xl font-bold text-white">
          Matriz de Disponibilidade
        </h2>
        <p className="text-gray-400 mt-2">
          ✅ Disponível • ❌ Não Disponível
        </p>
      </div>
      
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead>
            <tr className="bg-gray-800/10">
              <th className="text-left p-4 text-white font-semibold border-b border-gray-700/30 left-0 bg-gray-800/10">
                Tipo de Dado
              </th>
              {sortedYears.map((year, index) => (
                <th 
                  key={year} 
                  className="text-center p-4 text-white font-semibold border-b border-gray-700/30 animate-slide-in-left"
                  style={{ animationDelay: `${index * 50}ms` }}
                >
                  {year}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {dataTypes.map((dataType, rowIndex) => (
              <tr 
                key={dataType.key}
                className="hover:bg-gray-800/10 transition-all duration-300 animate-slide-in-left"
                style={{ animationDelay: `${rowIndex * 100}ms` }}
              >
                <td className="p-4 border-b border-gray-700/20 text-white left-0 bg-gray-800/10">
                  <div className="flex items-center space-x-3">
                    <span className="text-xl">{dataType.icon}</span>
                    <span>{dataType.label}</span>
                  </div>
                </td>
                {sortedYears.map((year) => {
                  const isAvailable = (stats[dataType.key as keyof OpenCandDataAvailabilityStats] as number[]).includes(year);
                  return (
                    <td key={year} className="text-center p-4 border-b border-gray-700/20">
                      <div className={`inline-flex items-center justify-center w-8 h-8 rounded-full transition-all duration-100 ${isAvailable ? 'bg-green-500/20 text-green-300 hover:bg-green-500/30 hover:scale-110 hover:cursor-default' : 'bg-red-500/20 text-red-300 hover:bg-red-500/30 hover:cursor-default'}`}>
                        {isAvailable ? '✅' : '❌'}
                      </div>
                    </td>
                  );
                })}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default DataAvailabilityTable;