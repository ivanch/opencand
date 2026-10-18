import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
  BuildingOfficeIcon,
  CalendarIcon,
  ChevronDownIcon,
  ChevronUpIcon,
  UserIcon,
} from '@heroicons/react/24/outline';
import { isRequestCancelled, type YearSummary } from '../../../api';
import { formatDateToDDMMYYYY } from '../../../utils/utils';
import type { FinanceAccent, FinanceKind, FinanceRow } from './financeKinds';

const PAGE_SIZE = 50;

// Tailwind only generates classes it can find as whole strings, so they are listed per accent.
const ACCENT_CLASSES: Record<FinanceAccent, { text: string; spinner: string; summary: string; border: string; button: string; icon: string }> = {
  red: {
    text: 'text-red-600',
    spinner: 'border-red-600',
    summary: 'bg-gradient-to-r from-red-50 to-orange-50 border-red-100',
    border: 'border-red-500',
    button: 'bg-red-600 hover:bg-red-700',
    icon: 'text-red-600',
  },
  green: {
    text: 'text-green-600',
    spinner: 'border-green-600',
    summary: 'bg-gradient-to-r from-green-50 to-blue-50 border-green-100',
    border: 'border-green-500',
    button: 'bg-green-600 hover:bg-green-700',
    icon: 'text-green-600',
  },
};

interface YearState {
  rows: FinanceRow[];
  total: number;
  page: number;
  loading: boolean;
  error: boolean;
}

const EMPTY_YEAR: YearState = { rows: [], total: 0, page: 0, loading: false, error: false };

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);

interface FinanceSectionProps {
  idCandidato: string;
  kind: FinanceKind;
  summary: YearSummary[];
}

/**
 * Despesas / receitas of a candidate. Only the per-year totals are loaded up front; the rows of a year are fetched
 * (50 at a time) when the year is expanded, so candidates with tens of thousands of rows stay fast.
 */
const FinanceSection: React.FC<FinanceSectionProps> = ({ idCandidato, kind, summary }) => {
  const accent = ACCENT_CLASSES[kind.accent];
  const Icon = kind.icon;

  const [expandedYears, setExpandedYears] = useState<Set<number>>(new Set());
  const [expandedRows, setExpandedRows] = useState<Set<string>>(new Set());
  const [years, setYears] = useState<Record<number, YearState>>({});
  const inFlight = useRef(new Set<AbortController>());

  // abort whatever is still loading when the section goes away (or the candidate changes: the parent re-keys it)
  useEffect(() => {
    const controllers = inFlight.current;
    return () => controllers.forEach(controller => controller.abort());
  }, []);

  const sortedSummary = useMemo(() => [...summary].sort((a, b) => b.ano - a.ano), [summary]);
  const grandTotal = useMemo(() => summary.reduce((total, item) => total + item.total, 0), [summary]);

  const loadPage = useCallback(async (ano: number, page: number) => {
    const controller = new AbortController();
    inFlight.current.add(controller);
    setYears(prev => ({ ...prev, [ano]: { ...(prev[ano] ?? EMPTY_YEAR), loading: true, error: false } }));

    try {
      const result = await kind.fetchPage(idCandidato, ano, page, PAGE_SIZE, controller.signal);
      setYears(prev => {
        const current = prev[ano] ?? EMPTY_YEAR;
        return {
          ...prev,
          [ano]: {
            rows: page === 1 ? result.rows : [...current.rows, ...result.rows],
            total: result.total,
            page,
            loading: false,
            error: false,
          },
        };
      });
    } catch (err) {
      if (isRequestCancelled(err)) return;
      console.error(`Error loading ${kind.title} of ${ano}:`, err);
      setYears(prev => ({ ...prev, [ano]: { ...(prev[ano] ?? EMPTY_YEAR), loading: false, error: true } }));
    } finally {
      inFlight.current.delete(controller);
    }
  }, [idCandidato, kind]);

  const toggleYear = (ano: number) => {
    const willExpand = !expandedYears.has(ano);
    setExpandedYears(prev => {
      const next = new Set(prev);
      if (willExpand) next.add(ano);
      else next.delete(ano);
      return next;
    });

    const state = years[ano];
    if (willExpand && (!state || (state.page === 0 && !state.loading))) {
      void loadPage(ano, 1);
    }
  };

  const toggleRow = (rowId: string) => {
    setExpandedRows(prev => {
      const next = new Set(prev);
      if (next.has(rowId)) next.delete(rowId);
      else next.add(rowId);
      return next;
    });
  };

  return (
    <div>
      <div className="flex items-center mb-4">
        <Icon className={`h-6 w-6 ${accent.icon} mr-2`} />
        <h3 className="text-xl font-bold text-gray-900">{kind.title}</h3>
      </div>

      {sortedSummary.length === 0 ? (
        <div className="text-center text-gray-500 py-6">
          <div className="flex flex-col items-center">
            <Icon className="h-10 w-10 text-gray-300 mb-2" />
            <span>{kind.emptyText}</span>
          </div>
        </div>
      ) : (
        <>
          <div className={`${accent.summary} p-4 rounded-lg mb-4 border`}>
            <div className="flex items-center justify-between">
              <span className="text-lg font-semibold text-gray-900">{kind.totalLabel}</span>
              <span className={`text-xl font-bold ${accent.text}`}>{formatCurrency(grandTotal)}</span>
            </div>
          </div>

          <div className="space-y-3">
            {sortedSummary.map(({ ano, quantidade, total }) => {
              const isExpanded = expandedYears.has(ano);
              const state = years[ano] ?? EMPTY_YEAR;
              const remaining = Math.max(quantidade - state.rows.length, 0);

              return (
                <div key={ano} className="border border-gray-200 rounded-lg overflow-hidden">
                  <button
                    onClick={() => toggleYear(ano)}
                    className="w-full px-4 py-3 bg-gray-50 hover:bg-gray-100 flex items-center justify-between transition-colors"
                    aria-expanded={isExpanded}
                  >
                    <div className="flex items-center">
                      <CalendarIcon className="h-5 w-5 text-gray-600 mr-2" />
                      <span className="font-semibold text-gray-900">{ano}</span>
                      <span className="ml-2 text-sm text-gray-600">
                        ({quantidade} {quantidade === 1 ? 'item' : 'itens'})
                      </span>
                    </div>
                    <div className="flex items-center">
                      <span className={`font-bold ${accent.text} mr-2`}>{formatCurrency(total)}</span>
                      {isExpanded ? (
                        <ChevronUpIcon className="h-5 w-5 text-gray-600" />
                      ) : (
                        <ChevronDownIcon className="h-5 w-5 text-gray-600" />
                      )}
                    </div>
                  </button>

                  {isExpanded && (
                    <div className="bg-white">
                      {state.rows.length > 0 && (
                        <div className="overflow-x-auto">
                          <table className="w-full text-sm">
                            <thead className="bg-gray-50 border-b">
                              <tr>
                                <th className="px-4 py-3 text-left font-medium text-gray-900">Data</th>
                                <th className="px-4 py-3 text-left font-medium text-gray-900">{kind.counterpartHeader}</th>
                                <th className="px-4 py-3 text-left font-medium text-gray-900">Descrição</th>
                                <th className="px-4 py-3 text-right font-medium text-gray-900">Valor</th>
                              </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-200">
                              {state.rows.map(row => {
                                const isRowExpanded = expandedRows.has(row.id);
                                return (
                                  <React.Fragment key={row.id}>
                                    <tr
                                      className="hover:bg-gray-50 cursor-pointer transition-colors"
                                      onClick={() => toggleRow(row.id)}
                                    >
                                      <td className="px-4 py-3 text-gray-900">{formatDateToDDMMYYYY(row.data)}</td>
                                      <td className="px-4 py-3">
                                        <div className="flex items-center">
                                          {row.isPerson ? (
                                            <UserIcon className="h-4 w-4 text-gray-400 mr-2" />
                                          ) : (
                                            <BuildingOfficeIcon className="h-4 w-4 text-gray-400 mr-2" />
                                          )}
                                          <span className="text-gray-900 font-medium">{row.counterpart}</span>
                                        </div>
                                      </td>
                                      <td className="px-4 py-3 text-gray-600 max-w-xs truncate">
                                        {row.descricao || 'Descrição não informada'}
                                      </td>
                                      <td className="px-4 py-3 text-right font-semibold">
                                        <div className="flex items-center justify-end">
                                          <span className={`${accent.text} mr-2`}>{formatCurrency(row.valor)}</span>
                                          {isRowExpanded ? (
                                            <ChevronUpIcon className="h-4 w-4 text-gray-400" />
                                          ) : (
                                            <ChevronDownIcon className="h-4 w-4 text-gray-400" />
                                          )}
                                        </div>
                                      </td>
                                    </tr>
                                    {isRowExpanded && (
                                      <tr>
                                        <td colSpan={4} className={`px-4 py-3 bg-gray-50 border-l-4 ${accent.border}`}>
                                          <div className="space-y-3">
                                            <h4 className="font-semibold text-gray-900 mb-3">{kind.detailsTitle}</h4>
                                            <div className="grid grid-cols-1 md:grid-cols-2 gap-3 text-sm">
                                              {row.details.map(detail => (
                                                <div key={detail.label}>
                                                  <span className="text-gray-600 font-medium">{detail.label}:</span>
                                                  <span className="ml-2 text-gray-900">{detail.value}</span>
                                                </div>
                                              ))}
                                            </div>
                                            {row.descricao && (
                                              <div className="mt-3 pt-3 border-t border-gray-200">
                                                <span className="text-gray-600 font-medium">Descrição Completa:</span>
                                                <p className="ml-2 text-gray-900 mt-1">{row.descricao}</p>
                                              </div>
                                            )}
                                          </div>
                                        </td>
                                      </tr>
                                    )}
                                  </React.Fragment>
                                );
                              })}
                            </tbody>
                          </table>
                        </div>
                      )}

                      {state.loading && (
                        <div className="flex items-center justify-center h-16">
                          <div className={`animate-spin rounded-full h-6 w-6 border-4 ${accent.spinner} border-t-transparent`}></div>
                        </div>
                      )}

                      {state.error && !state.loading && (
                        <div className="p-4 text-center text-sm text-gray-600">
                          Não foi possível carregar os itens.{' '}
                          <button
                            onClick={() => void loadPage(ano, Math.max(state.page, 0) + 1)}
                            className="underline text-gray-900"
                          >
                            Tentar novamente
                          </button>
                        </div>
                      )}

                      {!state.loading && !state.error && remaining > 0 && state.page > 0 && (
                        <div className="p-4 border-t border-gray-200 bg-gray-50 text-center">
                          <button
                            onClick={() => void loadPage(ano, state.page + 1)}
                            className={`px-4 py-2 ${accent.button} text-white rounded-lg transition-colors font-medium`}
                          >
                            Carregar mais {Math.min(PAGE_SIZE, remaining)} itens ({remaining} restantes)
                          </button>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </>
      )}
    </div>
  );
};

export default FinanceSection;
