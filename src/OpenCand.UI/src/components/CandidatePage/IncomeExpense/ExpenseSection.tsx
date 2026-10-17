import React, { useState, useMemo } from 'react';
import { 
  ChevronDownIcon, 
  ChevronUpIcon,
  ArrowTrendingDownIcon,
  CalendarIcon,
  UserIcon,
  BuildingOfficeIcon
} from '@heroicons/react/24/outline';
import { type CandidateExpenses, type Expense } from '../../../api';

interface ExpenseSectionProps {
  expenses: CandidateExpenses | null;
  isLoadingExpenses: boolean;
}

const ExpenseSection: React.FC<ExpenseSectionProps> = ({ 
  expenses, 
  isLoadingExpenses 
}) => {
  const [expandedYears, setExpandedYears] = useState<Set<number>>(new Set());
  const [visibleExpenseItems, setVisibleExpenseItems] = useState<Record<number, number>>({});
  const [expandedExpenseItems, setExpandedExpenseItems] = useState<Set<string>>(new Set());

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(value);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('pt-BR');
  };

  const groupExpensesByYear = useMemo(() => {
    const despesas = expenses?.despesas || [];
    if (!despesas.length) return {};
    
    return despesas.reduce((acc: Record<number, Expense[]>, item: Expense) => {
      const year = item.ano;
      if (!acc[year]) {
        acc[year] = [];
      }
      acc[year].push(item);
      return acc;
    }, {} as Record<number, Expense[]>);
  }, [expenses]);

  // Sort items by value (descending) for each year
  const sortedExpensesByYear = useMemo(() => {
    const sorted: Record<number, Expense[]> = {};
    Object.keys(groupExpensesByYear).forEach(year => {
      const yearNum = Number(year);
      sorted[yearNum] = [...groupExpensesByYear[yearNum]].sort((a, b) => b.valor - a.valor);
    });
    return sorted;
  }, [groupExpensesByYear]);

  const sortedExpenseYears = useMemo(() => {
    return Object.keys(sortedExpensesByYear).map(Number).sort((a, b) => b - a);
  }, [sortedExpensesByYear]);

  const getTotalExpenses = () => {
    const despesas = expenses?.despesas || [];
    return despesas.reduce((total: number, item: Expense) => total + item.valor, 0);
  };

  const getTotalForExpenseYear = (yearExpenses: Expense[]) => {
    return yearExpenses.reduce((total, item) => total + item.valor, 0);
  };

  const getVisibleExpenseItemsForYear = (year: number) => {
    return visibleExpenseItems[year] || 50;
  };

  const loadMoreExpenseItems = (year: number) => {
    setVisibleExpenseItems(prev => ({
      ...prev,
      [year]: (prev[year] || 50) + 50
    }));
  };

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

  const toggleExpenseItem = (itemId: string) => {
    setExpandedExpenseItems(prev => {
      const newSet = new Set(prev);
      if (newSet.has(itemId)) {
        newSet.delete(itemId);
      } else {
        newSet.add(itemId);
      }
      return newSet;
    });
  };

  const despesas = expenses?.despesas || [];
  const isEmpty = despesas.length === 0;

  return (
    <div>
      <div className="flex items-center mb-4">
        <ArrowTrendingDownIcon className="h-6 w-6 text-red-600 mr-2" />
        <h3 className="text-xl font-bold text-gray-900">Despesas</h3>
      </div>

      {isLoadingExpenses ? (
        <div className="flex items-center justify-center h-24">
          <div className="animate-spin rounded-full h-6 w-6 border-4 border-red-600 border-t-transparent"></div>
        </div>
      ) : !isEmpty ? (
        <>
          {/* Summary Card */}
          <div className="bg-gradient-to-r from-red-50 to-orange-50 p-4 rounded-lg mb-4 border border-red-100">
            <div className="flex items-center justify-between">
              <span className="text-lg font-semibold text-gray-900">Total de Despesas</span>
              <span className="text-xl font-bold text-red-600">
                {formatCurrency(getTotalExpenses())}
              </span>
            </div>
          </div>

          <div className="space-y-3">
            {sortedExpenseYears.map((year) => {
              const yearExpenses = sortedExpensesByYear[year];
              const isExpanded = expandedYears.has(year + 10000); // Add offset to avoid conflicts with income years
              const visibleCount = getVisibleExpenseItemsForYear(year);
              const hasMore = yearExpenses.length > visibleCount;
              const visibleItems = yearExpenses.slice(0, visibleCount);
              
              return (
                <div key={`expense-${year}`} className="border border-gray-200 rounded-lg overflow-hidden">
                  <button
                    onClick={() => toggleYear(year + 10000)}
                    className="w-full px-4 py-3 bg-gray-50 hover:bg-gray-100 flex items-center justify-between transition-colors"
                  >
                    <div className="flex items-center">
                      <CalendarIcon className="h-5 w-5 text-gray-600 mr-2" />
                      <span className="font-semibold text-gray-900">{year}</span>
                      <span className="ml-2 text-sm text-gray-600">
                        ({yearExpenses.length} {yearExpenses.length === 1 ? 'item' : 'itens'})
                      </span>
                    </div>
                    <div className="flex items-center">
                      <span className="font-bold text-red-600 mr-2">
                        {formatCurrency(getTotalForExpenseYear(yearExpenses))}
                      </span>
                      {isExpanded ? (
                        <ChevronUpIcon className="h-5 w-5 text-gray-600" />
                      ) : (
                        <ChevronDownIcon className="h-5 w-5 text-gray-600" />
                      )}
                    </div>
                  </button>

                  {isExpanded && (
                    <div className="bg-white">
                      <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                          <thead className="bg-gray-50 border-b">
                            <tr>
                              <th className="px-4 py-3 text-left font-medium text-gray-900">Data</th>
                              <th className="px-4 py-3 text-left font-medium text-gray-900">Fornecedor</th>
                              <th className="px-4 py-3 text-left font-medium text-gray-900">Descrição</th>
                              <th className="px-4 py-3 text-right font-medium text-gray-900">Valor</th>
                            </tr>
                          </thead>
                          <tbody className="divide-y divide-gray-200">
                            {visibleItems.map((item, index) => {
                              const itemId = `expense-${item.ano}-${index}`;
                              const isItemExpanded = expandedExpenseItems.has(itemId);
                              
                              return (
                                <React.Fragment key={itemId}>
                                  <tr 
                                    className="hover:bg-gray-50 cursor-pointer transition-colors"
                                    onClick={() => toggleExpenseItem(itemId)}
                                  >
                                    <td className="px-4 py-3 text-gray-900">
                                      {formatDate(item.dataDespesa)}
                                    </td>
                                    <td className="px-4 py-3">
                                      <div className="flex items-center">
                                        {item.cpfFornecedor ? (
                                          <UserIcon className="h-4 w-4 text-gray-400 mr-2" />
                                        ) : (
                                          <BuildingOfficeIcon className="h-4 w-4 text-gray-400 mr-2" />
                                        )}
                                        <span className="text-gray-900 font-medium">
                                          {item.nomeFornecedor || item.nomeFornecedorRFB || 'Fornecedor não informado'}
                                        </span>
                                      </div>
                                    </td>
                                    <td className="px-4 py-3 text-gray-600 max-w-xs truncate">
                                      {item.descricao || 'Descrição não informada'}
                                    </td>
                                    <td className="px-4 py-3 text-right font-semibold">
                                      <div className="flex items-center justify-end">
                                        <span className="text-red-600 mr-2">
                                          {formatCurrency(item.valor)}
                                        </span>
                                        {isItemExpanded ? (
                                          <ChevronUpIcon className="h-4 w-4 text-gray-400" />
                                        ) : (
                                          <ChevronDownIcon className="h-4 w-4 text-gray-400" />
                                        )}
                                      </div>
                                    </td>
                                  </tr>
                                  {isItemExpanded && (
                                    <tr>
                                      <td colSpan={4} className="px-4 py-3 bg-gray-50 border-l-4 border-red-500">
                                        <div className="space-y-3">
                                          <h4 className="font-semibold text-gray-900 mb-3">Detalhes da Despesa</h4>
                                          <div className="grid grid-cols-1 md:grid-cols-2 gap-3 text-sm">
                                            <div>
                                              <span className="text-gray-600 font-medium">ID:</span>
                                              <span className="ml-2 text-gray-900">{item.idDespesa}</span>
                                            </div>
                                            <div>
                                              <span className="text-gray-600 font-medium">Ano:</span>
                                              <span className="ml-2 text-gray-900">{item.ano}</span>
                                            </div>
                                            <div>
                                              <span className="text-gray-600 font-medium">Turno:</span>
                                              <span className="ml-2 text-gray-900">{item.turno}</span>
                                            </div>
                                            <div>
                                              <span className="text-gray-600 font-medium">Partido:</span>
                                              <span className="ml-2 text-gray-900">{item.sgPartido}</span>
                                            </div>
                                            <div>
                                              <span className="text-gray-600 font-medium">Tipo Fornecedor:</span>
                                              <span className="ml-2 text-gray-900">{item.tipoFornecedor || '-'}</span>
                                            </div>
                                            <div>
                                              <span className="text-gray-600 font-medium">Tipo Documento:</span>
                                              <span className="ml-2 text-gray-900">{item.tipoDocumento || '-'}</span>
                                            </div>
                                            <div>
                                              <span className="text-gray-600 font-medium">Origem:</span>
                                              <span className="ml-2 text-gray-900">{item.origemDespesa || '-'}</span>
                                            </div>
                                            {item.cpfFornecedor && (
                                              <div>
                                                <span className="text-gray-600 font-medium">CPF:</span>
                                                <span className="ml-2 text-gray-900">{item.cpfFornecedor}</span>
                                              </div>
                                            )}
                                            {item.cnpjFornecedor && (
                                              <div>
                                                <span className="text-gray-600 font-medium">CNPJ:</span>
                                                <span className="ml-2 text-gray-900">{item.cnpjFornecedor}</span>
                                              </div>
                                            )}
                                            {item.municipioFornecedor && (
                                              <div>
                                                <span className="text-gray-600 font-medium">Município:</span>
                                                <span className="ml-2 text-gray-900">{item.municipioFornecedor}</span>
                                              </div>
                                            )}
                                          </div>
                                          {item.descricao && (
                                            <div className="mt-3 pt-3 border-t border-gray-200">
                                              <span className="text-gray-600 font-medium">Descrição Completa:</span>
                                              <p className="ml-2 text-gray-900 mt-1">{item.descricao}</p>
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
                      
                      {/* Load More Button */}
                      {hasMore && (
                        <div className="p-4 border-t border-gray-200 bg-gray-50 text-center">
                          <button
                            onClick={() => loadMoreExpenseItems(year)}
                            className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition-colors font-medium"
                          >
                            Carregar mais 50 itens ({yearExpenses.length - visibleCount} restantes)
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
      ) : (
        <div className="text-center text-gray-500 py-6">
          <div className="flex flex-col items-center">
            <ArrowTrendingDownIcon className="h-10 w-10 text-gray-300 mb-2" />
            <span>Nenhuma despesa encontrada</span>
          </div>
        </div>
      )}
    </div>
  );
};

export default ExpenseSection;
