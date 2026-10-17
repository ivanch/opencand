import React from 'react';

interface DbStatItem {
  name: string;
  totalSize: number;
  entries: number;
}

interface DbStatsTableProps {
  title: string;
  items: DbStatItem[];
  showTotal?: boolean;
}

const formatSize = (sizeInBytes: number): string => {
  const sizeMB = sizeInBytes / 1024 / 1024;
  return sizeMB > 1024
    ? `${(sizeMB / 1024).toFixed(2)} GB`
    : `${sizeMB.toFixed(2)} MB`;
};

const DbStatsTable: React.FC<DbStatsTableProps> = ({ title, items, showTotal = false }) => {
  const totalSize = showTotal ? items.reduce((acc, item) => acc + item.totalSize, 0) : 0;
  const totalEntries = showTotal ? items.reduce((acc, item) => acc + item.entries, 0) : 0;

  return (
    <div>
      <h3 className="text-xl font-semibold text-white mb-2">{title}</h3>
      <div className="overflow-x-auto" style={{maxHeight: '32rem'}}>
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-gray-800/10">
              <th className="p-3 text-white font-semibold border-b border-gray-700/30">Nome</th>
              <th className="p-3 text-white font-semibold border-b border-gray-700/30">Tamanho Total</th>
              <th className="p-3 text-white font-semibold border-b border-gray-700/30">Entradas</th>
            </tr>
          </thead>
          <tbody>
            {items.map((item) => (
              <tr key={item.name} className="hover:bg-gray-800/10 transition-all duration-200">
                <td className="p-3 text-white">{item.name.replace(/^public\./, '')}</td>
                <td className="p-3 text-gray-300">{formatSize(item.totalSize)}</td>
                <td className="p-3 text-gray-300">{item.entries.toLocaleString()}</td>
              </tr>
            ))}
            {showTotal && (
              <tr className="font-bold bg-gray-900/30">
                <td className="p-3 text-white">Total</td>
                <td className="p-3 text-gray-300">{formatSize(totalSize)}</td>
                <td className="p-3 text-gray-300">{totalEntries.toLocaleString()}</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default DbStatsTable;