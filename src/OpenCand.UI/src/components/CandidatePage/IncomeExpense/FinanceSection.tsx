import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
  BuildingOfficeIcon,
  CalendarIcon,
  ChevronDownIcon,
  UserIcon,
} from '@heroicons/react/24/outline';
import { isRequestCancelled, type YearSummary } from '../../../api';
import { formatCurrency, formatDateToDDMMYYYY } from '../../../utils/utils';
import Spinner from '../../../shared/Spinner';
import type { FinanceAccent, FinanceKind, FinanceRow } from './financeKinds';

const PAGE_SIZE = 50;

// Tailwind only generates classes it can find as whole strings, so they are listed per accent.
const ACCENT_CLASSES: Record<FinanceAccent, { text: string; spinner: string; summary: string; border: string; button: string; icon: string }> = {
  red: {
    text: 'text-rose-600',
    spinner: 'border-rose-600',
    summary: 'bg-rose-50/70 ring-rose-100',
    border: 'border-rose-400',
    button: 'bg-rose-600 hover:bg-rose-500',
    icon: 'bg-rose-50 text-rose-600',
  },
  green: {
    text: 'text-emerald-600',
    spinner: 'border-emerald-600',
    summary: 'bg-emerald-50/70 ring-emerald-100',
    border: 'border-emerald-400',
    button: 'bg-emerald-600 hover:bg-emerald-500',
    icon: 'bg-emerald-50 text-emerald-600',
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

  const th = 'px-4 py-2.5 text-left text-xs font-semibold uppercase tracking-wider text-slate-500 whitespace-nowrap';

  return (
    <div>
      <div className="mb-4 flex items-center gap-2.5">
        <span className={`flex h-8 w-8 items-center justify-center rounded-lg ${accent.icon}`}>
          <Icon className="h-4 w-4" aria-hidden="true" />
        </span>
        <h3 className="text-base font-semibold text-slate-900">{kind.title}</h3>
      </div>

      {sortedSummary.length === 0 ? (
        <div className="flex flex-col items-center py-6 text-center text-slate-500">
          <Icon className="mb-2 h-10 w-10 text-slate-300" />
          <span>{kind.emptyText}</span>
        </div>
      ) : (
        <>
          <div className={`mb-4 flex items-center justify-between rounded-xl p-4 ring-1 ring-inset ${accent.summary}`}>
            <span className="text-sm font-medium text-slate-700">{kind.totalLabel}</span>
            <span className={`text-xl font-bold tabular-nums ${accent.text}`}>{formatCurrency(grandTotal)}</span>
          </div>

          <div className="space-y-2.5">
            {sortedSummary.map(({ ano, quantidade, total }) => {
              const isExpanded = expandedYears.has(ano);
              const state = years[ano] ?? EMPTY_YEAR;
              const remaining = Math.max(quantidade - state.rows.length, 0);

              return (
                <div key={ano} className="overflow-hidden rounded-xl ring-1 ring-inset ring-slate-200">
                  <button
                    type="button"
                    onClick={() => toggleYear(ano)}
                    className={`flex w-full items-center justify-between gap-3 px-4 py-3.5 text-left transition-colors ${isExpanded ? 'bg-slate-50' : 'hover:bg-slate-50'}`}
                    aria-expanded={isExpanded}
                  >
                    <div className="flex items-center gap-3">
                      <CalendarIcon className="h-5 w-5 text-slate-400" aria-hidden="true" />
                      <span className="font-semibold text-slate-900 tabular-nums">{ano}</span>
                      <span className="rounded-full bg-slate-100 px-2 py-0.5 text-xs text-slate-500">
                        {quantidade} {quantidade === 1 ? 'item' : 'itens'}
                      </span>
                    </div>
                    <div className="flex items-center gap-3">
                      <span className={`font-semibold tabular-nums ${accent.text}`}>{formatCurrency(total)}</span>
                      <ChevronDownIcon className={`h-5 w-5 text-slate-400 transition-transform duration-200 ${isExpanded ? 'rotate-180' : ''}`} aria-hidden="true" />
                    </div>
                  </button>

                  {isExpanded && (
                    <div className="border-t border-slate-100 animate-fadeIn">
                      {state.rows.length > 0 && (
                        <div className="overflow-x-auto custom-scrollbar">
                          <table className="w-full text-sm">
                            <thead className="bg-slate-50/60">
                              <tr>
                                <th className={th}>Data</th>
                                <th className={th}>{kind.counterpartHeader}</th>
                                <th className={th}>Descrição</th>
                                <th className={`${th} text-right`}>Valor</th>
                              </tr>
                            </thead>
                            <tbody className="divide-y divide-slate-100">
                              {state.rows.map(row => {
                                const isRowExpanded = expandedRows.has(row.id);
                                return (
                                  <React.Fragment key={row.id}>
                                    <tr
                                      className={`cursor-pointer transition-colors ${isRowExpanded ? 'bg-slate-50' : 'hover:bg-slate-50'}`}
                                      onClick={() => toggleRow(row.id)}
                                      onKeyDown={(e) => {
                                        if (e.key === 'Enter' || e.key === ' ') {
                                          e.preventDefault();
                                          toggleRow(row.id);
                                        }
                                      }}
                                      tabIndex={0}
                                      aria-expanded={isRowExpanded}
                                    >
                                      <td className="whitespace-nowrap px-4 py-3 text-slate-600 tabular-nums">{formatDateToDDMMYYYY(row.data)}</td>
                                      <td className="px-4 py-3">
                                        <div className="flex items-center gap-2">
                                          {row.isPerson ? (
                                            <UserIcon className="h-4 w-4 shrink-0 text-slate-400" aria-label="Pessoa física" />
                                          ) : (
                                            <BuildingOfficeIcon className="h-4 w-4 shrink-0 text-slate-400" aria-label="Pessoa jurídica" />
                                          )}
                                          <span className="font-medium text-slate-900">{row.counterpart}</span>
                                        </div>
                                      </td>
                                      <td className="max-w-xs truncate px-4 py-3 text-slate-500">
                                        {row.descricao || 'Descrição não informada'}
                                      </td>
                                      <td className="whitespace-nowrap px-4 py-3 text-right font-semibold">
                                        <div className="flex items-center justify-end gap-2">
                                          <span className={`tabular-nums ${accent.text}`}>{formatCurrency(row.valor)}</span>
                                          <ChevronDownIcon className={`h-4 w-4 text-slate-400 transition-transform duration-200 ${isRowExpanded ? 'rotate-180' : ''}`} aria-hidden="true" />
                                        </div>
                                      </td>
                                    </tr>
                                    {isRowExpanded && (
                                      <tr>
                                        <td colSpan={4} className={`border-l-4 bg-slate-50 px-4 py-4 ${accent.border}`}>
                                          <h4 className="mb-3 text-sm font-semibold text-slate-900">{kind.detailsTitle}</h4>
                                          <dl className="grid grid-cols-1 gap-x-6 gap-y-2 text-sm md:grid-cols-2">
                                            {row.details.map(detail => (
                                              <div key={detail.label} className="flex gap-2">
                                                <dt className="shrink-0 font-medium text-slate-500">{detail.label}:</dt>
                                                <dd className="break-words text-slate-900">{detail.value}</dd>
                                              </div>
                                            ))}
                                          </dl>
                                          {row.descricao && (
                                            <div className="mt-3 border-t border-slate-200 pt-3 text-sm">
                                              <span className="font-medium text-slate-500">Descrição completa</span>
                                              <p className="mt-1 text-slate-900">{row.descricao}</p>
                                            </div>
                                          )}
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
                        <div className="flex h-16 items-center justify-center">
                          <Spinner colorClassName={accent.spinner} />
                        </div>
                      )}

                      {state.error && !state.loading && (
                        <div className="p-4 text-center text-sm text-slate-600">
                          Não foi possível carregar os itens.{' '}
                          <button
                            type="button"
                            onClick={() => void loadPage(ano, Math.max(state.page, 0) + 1)}
                            className="font-medium text-slate-900 underline"
                          >
                            Tentar novamente
                          </button>
                        </div>
                      )}

                      {!state.loading && !state.error && remaining > 0 && state.page > 0 && (
                        <div className="border-t border-slate-100 bg-slate-50/60 p-4 text-center">
                          <button
                            type="button"
                            onClick={() => void loadPage(ano, state.page + 1)}
                            className={`rounded-full px-5 py-2 text-sm font-medium text-white shadow-sm transition-colors ${accent.button}`}
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
