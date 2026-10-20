import React, { useState, useEffect } from 'react';
import Panel from '../../shared/Panel';
import SectionHeading from '../../shared/SectionHeading';
import { usePageTitle } from '../../hooks/usePageTitle';
import StatisticsFilters from './StatisticsFilters';
import StatisticsGraphs from './StatisticsGraphs';
import { fetchAllStatisticsData, type StatisticsData, type StatisticsRequestOptions } from './statisticsRequests';
import { isRequestCancelled } from '../../api/base';
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

  usePageTitle('Estatísticas');

  // Load statistics data when component mounts or filters change
  useEffect(() => {
    // Cancelled in the cleanup, so a newer filter change aborts the in-flight batch
    // and a stale response can never overwrite newer data.
    const controller = new AbortController();

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
          signal: controller.signal,
        };

        const data = await fetchAllStatisticsData(options);
        if (controller.signal.aborted) return;
        setStatisticsData(data);
      } catch (err) {
        if (controller.signal.aborted || isRequestCancelled(err)) return;
        setError('Erro ao carregar dados estatísticos');
        console.error('Error loading statistics data:', err);
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    };

    loadStatisticsData();
    return () => controller.abort();
  }, [filters]);

  const handleFiltersChange = (newFilters: FilterState) => {
    setFilters(newFilters);
  };

  const linkClass = 'text-indigo-300 underline-offset-4 hover:text-indigo-200 hover:underline';

  return (
    <div className="mx-auto w-full max-w-[96rem] px-4 pb-8 pt-10 sm:px-6 md:pt-14">
      <SectionHeading
        as="h1"
        align="left"
        eyebrow="Análises"
        title="Estatísticas"
        subtitle={
          <>
            Análise de dados e estatísticas dos candidatos e partidos brasileiros. Para mais informações, acesse a{' '}
            <a href="https://sig.tse.jus.br/ords/dwapr/r/seai/sig-eleicao/home" target="_blank" rel="noopener noreferrer" className={linkClass}>página de estatísticas do TSE</a>{' '}
            ou o{' '}
            <a href="https://divulgacandcontas.tse.jus.br/divulga/#/home" target="_blank" rel="noopener noreferrer" className={linkClass}>DivulgaCand do TSE</a>.
          </>
        }
        className="mb-8 md:mb-10"
      />

      <div className="flex flex-col gap-6 lg:flex-row">
        {/* Sidebar - Filters */}
        <aside className="w-full shrink-0 lg:w-72">
          <Panel className="lg:sticky lg:top-24">
            <StatisticsFilters
              filters={filters}
              onFiltersChange={handleFiltersChange}
              isLoading={isLoading}
            />
          </Panel>
        </aside>

        {/* Content - Rankings */}
        <div className="min-w-0 flex-1">
          <StatisticsGraphs
            isLoading={isLoading}
            error={error}
            statisticsData={statisticsData}
          />
        </div>
      </div>
    </div>
  );
};

export default StatisticsPage;
