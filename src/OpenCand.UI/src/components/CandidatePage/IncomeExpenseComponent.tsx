import React from 'react';
import { BanknotesIcon } from '@heroicons/react/24/outline';
import { type CandidateExpenses, type CandidateIncome } from '../../api';
import IncomeSection from './IncomeExpense/IncomeSection';
import ExpenseSection from './IncomeExpense/ExpenseSection';

interface IncomeExpenseComponentProps {
  expenses: CandidateExpenses | null;
  income: CandidateIncome | null;
  isLoadingExpenses: boolean;
  isLoadingIncome: boolean;
}

const hasIncomeData = (income: CandidateIncome | null) => {
  if (!income || income.receitas.length === 0) return false;
  return true;
};

const hasExpenseData = (expenses: CandidateExpenses | null) => {
  if (!expenses || expenses.despesas.length === 0) return false;
  return true;
};

const IncomeExpenseComponent: React.FC<IncomeExpenseComponentProps> = ({ 
  expenses, 
  income, 
  isLoadingExpenses, 
  isLoadingIncome 
}) => {
  const showIncome = hasIncomeData(income);
  const showExpenses = hasExpenseData(expenses);
  if (!showIncome && !showExpenses) return null;
  return (
    <div className="bg-white/95 backdrop-blur-sm rounded-xl shadow-lg hover:shadow-xl transform hover:scale-[1.01] transition-all duration-200 p-6">
      <div className="flex items-center mb-6">
        <BanknotesIcon className="h-8 w-8 text-green-600 mr-3" />
        <h2 className="text-2xl font-bold text-gray-900">Receitas e Despesas</h2>
      </div>

      {/* Income Section */}
      {showIncome && (
        <IncomeSection 
          income={income}
          isLoadingIncome={isLoadingIncome}
        />
      )}

      {/* Separator */}
      {showIncome && showExpenses && (
        <div className="flex items-center my-8">
          <div className="flex-grow border-t border-gray-300"></div>
          <div className="flex-shrink-0 px-4">
            <div className="w-3 h-3 bg-gray-300 rounded-full"></div>
          </div>
          <div className="flex-grow border-t border-gray-300"></div>
        </div>
      )}

      {/* Expenses Section */}
      {showExpenses && (
        <ExpenseSection 
          expenses={expenses}
          isLoadingExpenses={isLoadingExpenses}
        />
      )}
    </div>
  );
};

export default IncomeExpenseComponent;
