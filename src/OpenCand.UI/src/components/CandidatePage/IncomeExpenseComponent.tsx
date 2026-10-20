import React, { useEffect, useState } from 'react';
import { BanknotesIcon } from '@heroicons/react/24/outline';
import { openCandApi, isRequestCancelled, type YearSummary } from '../../api';
import FinanceSection from './IncomeExpense/FinanceSection';
import { EXPENSE_KIND, INCOME_KIND } from './IncomeExpense/financeKinds';
import Panel from '../../shared/Panel';

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
    <Panel
      title="Receitas e Despesas"
      subtitle="Prestação de contas de campanha"
      icon={BanknotesIcon}
      iconClassName="bg-teal-50 text-teal-600"
    >
      {showIncome && (
        <FinanceSection idCandidato={idCandidato} kind={INCOME_KIND} summary={summaries.income} />
      )}

      {showIncome && showExpenses && <div className="my-8 border-t border-slate-100" />}

      {showExpenses && (
        <FinanceSection idCandidato={idCandidato} kind={EXPENSE_KIND} summary={summaries.expenses} />
      )}
    </Panel>
  );
};

export default IncomeExpenseComponent;
