import React from 'react';
import { CheckIcon, MinusIcon } from '@heroicons/react/24/outline';
import type { OpenCandDataAvailabilityStats } from '../../api/apiModels';
import { dataTypes } from './dataTypes';

interface DataAvailabilityTableProps {
  stats: OpenCandDataAvailabilityStats;
  sortedYears: number[];
}

const DataAvailabilityTable: React.FC<DataAvailabilityTableProps> = ({ stats, sortedYears }) => {
  return (
    <div className="overflow-hidden rounded-2xl bg-white/[0.04] ring-1 ring-inset ring-white/10 backdrop-blur-md">
      <div className="flex flex-col gap-3 border-b border-white/10 p-6 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-xl font-semibold text-white">Matriz de Disponibilidade</h2>
          <p className="mt-1 text-sm text-slate-400">Quais dados existem para cada ano eleitoral</p>
        </div>
        <div className="flex gap-2 text-xs">
          <span className="inline-flex items-center gap-1.5 rounded-full bg-emerald-500/10 px-2.5 py-1 text-emerald-300 ring-1 ring-inset ring-emerald-400/20">
            <CheckIcon className="h-3.5 w-3.5" aria-hidden="true" /> Disponível
          </span>
          <span className="inline-flex items-center gap-1.5 rounded-full bg-white/5 px-2.5 py-1 text-slate-400 ring-1 ring-inset ring-white/10">
            <MinusIcon className="h-3.5 w-3.5" aria-hidden="true" /> Não disponível
          </span>
        </div>
      </div>

      <div className="overflow-x-auto custom-scrollbar">
        <table className="w-full text-sm">
          <thead>
            <tr>
              <th className="sticky left-0 z-10 bg-surface-900 p-4 text-left font-semibold text-slate-300 border-b border-white/10">
                Tipo de Dado
              </th>
              {sortedYears.map((year) => (
                <th key={year} className="border-b border-white/10 p-4 text-center font-semibold text-slate-300 tabular-nums">
                  {year}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {dataTypes.map((dataType, rowIndex) => {
              const Icon = dataType.icon;
              return (
                <tr
                  key={dataType.key}
                  className="group animate-slide-in-left"
                  style={{ animationDelay: `${rowIndex * 60}ms` }}
                >
                  <td className="sticky left-0 z-10 whitespace-nowrap border-b border-white/5 bg-surface-900 p-4 text-slate-100 group-hover:bg-surface-800">
                    <div className="flex items-center gap-3">
                      <Icon className="h-5 w-5 text-indigo-300" aria-hidden="true" />
                      <span>{dataType.label}</span>
                    </div>
                  </td>
                  {sortedYears.map((year) => {
                    const isAvailable = stats[dataType.key].includes(year);
                    return (
                      <td key={year} className="border-b border-white/5 p-4 text-center group-hover:bg-white/[0.03]">
                        <span
                          className={`inline-flex h-7 w-7 items-center justify-center rounded-full ${isAvailable
                            ? 'bg-emerald-500/15 text-emerald-300 ring-1 ring-inset ring-emerald-400/30'
                            : 'bg-white/[0.03] text-slate-600'}`}
                          title={`${dataType.label} ${year}: ${isAvailable ? 'disponível' : 'não disponível'}`}
                        >
                          {isAvailable
                            ? <CheckIcon className="h-4 w-4" aria-label="Disponível" />
                            : <MinusIcon className="h-4 w-4" aria-label="Não disponível" />}
                        </span>
                      </td>
                    );
                  })}
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default DataAvailabilityTable;
