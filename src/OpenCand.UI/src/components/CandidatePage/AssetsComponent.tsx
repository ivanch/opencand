import React, { useState, useMemo } from 'react';
import { CurrencyDollarIcon, ChevronDownIcon, ChevronUpIcon } from '@heroicons/react/24/outline';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { type Asset } from '../../api';

interface AssetsComponentProps {
  assets: Asset[] | null;
  isLoading: boolean;
}

const AssetsComponent: React.FC<AssetsComponentProps> = ({ assets, isLoading }) => {
  const [expandedYears, setExpandedYears] = useState<Set<number>>(new Set());

  const formatCurrencyCompact = (value: number) => {
    if (value >= 1000000) {
      return `R$ ${(value / 1000000).toFixed(1)}M`;
    } else if (value >= 1000) {
      return `R$ ${(value / 1000).toFixed(0)}K`;
    } else {
      return `R$ ${value.toFixed(0)}`;
    }
  };

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(value);
  };

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

  const getTotalForYear = (yearAssets: Asset[]) => {
    return yearAssets.reduce((total, asset) => total + asset.valor, 0);
  };

  const getTotalAssets = () => {
    if (!assets) return 0;
    return assets.reduce((total, asset) => total + asset.valor, 0);
  };

  return (
    <div className="bg-white/95 backdrop-blur-sm rounded-xl shadow-lg hover:shadow-xl transform hover:scale-[1.01] transition-all duration-200 p-6">
      <div className="flex items-center mb-6">
        <CurrencyDollarIcon className="h-8 w-8 text-purple-600 mr-3" />
        <h2 className="text-2xl font-bold text-gray-900">Patrimônio Declarado</h2>
      </div>

      {isLoading ? (
        <div className="flex items-center justify-center h-32">
          <div className="animate-spin rounded-full h-8 w-8 border-4 border-purple-600 border-t-transparent"></div>
        </div>
      ) : assets && assets.length > 0 ? (
        <>
          <div className="space-y-4">
            {sortedYears.map((year) => {
              const yearAssets = groupedAssets[year];
              const isExpanded = expandedYears.has(year);
              
              return (
                <div key={year} className="border border-gray-200 rounded-lg overflow-hidden">
                  {/* Year Header - Clickable */}
                  <button
                    onClick={() => toggleYear(year)}
                    className="w-full px-4 py-4 bg-gray-50 hover:bg-gray-100 transition-colors duration-200 flex items-center justify-between focus:outline-none focus:ring-2 focus:ring-purple-500 focus:ring-inset"
                  >
                    <div className="flex items-center space-x-3">
                      <span className="text-lg font-semibold text-gray-900">{year}</span>
                      <span className="text-sm text-gray-600">
                        ({yearAssets.length} {yearAssets.length === 1 ? 'item' : 'itens'})
                      </span>
                    </div>
                    <div className="flex items-center space-x-3">
                      <span className="text-lg font-bold text-purple-600">
                        {formatCurrency(getTotalForYear(yearAssets))}
                      </span>
                      {isExpanded ? (
                        <ChevronUpIcon className="h-5 w-5 text-gray-500 transition-transform duration-200" />
                      ) : (
                        <ChevronDownIcon className="h-5 w-5 text-gray-500 transition-transform duration-200" />
                      )}
                    </div>
                  </button>
                  
                  {/* Assets Table - Collapsible */}
                  {isExpanded && (
                    <div className="bg-white animate-fadeIn">
                      <div className="overflow-x-auto">
                        <table className="w-full table-auto">
                          <thead>
                            <tr className="border-b border-gray-200 bg-gray-50">
                              <th className="text-left py-3 px-4 font-semibold text-gray-700">Tipo</th>
                              <th className="text-left py-3 px-4 font-semibold text-gray-700">Descrição</th>
                              <th className="text-right py-3 px-4 font-semibold text-gray-700">Valor</th>
                            </tr>
                          </thead>
                          <tbody>
                            {yearAssets.map((asset: Asset, index: number) => (
                              <tr 
                                key={`${asset.idCandidato}-${asset.ano}-${index}`} 
                                className="border-b border-gray-100 hover:bg-gray-50 transition-colors duration-150"
                              >
                                <td className="py-3 px-4 text-gray-900">{asset.tipoBem}</td>
                                <td className="py-3 px-4 text-gray-900">{asset.descricao}</td>
                                <td className="py-3 px-4 text-right text-gray-900 font-medium">
                                  {formatCurrency(asset.valor)}
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </div>
                  )}
                </div>
              );
            })}
          </div>
           {/* Assets Progression Graph */}
          {sortedYears.length > 1 && (
            <div className="mt-6 pt-6 border-t-2 border-gray-200">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Evolução Patrimonial Declarada</h3>
              <div className="bg-gradient-to-br from-purple-50 to-green-50 p-6 rounded-lg shadow-inner">
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart
                    data={sortedYears.map(year => ({
                      year,
                      valor: getTotalForYear(groupedAssets[year])
                    })).sort((a, b) => a.year - b.year)}
                    margin={{
                      top: 20,
                      right: 30,
                      left: 0,
                      bottom: 20,
                    }}
                  >
                    <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                    <XAxis 
                      dataKey="year" 
                      stroke="#6b7280"
                      fontSize={12}
                      fontWeight={500}
                    />
                    <YAxis 
                      stroke="#6b7280"
                      fontSize={12}
                      fontWeight={500}
                      tickFormatter={(value) => formatCurrencyCompact(value)}
                      width={75}
                    />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: '#fefefe',
                        color: '#374151',
                        border: '1px solid #e5e7eb',
                        borderRadius: '8px',
                        boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1)',
                      }}
                      formatter={(value: number) => [formatCurrency(value), 'Patrimônio']}
                      labelStyle={{ color: '#374151', fontWeight: 600 }}
                    />
                    <Line 
                      type="monotone" 
                      dataKey="valor" 
                      stroke="url(#gradient)" 
                      strokeWidth={3}
                      dot={{ fill: '#7c3aed', strokeWidth: 2, r: 6 }}
                      activeDot={{ r: 8, fill: '#059669' }}
                    />
                    <defs>
                      <linearGradient id="gradient" x1="0%" y1="0%" x2="100%" y2="0%">
                        <stop offset="0%" stopColor="#7c3aed" />
                        <stop offset="100%" stopColor="#059669" />
                      </linearGradient>
                    </defs>
                  </LineChart>
                </ResponsiveContainer>
                <div className="mt-3 text-center">
                  <p className="text-sm text-gray-600">
                    Variação total: {' '}
                    <span className={`font-semibold ${
                      sortedYears.length > 1 && 
                      getTotalForYear(groupedAssets[Math.max(...sortedYears)]) > 
                      getTotalForYear(groupedAssets[Math.min(...sortedYears)])
                        ? 'text-green-600' 
                        : 'text-red-600'
                    }`}>
                      {sortedYears.length > 1 && 
                        formatCurrency(
                          getTotalForYear(groupedAssets[Math.max(...sortedYears)]) - 
                          getTotalForYear(groupedAssets[Math.min(...sortedYears)])
                        )
                      }
                    </span>
                  </p>
                </div>
              </div>
            </div>
          )}
        </>
      ) : (
        <div className="text-center text-gray-500 py-8">
          Nenhum patrimônio declarado encontrado
        </div>
      )}
    </div>
  );
};

export default AssetsComponent;
