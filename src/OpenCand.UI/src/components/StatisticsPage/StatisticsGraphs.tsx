import React from 'react';
import type { StatisticsData } from './statisticsRequests';
import type { EnrichmentResponse, ValueSumResponse } from '../../api/apiStatisticsModels';
import WhiteCard from '../../shared/WhiteCard';

interface StatisticsGraphsProps {
  isLoading: boolean;
  error: string | null;
  statisticsData: StatisticsData | null;
}

const StatisticsGraphs: React.FC<StatisticsGraphsProps> = ({
  isLoading,
  error,
  statisticsData,
}) => {
  const [currentEnrichmentIndex, setCurrentEnrichmentIndex] = React.useState(0);
  if (error) {
    return (
      <WhiteCard className="flex items-center justify-center h-64">
        <div className="text-center">
          <div className="text-red-600 text-lg mb-2">⚠️ Erro</div>
          <p className="text-gray-700">{error}</p>
        </div>
      </WhiteCard>
    );
  }

  if (isLoading) {
    return (
      <WhiteCard className="flex items-center justify-center h-64">
        <div className="text-center">
          <div className="animate-spin w-8 h-8 border-2 border-blue-600 border-t-transparent rounded-full mx-auto mb-4"></div>
          <p className="text-gray-700">Carregando dados...</p>
        </div>
      </WhiteCard>
    );
  }

  if (!statisticsData) {
    return (
      <WhiteCard className="flex items-center justify-center h-64">
        <div className="text-center">
          <div className="text-gray-500 text-4xl mb-4">📊</div>
          <h3 className="text-gray-900 text-lg mb-2">Nenhum dado encontrado</h3>
          <p className="text-gray-700">
            Os dados estatísticos não puderam ser carregados
          </p>
        </div>
      </WhiteCard>
    );
  }

  const renderDataTable = (title: string, data: ReadonlyArray<ValueSumResponse | EnrichmentResponse> | null, type: 'candidate' | 'party' | 'state' | 'enrichment') => {
    if (!data || data.length === 0) {
      return (
        <WhiteCard>
          <div>
            <h3 className="text-lg md:text-xl font-semibold text-gray-900 mb-4">{title}</h3>
            <p className="text-gray-700 text-sm md:text-base">Nenhum dado disponível</p>
          </div>
        </WhiteCard>
      );
    }

    if (type === 'enrichment') {
      const enrichmentData = data[currentEnrichmentIndex] as EnrichmentResponse; // Use current index instead of always first item
      const hasMultipleItems = data.length > 1;
      
      const handleNext = () => {
        if (currentEnrichmentIndex < data.length - 1) {
          setCurrentEnrichmentIndex(currentEnrichmentIndex + 1);
        }
      };

      const handleBack = () => {
        if (currentEnrichmentIndex > 0) {
          setCurrentEnrichmentIndex(currentEnrichmentIndex - 1);
        }
      };

      return (
        <WhiteCard>
          <div className="relative">
            <h3 className="text-lg md:text-xl font-semibold text-gray-900 mb-4">{title}</h3>
            <div className="space-y-3 md:space-y-4">
              <div className="flex flex-col sm:flex-row sm:justify-between sm:items-center space-y-1 sm:space-y-0">
                <span className="text-gray-600 text-sm md:text-base font-medium">Candidato:</span>
                <a 
                  href={`/candidato/${enrichmentData.idCandidato}`}
                  className="text-gray-900 font-medium hover:text-blue-600 hover:underline transition-colors duration-200 text-sm md:text-base break-words"
                >
                  {enrichmentData.nome}
                </a>
              </div>
              <div className="flex flex-col sm:flex-row sm:justify-between sm:items-center space-y-1 sm:space-y-0">
                <span className="text-gray-600 text-sm md:text-base">
                  Patrimônio Inicial (<span className="text-xs text-gray-500">{enrichmentData.anoInicial}</span>):
                </span>
                <span className="text-gray-900 font-medium text-sm md:text-base">
                  R$ {enrichmentData.patrimonioInicial?.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) || '0,00'}
                </span>
              </div>
              <div className="flex flex-col sm:flex-row sm:justify-between sm:items-center space-y-1 sm:space-y-0">
                <span className="text-gray-600 text-sm md:text-base">
                  Patrimônio Final (<span className="text-xs text-gray-500">{enrichmentData.anoFinal}</span>):
                </span>
                <span className="text-gray-900 font-medium text-sm md:text-base">
                  R$ {enrichmentData.patrimonioFinal?.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) || '0,00'}
                </span>
              </div>
              <div className="border-t border-gray-200 pt-3">
                <div className="flex flex-col sm:flex-row sm:justify-between sm:items-center space-y-1 sm:space-y-0">
                  <span className="text-gray-600 text-sm md:text-base font-medium">Enriquecimento:</span>
                  <span className={`font-bold text-lg md:text-xl ${
                    enrichmentData.enriquecimento >= 0 ? 'text-green-600' : 'text-red-600'
                  }`}>
                    {enrichmentData.enriquecimento >= 0 ? '+' : ''}
                    R$ {enrichmentData.enriquecimento?.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) || '0,00'}
                  </span>
                </div>
              </div>
            </div>
            
            {/* Navigation buttons */}
            {hasMultipleItems && (
              <div className="flex justify-between items-center mt-4 pt-3 border-t border-gray-200 space-x-2">
                <button
                  onClick={handleBack}
                  disabled={currentEnrichmentIndex === 0}
                  className={`flex items-center justify-center px-2 md:px-3 py-2 rounded-lg text-xs md:text-sm font-medium transition-all duration-200 flex-1 sm:flex-initial ${
                    currentEnrichmentIndex === 0
                      ? 'text-gray-400 cursor-not-allowed opacity-50'
                      : 'text-gray-600 hover:text-gray-900 hover:bg-gray-100 active:bg-gray-200'
                  }`}
                >
                  <svg className="w-3 h-3 md:w-4 md:h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                  </svg>
                  <span className="hidden sm:inline">Anterior</span>
                  <span className="sm:hidden">←</span>
                </button>
                
                <div className="text-xs text-gray-500 font-medium">
                  {currentEnrichmentIndex + 1} de {data.length}
                </div>
                
                <button
                  onClick={handleNext}
                  disabled={currentEnrichmentIndex === data.length - 1}
                  className={`flex items-center justify-center px-2 md:px-3 py-2 rounded-lg text-xs md:text-sm font-medium transition-all duration-200 flex-1 sm:flex-initial ${
                    currentEnrichmentIndex === data.length - 1
                      ? 'text-gray-400 cursor-not-allowed opacity-50'
                      : 'text-gray-600 hover:text-gray-900 hover:bg-gray-100 active:bg-gray-200'
                  }`}
                >
                  <span className="hidden sm:inline">Próximo</span>
                  <span className="sm:hidden">→</span>
                  <svg className="w-3 h-3 md:w-4 md:h-4 ml-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                  </svg>
                </button>
              </div>
            )}
          </div>
        </WhiteCard>
      );
    }

    return (
      <WhiteCard>
        <div>
          <h3 className="text-lg md:text-xl font-semibold text-gray-900 mb-4">{title}</h3>
          <div className="overflow-auto overflow-y-auto custom-scrollbar pr-2">
            <table className="w-full text-sm md:text-base">
              <thead>
                <tr className="border-b border-gray-200">
                  {type === 'candidate' && (
                    <>
                      <th className="text-left py-2 text-gray-600 font-medium">Nome</th>
                    </>
                  )}
                  {type === 'party' && (
                    <th className="text-left py-2 text-gray-600 font-medium">Partido</th>
                  )}
                  {type === 'state' && (
                    <th className="text-left py-2 text-gray-600 font-medium">UF</th>
                  )}
                  <th className="py-2 text-gray-600 font-medium">Ano</th>
                  <th className="py-2 text-gray-600 font-medium">Valor</th>
                </tr>
              </thead>
              <tbody>
                {(data as ValueSumResponse[]).slice(0, 5).map((item, index) => (
                  <tr key={index} className="border-b border-gray-100 hover:bg-gray-50 transition-colors duration-150">
                    {type === 'candidate' && (
                      <a href={`/candidato/${item.idCandidato}`}
                         className="text-left w-100 text-gray-900 font-medium hover:text-blue-600 hover:underline transition-colors duration-200"
                      >
                        <td className="text-left w-100 py-3 text-sm md:text-base">{item.nome || 'N/A'}</td>
                      </a>
                    )}
                    {type === 'party' && (
                      <td className="py-3 text-gray-900 text-sm md:text-base text-left">{item.sgpartido || 'N/A'}</td>
                    )}
                    {type === 'state' && (
                      <td className="py-3 text-gray-900 text-sm md:text-base text-left">{item.siglaUf || 'N/A'}</td>
                    )}
                    <td className="py-3 text-gray-900 text-sm md:text-base">{item.ano || 'N/A'}</td>
                    <td className="py-3 font-medium text-gray-900 text-sm md:text-base">
                      R$ {item.valor?.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) || '0,00'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </WhiteCard>
    );
  };

  return (
    <div className="">
      <div className="space-y-4 md:space-y-6">
        {/* First Row - Candidates */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 md:gap-6">
          {renderDataTable(
            'Candidatos com Maiores Bens',
            statisticsData.candidatesWithMostAssets,
            'candidate'
          )}
          {statisticsData.enrichmentData && renderDataTable(
            'Análise de Enriquecimento',
            statisticsData.enrichmentData,
            'enrichment'
          )}
        </div>

        {/* Second Row - Candidates Revenue & Expenses */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 md:gap-6">
          {renderDataTable(
            'Candidatos com Maiores Receitas',
            statisticsData.candidatesWithMostRevenue,
            'candidate'
          )}
          {renderDataTable(
            'Candidatos com Maiores Despesas',
            statisticsData.candidatesWithMostExpenses,
            'candidate'
          )}
        </div>

        {/* Third Row - Parties */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 md:gap-6">
          {renderDataTable(
            'Partidos com Maiores Bens',
            statisticsData.partiesWithMostAssets,
            'party'
          )}
          {renderDataTable(
            'Partidos com Maiores Despesas',
            statisticsData.partiesWithMostExpenses,
            'party'
          )}
          {renderDataTable(
            'Partidos com Maiores Receitas',
            statisticsData.partiesWithMostRevenue,
            'party'
          )}
        </div>

        {/* Fourth Row - States */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 md:gap-6">
          {renderDataTable(
            'UFs com Maiores Bens',
            statisticsData.statesWithMostAssets,
            'state'
          )}
          {renderDataTable(
            'UFs com Maiores Despesas',
            statisticsData.statesWithMostExpenses,
            'state'
          )}
          {renderDataTable(
            'UFs com Maiores Receitas',
            statisticsData.statesWithMostRevenue,
            'state'
          )}
        </div>
      </div>
    </div>
  );
};

export default StatisticsGraphs;
