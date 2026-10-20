import React from 'react';
import { formatBytes } from '../../utils/utils';

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

const th = 'p-3 text-xs font-semibold uppercase tracking-wider text-slate-400 border-b border-white/10';

const DbStatsTable: React.FC<DbStatsTableProps> = ({ title, items, showTotal = false }) => {
  const totalSize = showTotal ? items.reduce((acc, item) => acc + item.totalSize, 0) : 0;
  const totalEntries = showTotal ? items.reduce((acc, item) => acc + item.entries, 0) : 0;

  return (
    <div>
      <h3 className="mb-3 text-base font-semibold text-white">{title}</h3>
      <div className="overflow-auto rounded-xl ring-1 ring-inset ring-white/10 custom-scrollbar" style={{ maxHeight: '32rem' }}>
        <table className="w-full border-collapse text-left text-sm">
          <thead className="sticky top-0 bg-surface-900">
            <tr>
              <th className={th}>Nome</th>
              <th className={`${th} text-right`}>Tamanho Total</th>
              <th className={`${th} text-right`}>Entradas</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-white/5">
            {items.map((item) => (
              <tr key={item.name} className="transition-colors hover:bg-white/[0.03]">
                <td className="p-3 font-mono text-[13px] text-slate-100">{item.name.replace(/^public\./, '')}</td>
                <td className="p-3 text-right text-slate-300 tabular-nums">{formatBytes(item.totalSize)}</td>
                <td className="p-3 text-right text-slate-300 tabular-nums">{item.entries.toLocaleString('pt-BR')}</td>
              </tr>
            ))}
            {showTotal && (
              <tr className="bg-white/[0.04] font-semibold">
                <td className="p-3 text-white">Total</td>
                <td className="p-3 text-right text-white tabular-nums">{formatBytes(totalSize)}</td>
                <td className="p-3 text-right text-white tabular-nums">{totalEntries.toLocaleString('pt-BR')}</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default DbStatsTable;
