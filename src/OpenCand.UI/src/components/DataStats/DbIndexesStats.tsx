import React from 'react';

interface DbIndexesStatsProps {
  amount: number;
  size: number;
}

const formatSize = (sizeInBytes: number): string => {
  const sizeMB = sizeInBytes / 1024 / 1024;
  return sizeMB > 1024
    ? `${(sizeMB / 1024).toFixed(2)} GB`
    : `${sizeMB.toFixed(2)} MB`;
};

const DbIndexesStats: React.FC<DbIndexesStatsProps> = ({ amount, size }) => {
  return (
    <div>
      <h3 className="text-xl font-semibold text-white mb-2">Índices</h3>
      <div className="overflow-x-auto max-h-96 flex justify-center" style={{maxHeight: '28rem'}}>
        <table className="w-auto text-left border-collapse mx-auto">
          <thead>
            <tr className="bg-gray-800/10">
              <th className="p-3 text-white font-semibold border-b border-gray-700/30">Quantidade</th>
              <th className="p-3 text-white font-semibold border-b border-gray-700/30">Tamanho Total</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td className="p-3 text-gray-300">{amount}</td>
              <td className="p-3 text-gray-300">{formatSize(size)}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default DbIndexesStats;