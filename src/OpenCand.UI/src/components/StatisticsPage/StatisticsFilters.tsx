import React, { useState, useEffect } from 'react';
import type { FilterState } from './StatisticsPage';
import type { StatisticsConfig } from '../../api/apiStatisticsModels';
import { openCandApi } from '../../api/openCandApi';
import { ChevronUpDownIcon, FunnelIcon } from '@heroicons/react/24/outline';

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

  const handleLocalChange = (key: keyof FilterState, value: FilterState[keyof FilterState]) => {
    setLocalFilters((prev) => ({
      ...prev,
      [key]: value === '' ? null : value,
    }));
  };

  const handleApply = (e: React.FormEvent) => {
    e.preventDefault();
    onFiltersChange(localFilters);
  };

  const disabled = isLoading || configLoading;
  const hasActiveFilters = !!(filters.partido || filters.uf || filters.ano || filters.cargo);

  const handleClear = () => {
    const cleared: FilterState = { ...localFilters, partido: null, uf: null, ano: null, cargo: undefined };
    setLocalFilters(cleared);
    onFiltersChange(cleared);
  };

  const renderSelect = (
    id: string,
    label: string,
    value: string | number,
    onChange: (value: string) => void,
    placeholder: string,
    options: ReadonlyArray<string | number> | undefined,
  ) => (
    <div>
      <label htmlFor={id} className="mb-1.5 block text-xs font-medium uppercase tracking-wider text-slate-500">
        {label}
      </label>
      <div className="relative">
        <select
          id={id}
          value={value}
          onChange={(e) => onChange(e.target.value)}
          disabled={disabled}
          className="w-full cursor-pointer appearance-none rounded-xl border-0 bg-slate-50 py-2.5 pl-3 pr-10 text-sm text-slate-900 ring-1 ring-inset ring-slate-200 transition-shadow hover:ring-slate-300 focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:cursor-not-allowed disabled:opacity-50"
        >
          <option value="">{placeholder}</option>
          {options?.map((option) => (
            <option key={option} value={option}>
              {option}
            </option>
          ))}
        </select>
        <ChevronUpDownIcon className="pointer-events-none absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" aria-hidden="true" />
      </div>
    </div>
  );

  return (
    <form className="flex flex-col gap-5" onSubmit={handleApply}>
      <div className="flex items-center justify-between">
        <h2 className="flex items-center gap-2 text-base font-semibold text-slate-900">
          <FunnelIcon className="h-4 w-4 text-indigo-600" aria-hidden="true" />
          Filtros
        </h2>
        {hasActiveFilters && (
          <button
            type="button"
            onClick={handleClear}
            disabled={disabled}
            className="text-xs font-medium text-indigo-600 hover:text-indigo-500 disabled:opacity-50"
          >
            Limpar filtros
          </button>
        )}
      </div>

      {configError && (
        <div className="rounded-xl bg-rose-50 p-3 text-sm text-rose-700 ring-1 ring-inset ring-rose-200">
          {configError}
        </div>
      )}

      {renderSelect('filter-partido', 'Partido', localFilters.partido || '', (value) => handleLocalChange('partido', value), 'Todos os Partidos', config?.partidos)}
      {renderSelect('filter-uf', 'UF', localFilters.uf || '', (value) => handleLocalChange('uf', value), 'Todos os Estados', config?.siglasUF)}
      {renderSelect('filter-ano', 'Ano', localFilters.ano || '', (value) => handleLocalChange('ano', value ? Number(value) : null), 'Todos os Anos', config?.anos)}
      {renderSelect('filter-cargo', 'Cargo', localFilters.cargo || '', (value) => handleLocalChange('cargo', value), 'Todos os Cargos', config?.cargos)}

      <button
        type="submit"
        disabled={disabled}
        className="mt-1 w-full rounded-xl bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {disabled ? 'Carregando...' : 'Aplicar Filtros'}
      </button>
    </form>
  );
};

export default StatisticsFilters;
