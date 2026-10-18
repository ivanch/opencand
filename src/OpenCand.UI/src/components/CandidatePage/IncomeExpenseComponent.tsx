import React, { useEffect, useState } from 'react';
import { BanknotesIcon } from '@heroicons/react/24/outline';
import { openCandApi, isRequestCancelled, type YearSummary } from '../../api';
import FinanceSection from './IncomeExpense/FinanceSection';
import { EXPENSE_KIND, INCOME_KIND } from './IncomeExpense/financeKinds';

interface IncomeExpenseComponentProps {
  idCandidato: string;
}

interface Summaries {
  income: YearSummary[];
  expenses: YearSummary[];
}

/** Loads the per-year totals of receitas/despesas; the rows are loaded by each section when a year is opened. */
const IncomeExpenseComponent: React.FC<IncomeExpenseComponentProps> = ({ idCandidato }) => {
  const [summaries, setSummaries] = useState<Summaries | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    const { signal } = controller;
    setSummaries(null);

    const fetchSummary = async (request: () => Promise<{ resumo: YearSummary[] }>): Promise<YearSummary[]> => {
      try {
        return (await request()).resumo ?? [];
      } catch (err) {
        if (!isRequestCancelled(err)) console.error('Error fetching income/expense summary:', err);
        return [];
      }
    };

    Promise.all([
      fetchSummary(() => openCandApi.getCandidateReceitasResumo(idCandidato, signal)),
      fetchSummary(() => openCandApi.getCandidateDespesasResumo(idCandidato, signal)),
    ]).then(([income, expenses]) => {
      if (!signal.aborted) setSummaries({ income, expenses });
    });

    return () => controller.abort();
  }, [idCandidato]);

  const showIncome = (summaries?.income.length ?? 0) > 0;
  const showExpenses = (summaries?.expenses.length ?? 0) > 0;
  if (!summaries || (!showIncome && !showExpenses)) return null;

  return (
    <div className="bg-white/95 backdrop-blur-sm rounded-xl shadow-lg hover:shadow-xl transform hover:scale-[1.01] transition-all duration-200 p-6">
      <div className="flex items-center mb-6">
        <BanknotesIcon className="h-8 w-8 text-green-600 mr-3" />
        <h2 className="text-2xl font-bold text-gray-900">Receitas e Despesas</h2>
      </div>

      {showIncome && (
        <div className="mb-8">
          <FinanceSection idCandidato={idCandidato} kind={INCOME_KIND} summary={summaries.income} />
        </div>
      )}

      {showIncome && showExpenses && (
        <div className="flex items-center my-8">
          <div className="flex-grow border-t border-gray-300"></div>
          <div className="flex-shrink-0 px-4">
            <div className="w-3 h-3 bg-gray-300 rounded-full"></div>
          </div>
          <div className="flex-grow border-t border-gray-300"></div>
        </div>
      )}

      {showExpenses && (
        <FinanceSection idCandidato={idCandidato} kind={EXPENSE_KIND} summary={summaries.expenses} />
      )}
    </div>
  );
};

export default IncomeExpenseComponent;
