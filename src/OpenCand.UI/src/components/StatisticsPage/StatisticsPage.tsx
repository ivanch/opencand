import React, { useState, useEffect } from 'react';
import WhiteCard from '../../shared/WhiteCard';
import StatisticsFilters from './StatisticsFilters';
import StatisticsGraphs from './StatisticsGraphs';
import { fetchAllStatisticsData, type StatisticsData, type StatisticsRequestOptions } from './statisticsRequests';
import type { CargoFilter } from '../../api/apiStatisticsModels';

export interface FilterState {
  type: 'bem' | 'despesa' | 'receita';
  groupBy: 'candidato' | 'partido' | 'uf' | 'cargo';
  partido?: string | null;
  uf?: string | null;
  ano?: number | null;
  cargo?: CargoFilter;
}

const StatisticsPage: React.FC = () => {
  const [filters, setFilters] = useState<FilterState>({
    type: 'bem',
    groupBy: 'candidato',
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [statisticsData, setStatisticsData] = useState<StatisticsData | null>(null);

  // Load statistics data when component mounts or filters change
  useEffect(() => {
    const loadStatisticsData = async () => {
      try {
        setIsLoading(true);
        setError(null);
        
        const options: StatisticsRequestOptions = {
          filters: {
            partido: filters.partido,
            uf: filters.uf,
            ano: filters.ano,
            cargo: filters.cargo,
          },
        };

        const data = await fetchAllStatisticsData(options);
        setStatisticsData(data);
      } catch (err) {
        setError('Erro ao carregar dados estatísticos');
        console.error('Error loading statistics data:', err);
      } finally {
        setIsLoading(false);
      }
    };

    loadStatisticsData();
  }, [filters]);

  const handleFiltersChange = (newFilters: FilterState) => {
    setFilters(newFilters);
  };

  return (
    <div className="p-4 md:p-6 pt-16 pl-4 md:pl-16 pr-4 md:pr-16">
      <div className="max-w-full mx-auto">
        <div className="mb-6 md:mb-8">
          <h1 className="text-3xl md:text-4xl lg:text-6xl font-bold mb-4 text-white4">
            Estatísticas
          </h1>
          <p className="text-white/70 text-base md:text-lg">
            Análise de dados e estatísticas dos candidatos e partidos brasileiros
          </p>
          <p className="text-white/70 text-base md:text-lg">
            Para mais informações, acesse a <a href="https://sig.tse.jus.br/ords/dwapr/r/seai/sig-eleicao/home" target="_blank" rel="noopener noreferrer" className="text-indigo-400 hover:underline">página de estatísticas do TSE</a> ou o <a href="https://divulgacandcontas.tse.jus.br/divulga/#/home" target="_blank" rel="noopener noreferrer" className="text-indigo-400 hover:underline">DivulgaCand do TSE</a>.
          </p>
          <div className="w-24 md:w-32 h-1 bg-gradient-to-r from-indigo-400 to-purple-400 mx-auto mt-4 md:mt-6 rounded-full"></div>
        </div>

        <div className="flex flex-col md:flex-row gap-4 md:gap-6">
          {/* Left Sidebar - Filters (full width on mobile, 20% on desktop) */}
          <div className="w-full md:w-1/5 md:min-w-[300px] h-auto md:h-[calc(100vh-18rem)] flex flex-col items-center justify-center">
            <WhiteCard 
              fullHeight 
              className="overflow-y-auto w-80"
            >
              <StatisticsFilters
                filters={filters}
                onFiltersChange={handleFiltersChange}
                isLoading={isLoading}
              />
            </WhiteCard>
          </div>

          {/* Right Content - Graphs (full width on mobile, 80% on desktop) */}
          <div className="flex-1 overflow-auto min-h-[400px] md:min-h-0">
            <StatisticsGraphs
                isLoading={isLoading}
                error={error}
                statisticsData={statisticsData}
              />
          </div>
        </div>
      </div>
    </div>
  );
};

export default StatisticsPage;
