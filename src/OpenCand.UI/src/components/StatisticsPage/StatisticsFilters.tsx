import React, { useState, useEffect } from 'react';
import type { FilterState } from './StatisticsPage';
import type { CargoFilter, StatisticsConfig } from '../../api/apiStatisticsModels';
import { openCandApi } from '../../api/openCandApi';
import Button from '../../shared/Button';

interface StatisticsFiltersProps {
  filters: FilterState;
  onFiltersChange: (filters: FilterState) => void;
  isLoading?: boolean;
}

const StatisticsFilters: React.FC<StatisticsFiltersProps> = ({
  filters,
  onFiltersChange,
  isLoading = false,
}) => {
  // Local state for form fields
  const [localFilters, setLocalFilters] = useState<FilterState>(filters);
  // State for configuration data from API
  const [config, setConfig] = useState<StatisticsConfig | null>(null);
  const [configLoading, setConfigLoading] = useState(true);
  const [configError, setConfigError] = useState<string | null>(null);

  // Sync local state when parent filters change
  useEffect(() => {
    setLocalFilters(filters);
  }, [filters]);

  // Fetch configuration data on component mount
  useEffect(() => {
    const fetchConfig = async () => {
      try {
        setConfigLoading(true);
        setConfigError(null);
        const configData = await openCandApi.getStatisticsConfig();
        setConfig(configData);
      } catch (error) {
        console.error('Error fetching statistics config:', error);
        setConfigError('Erro ao carregar configurações');
      } finally {
        setConfigLoading(false);
      }
    };

    fetchConfig();
  }, []);

  const handleLocalChange = (key: keyof FilterState, value: any) => {
    setLocalFilters((prev) => ({
      ...prev,
      [key]: value === '' ? null : value,
    }));
  };

  const handleApply = (e: React.FormEvent) => {
    e.preventDefault();
    onFiltersChange(localFilters);
  };

  return (
    <form
      className="h-full flex flex-col justify-between space-y-4 md:space-y-6"
      onSubmit={handleApply}
      style={{ minHeight: '300px' }} // Reduced min-height for mobile
    >
      <div className="flex-1 flex flex-col space-y-4 md:space-y-6">
        <div>
          <h2 className="text-lg md:text-xl font-semibold text-gray-900 mb-3 md:mb-4">
            Filtros
          </h2>
          {configError && (
            <div className="mb-3 md:mb-4 p-2 md:p-3 bg-red-100 border border-red-300 rounded-lg text-red-700 text-xs md:text-sm">
              {configError}
            </div>
          )}
        </div>

        {/* Party Filter */}
        <div className="space-y-1 md:space-y-2">
          <label className="block text-xs md:text-sm font-semibold text-gray-600 uppercase tracking-wide">
            Partido
          </label>
          <select
            value={localFilters.partido || ''}
            onChange={(e) => handleLocalChange('partido', e.target.value)}
            disabled={isLoading || configLoading}
            className="w-50 px-3 py-2 md:px-3 md:py-2 bg-white border border-gray-300 rounded-lg text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed text-sm md:text-base"
          >
            <option value="">Todos os Partidos</option>
            {config?.partidos.map((partido) => (
              <option key={partido} value={partido}>
                {partido}
              </option>
            ))}
          </select>
        </div>

        {/* UF Filter */}
        <div className="space-y-1 md:space-y-2">
          <label className="block text-xs md:text-sm font-semibold text-gray-600 uppercase tracking-wide">
            UF
          </label>
          <select
            value={localFilters.uf || ''}
            onChange={(e) => handleLocalChange('uf', e.target.value)}
            disabled={isLoading || configLoading}
            className="w-50 px-3 py-2 md:px-3 md:py-2 bg-white border border-gray-300 rounded-lg text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed text-sm md:text-base"
          >
            <option value="">Todos os Estados</option>
            {config?.siglasUF.map((uf) => (
              <option key={uf} value={uf}>
                {uf}
              </option>
            ))}
          </select>
        </div>

        {/* Year Filter */}
        <div className="space-y-1 md:space-y-2">
          <label className="block text-xs md:text-sm font-semibold text-gray-600 uppercase tracking-wide">
            Ano
          </label>
          <select
            value={localFilters.ano || ''}
            onChange={(e) => handleLocalChange('ano', e.target.value ? Number(e.target.value) : null)}
            disabled={isLoading || configLoading}
            className="w-50 px-3 py-2 md:px-3 md:py-2 bg-white border border-gray-300 rounded-lg text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed text-sm md:text-base"
          >
            <option value="">Todos os Anos</option>
            {config?.anos.map((year) => (
              <option key={year} value={year}>
                {year}
              </option>
            ))}
          </select>
        </div>

        {/* Cargo Filter */}
        <div className="space-y-1 md:space-y-2">
          <label className="block text-xs md:text-sm font-semibold text-gray-600 uppercase tracking-wide">
            Cargo
          </label>
          <select
            value={localFilters.cargo || ''}
            onChange={(e) => handleLocalChange('cargo', e.target.value)}
            disabled={isLoading || configLoading}
            className="w-50 px-3 py-2 md:px-3 md:py-2 bg-white border border-gray-300 rounded-lg text-gray-900 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed text-sm md:text-base"
          >
            <option value="">Todos os Cargos</option>
            {config?.cargos.map((cargo) => (
              <option key={cargo} value={cargo}>
                {cargo}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Apply Filters Button */}
      <div className="pt-3 md:pt-4">
        <button
          type="submit"
          disabled={isLoading || configLoading}
          className="w-60 px-3 py-2 md:px-4 md:py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg shadow-md hover:shadow-lg duration-200 disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none hover:cursor-pointer text-sm md:text-base"
        >
          {isLoading || configLoading ? 'Carregando...' : 'Aplicar Filtros'}
        </button>
      </div>
    </form>
  );
};

export default StatisticsFilters;
