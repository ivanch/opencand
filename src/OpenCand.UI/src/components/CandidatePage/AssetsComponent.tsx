import React, { useState, useMemo } from 'react';
import { ArrowDownRightIcon, ArrowUpRightIcon, ChevronDownIcon, CurrencyDollarIcon } from '@heroicons/react/24/outline';
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { type Asset } from '../../api';
import Panel from '../../shared/Panel';
import { LoadingBlock } from '../../shared/Spinner';
import { formatCurrency, formatCurrencyCompact } from '../../utils/utils';

interface AssetsComponentProps {
  assets: Asset[] | null;
  isLoading: boolean;
}

const sumValues = (items: Asset[]) => items.reduce((total, asset) => total + asset.valor, 0);

const AssetsComponent: React.FC<AssetsComponentProps> = ({ assets, isLoading }) => {
  const [expandedYears, setExpandedYears] = useState<Set<number>>(new Set());

  const groupedAssets = useMemo(() => {
    if (!assets) return {};

    return assets.reduce((acc, asset) => {
      const year = asset.ano;
      if (!acc[year]) {
        acc[year] = [];
      }
      acc[year].push(asset);
      return acc;
    }, {} as Record<number, Asset[]>);
  }, [assets]);

  const sortedYears = useMemo(() => {
    return Object.keys(groupedAssets).map(Number).sort((a, b) => b - a);
  }, [groupedAssets]);

  const chartData = useMemo(
    () => [...sortedYears].reverse().map(year => ({ year, valor: sumValues(groupedAssets[year]) })),
    [sortedYears, groupedAssets]
  );

  const toggleYear = (year: number) => {
    setExpandedYears(prev => {
      const newSet = new Set(prev);
      if (newSet.has(year)) {
        newSet.delete(year);
      } else {
        newSet.add(year);
      }
      return newSet;
    });
  };

  // Latest declared total and how it changed since the first declaration
  const latestTotal = chartData.length > 0 ? chartData[chartData.length - 1].valor : 0;
  const firstTotal = chartData.length > 0 ? chartData[0].valor : 0;
  const variation = latestTotal - firstTotal;
  const variationPercent = firstTotal > 0 ? (variation / firstTotal) * 100 : null;
  const variationClass = variation > 0 ? 'text-emerald-600' : variation < 0 ? 'text-rose-600' : 'text-slate-500';
  const VariationIcon = variation >= 0 ? ArrowUpRightIcon : ArrowDownRightIcon;

  return (
    <Panel title="Patrimônio Declarado" icon={CurrencyDollarIcon} iconClassName="bg-violet-50 text-violet-600">
      {isLoading ? (
        <LoadingBlock colorClassName="border-violet-600" />
      ) : assets && assets.length > 0 ? (
        <>
          {/* Summary */}
          <div className="mb-5 grid gap-3 sm:grid-cols-2">
            <div className="rounded-xl bg-violet-50/70 p-4 ring-1 ring-inset ring-violet-100">
              <p className="text-xs font-medium uppercase tracking-wider text-violet-700/80">
                Última declaração ({sortedYears[0]})
              </p>
              <p className="mt-1 text-2xl font-bold text-slate-900 tabular-nums">{formatCurrency(latestTotal)}</p>
            </div>
            {sortedYears.length > 1 && (
              <div className="rounded-xl bg-slate-50 p-4 ring-1 ring-inset ring-slate-100">
                <p className="text-xs font-medium uppercase tracking-wider text-slate-500">
                  Variação desde {sortedYears[sortedYears.length - 1]}
                </p>
                <p className={`mt-1 flex items-center gap-1.5 text-2xl font-bold tabular-nums ${variationClass}`}>
                  {variation !== 0 && <VariationIcon className="h-5 w-5" aria-hidden="true" />}
                  {variation > 0 ? '+' : ''}{formatCurrency(variation)}
                  {variationPercent !== null && variation !== 0 && (
                    <span className="text-sm font-semibold">
                      ({variation > 0 ? '+' : ''}{variationPercent.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%)
                    </span>
                  )}
                </p>
              </div>
            )}
          </div>

          {/* Assets Progression Graph */}
          {sortedYears.length > 1 && (
            <div className="mb-6 rounded-xl ring-1 ring-inset ring-slate-100 p-3 pt-5">
              <h3 className="mb-2 px-2 text-sm font-semibold text-slate-700">Evolução Patrimonial Declarada</h3>
              <ResponsiveContainer width="100%" height={260}>
                <AreaChart data={chartData} margin={{ top: 10, right: 16, left: 0, bottom: 0 }}>
                  <defs>
                    <linearGradient id="assetsFill" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor="#7c3aed" stopOpacity={0.25} />
                      <stop offset="100%" stopColor="#7c3aed" stopOpacity={0} />
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke="#eef2f7" vertical={false} />
                  <XAxis dataKey="year" stroke="#94a3b8" fontSize={12} tickLine={false} axisLine={false} />
                  <YAxis
                    stroke="#94a3b8"
                    fontSize={12}
                    tickLine={false}
                    axisLine={false}
                    tickFormatter={(value) => formatCurrencyCompact(value)}
                    width={80}
                  />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: '#ffffff',
                      color: '#334155',
                      border: '1px solid #e2e8f0',
                      borderRadius: '12px',
                      boxShadow: '0 10px 25px -10px rgba(15, 23, 42, 0.25)',
                    }}
                    formatter={(value: number) => [formatCurrency(value), 'Patrimônio']}
                    labelStyle={{ color: '#0f172a', fontWeight: 600 }}
                  />
                  <Area
                    type="monotone"
                    dataKey="valor"
                    stroke="#7c3aed"
                    strokeWidth={2.5}
                    fill="url(#assetsFill)"
                    dot={{ fill: '#ffffff', stroke: '#7c3aed', strokeWidth: 2, r: 4 }}
                    activeDot={{ r: 6, fill: '#7c3aed', stroke: '#ffffff', strokeWidth: 2 }}
                  />
                </AreaChart>
              </ResponsiveContainer>
            </div>
          )}

          {/* Declarations per year */}
          <div className="space-y-2.5">
            {sortedYears.map((year) => {
              const yearAssets = groupedAssets[year];
              const isExpanded = expandedYears.has(year);

              return (
                <div key={year} className="overflow-hidden rounded-xl ring-1 ring-inset ring-slate-200">
                  <button
                    type="button"
                    onClick={() => toggleYear(year)}
                    aria-expanded={isExpanded}
                    className={`flex w-full items-center justify-between gap-3 px-4 py-3.5 text-left transition-colors ${isExpanded ? 'bg-slate-50' : 'hover:bg-slate-50'}`}
                  >
                    <div className="flex items-center gap-3">
                      <span className="text-base font-semibold text-slate-900 tabular-nums">{year}</span>
                      <span className="rounded-full bg-slate-100 px-2 py-0.5 text-xs text-slate-500">
                        {yearAssets.length} {yearAssets.length === 1 ? 'item' : 'itens'}
                      </span>
                    </div>
                    <div className="flex items-center gap-3">
                      <span className="font-semibold text-violet-700 tabular-nums">
                        {formatCurrency(sumValues(yearAssets))}
                      </span>
                      <ChevronDownIcon className={`h-5 w-5 text-slate-400 transition-transform duration-200 ${isExpanded ? 'rotate-180' : ''}`} aria-hidden="true" />
                    </div>
                  </button>

                  {isExpanded && (
                    <div className="overflow-x-auto border-t border-slate-100 animate-fadeIn custom-scrollbar">
                      <table className="w-full text-sm">
                        <thead className="bg-slate-50/60">
                          <tr>
                            <th className="px-4 py-2.5 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">Tipo</th>
                            <th className="px-4 py-2.5 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">Descrição</th>
                            <th className="px-4 py-2.5 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">Valor</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-100">
                          {yearAssets.map((asset: Asset, index: number) => (
                            <tr
                              key={`${asset.idCandidato}-${asset.ano}-${index}`}
                              className="transition-colors hover:bg-slate-50"
                            >
                              <td className="px-4 py-3 font-medium text-slate-900">{asset.tipoBem}</td>
                              <td className="px-4 py-3 text-slate-600">{asset.descricao}</td>
                              <td className="whitespace-nowrap px-4 py-3 text-right font-medium text-slate-900 tabular-nums">
                                {formatCurrency(asset.valor)}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </>
      ) : (
        <p className="py-8 text-center text-slate-500">Nenhum patrimônio declarado encontrado</p>
      )}
    </Panel>
  );
};

export default AssetsComponent;
