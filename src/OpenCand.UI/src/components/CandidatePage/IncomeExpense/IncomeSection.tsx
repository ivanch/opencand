import React, { useState, useMemo } from 'react';
import { 
  ChevronDownIcon, 
  ChevronUpIcon,
  ArrowTrendingUpIcon,
  CalendarIcon,
  UserIcon,
  BuildingOfficeIcon
} from '@heroicons/react/24/outline';
import { type CandidateIncome, type Income } from '../../../api';

interface IncomeSectionProps {
  income: CandidateIncome | null;
  isLoadingIncome: boolean;
}

const IncomeSection: React.FC<IncomeSectionProps> = ({ 
  income, 
  isLoadingIncome 
}) => {
  const [expandedYears, setExpandedYears] = useState<Set<number>>(new Set());
  const [visibleIncomeItems, setVisibleIncomeItems] = useState<Record<number, number>>({});
  const [expandedIncomeItems, setExpandedIncomeItems] = useState<Set<string>>(new Set());

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(value);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('pt-BR');
  };

  const groupIncomeByYear = useMemo(() => {
    const receitas = income?.receitas || [];
    if (!receitas.length) return {};
    
    return receitas.reduce((acc: Record<number, Income[]>, item: Income) => {
      const year = item.ano;
      if (!acc[year]) {
        acc[year] = [];
      }
      acc[year].push(item);
      return acc;
    }, {} as Record<number, Income[]>);
  }, [income]);

  // Sort items by value (descending) for each year
  const sortedIncomeByYear = useMemo(() => {
    const sorted: Record<number, Income[]> = {};
    Object.keys(groupIncomeByYear).forEach(year => {
      const yearNum = Number(year);
      sorted[yearNum] = [...groupIncomeByYear[yearNum]].sort((a, b) => b.valor - a.valor);
    });
    return sorted;
  }, [groupIncomeByYear]);

  const sortedIncomeYears = useMemo(() => {
    return Object.keys(sortedIncomeByYear).map(Number).sort((a, b) => b - a);
  }, [sortedIncomeByYear]);

  const getTotalIncome = () => {
    const receitas = income?.receitas || [];
    return receitas.reduce((total: number, item: Income) => total + item.valor, 0);
  };

  const getTotalForIncomeYear = (yearIncome: Income[]) => {
    return yearIncome.reduce((total, item) => total + item.valor, 0);
  };

  const getVisibleIncomeItemsForYear = (year: number) => {
    return visibleIncomeItems[year] || 50;
  };

  const loadMoreIncomeItems = (year: number) => {
    setVisibleIncomeItems(prev => ({
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

  const toggleIncomeItem = (itemId: string) => {
    setExpandedIncomeItems(prev => {
      const newSet = new Set(prev);
      if (newSet.has(itemId)) {
        newSet.delete(itemId);
      } else {
        newSet.add(itemId);
      }
      return newSet;
    });
  };

  const receitas = income?.receitas || [];
  const isEmpty = receitas.length === 0;

  return (
    <div className="mb-8">
      <div className="flex items-center mb-4">
        <ArrowTrendingUpIcon className="h-6 w-6 text-green-600 mr-2" />
        <h3 className="text-xl font-bold text-gray-900">Receitas</h3>
      </div>

      {isLoadingIncome ? (
        <div className="flex items-center justify-center h-24">
          <div className="animate-spin rounded-full h-6 w-6 border-4 border-green-600 border-t-transparent"></div>
        </div>
      ) : !isEmpty ? (
        <>
          {/* Summary Card */}
          <div className="bg-gradient-to-r from-green-50 to-blue-50 p-4 rounded-lg mb-4 border border-green-100">
            <div className="flex items-center justify-between">
              <span className="text-lg font-semibold text-gray-900">Total de Receitas</span>
              <span className="text-xl font-bold text-green-600">
                {formatCurrency(getTotalIncome())}
              </span>
            </div>
          </div>

          <div className="space-y-3">
            {sortedIncomeYears.map((year) => {
              const yearIncome = sortedIncomeByYear[year];
              const isExpanded = expandedYears.has(year);
              const visibleCount = getVisibleIncomeItemsForYear(year);
              const hasMore = yearIncome.length > visibleCount;
              const visibleItems = yearIncome.slice(0, visibleCount);
              
              return (
                <div key={`income-${year}`} className="border border-gray-200 rounded-lg overflow-hidden">
                  <button
                    onClick={() => toggleYear(year)}
                    className="w-full px-4 py-3 bg-gray-50 hover:bg-gray-100 flex items-center justify-between transition-colors"
                  >
                    <div className="flex items-center">
                      <CalendarIcon className="h-5 w-5 text-gray-600 mr-2" />
                      <span className="font-semibold text-gray-900">{year}</span>
                      <span className="ml-2 text-sm text-gray-600">
                        ({yearIncome.length} {yearIncome.length === 1 ? 'item' : 'itens'})
                      </span>
                    </div>
                    <div className="flex items-center">
                      <span className="font-bold text-green-600 mr-2">
                        {formatCurrency(getTotalForIncomeYear(yearIncome))}
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
                              <th className="px-4 py-3 text-left font-medium text-gray-900">Doador</th>
                              <th className="px-4 py-3 text-left font-medium text-gray-900">Descrição</th>
                              <th className="px-4 py-3 text-right font-medium text-gray-900">Valor</th>
                            </tr>
                          </thead>
                          <tbody className="divide-y divide-gray-200">
                            {visibleItems.map((item, index) => {
                              const itemId = `income-${item.ano}-${index}`;
                              const isItemExpanded = expandedIncomeItems.has(itemId);
                              
                              return (
                                <React.Fragment key={itemId}>
                                  <tr 
                                    className="hover:bg-gray-50 cursor-pointer transition-colors"
                                    onClick={() => toggleIncomeItem(itemId)}
                                  >
                                    <td className="px-4 py-3 text-gray-900">
                                      {formatDate(item.dataReceita)}
                                    </td>
                                    <td className="px-4 py-3">
                                      <div className="flex items-center">
                                        {item.cpfDoador ? (
                                          <UserIcon className="h-4 w-4 text-gray-400 mr-2" />
                                        ) : (
                                          <BuildingOfficeIcon className="h-4 w-4 text-gray-400 mr-2" />
                                        )}
                                        <span className="text-gray-900 font-medium">
                                          {item.nomeDoador || item.nomeDoadorRFB || 'Doador não informado'}
                                        </span>
                                      </div>
                                    </td>
                                    <td className="px-4 py-3 text-gray-600 max-w-xs truncate">
                                      {item.descricao || 'Descrição não informada'}
                                    </td>
                                    <td className="px-4 py-3 text-right font-semibold">
                                      <div className="flex items-center justify-end">
                                        <span className="text-green-600 mr-2">
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
                                      <td colSpan={4} className="px-4 py-3 bg-gray-50 border-l-4 border-green-500">
                                        <div className="space-y-3">
                                          <h4 className="font-semibold text-gray-900 mb-3">Detalhes da Receita</h4>
                                          <div className="grid grid-cols-1 md:grid-cols-2 gap-3 text-sm">
                                            <div>
                                              <span className="text-gray-600 font-medium">ID:</span>
                                              <span className="ml-2 text-gray-900">{item.idReceita}</span>
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
                                              <span className="text-gray-600 font-medium">Fonte:</span>
                                              <span className="ml-2 text-gray-900">{item.fonteReceita || '-'}</span>
                                            </div>
                                            <div>
                                              <span className="text-gray-600 font-medium">Origem:</span>
                                              <span className="ml-2 text-gray-900">{item.origemReceita || '-'}</span>
                                            </div>
                                            <div>
                                              <span className="text-gray-600 font-medium">Natureza:</span>
                                              <span className="ml-2 text-gray-900">{item.naturezaReceita || '-'}</span>
                                            </div>
                                            <div>
                                              <span className="text-gray-600 font-medium">Espécie:</span>
                                              <span className="ml-2 text-gray-900">{item.especieReceita || '-'}</span>
                                            </div>
                                            {item.cpfDoador && (
                                              <div>
                                                <span className="text-gray-600 font-medium">CPF:</span>
                                                <span className="ml-2 text-gray-900">{item.cpfDoador}</span>
                                              </div>
                                            )}
                                            {item.cnpjDoador && (
                                              <div>
                                                <span className="text-gray-600 font-medium">CNPJ:</span>
                                                <span className="ml-2 text-gray-900">{item.cnpjDoador}</span>
                                              </div>
                                            )}
                                            {item.municipioDoador && (
                                              <div>
                                                <span className="text-gray-600 font-medium">Município:</span>
                                                <span className="ml-2 text-gray-900">{item.municipioDoador}</span>
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
                            onClick={() => loadMoreIncomeItems(year)}
                            className="px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg transition-colors font-medium"
                          >
                            Carregar mais 50 itens ({yearIncome.length - visibleCount} restantes)
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
            <ArrowTrendingUpIcon className="h-10 w-10 text-gray-300 mb-2" />
            <span>Nenhuma receita encontrada</span>
          </div>
        </div>
      )}
    </div>
  );
};

export default IncomeSection;
