import React from 'react';
import { formatBytes } from '../../utils/utils';

interface DbIndexesStatsProps {
  amount: number;
  size: number;
}

const DbIndexesStats: React.FC<DbIndexesStatsProps> = ({ amount, size }) => {
  return (
    <div>
      <h3 className="mb-3 text-base font-semibold text-white">Índices</h3>
      <dl className="grid grid-cols-2 gap-3">
        <div className="rounded-xl bg-white/[0.03] p-4 ring-1 ring-inset ring-white/10">
          <dt className="text-xs font-medium uppercase tracking-wider text-slate-400">Quantidade</dt>
          <dd className="mt-1 text-2xl font-bold text-white tabular-nums">{amount.toLocaleString('pt-BR')}</dd>
        </div>
        <div className="rounded-xl bg-white/[0.03] p-4 ring-1 ring-inset ring-white/10">
          <dt className="text-xs font-medium uppercase tracking-wider text-slate-400">Tamanho Total</dt>
          <dd className="mt-1 text-2xl font-bold text-white tabular-nums">{formatBytes(size)}</dd>
        </div>
      </dl>
    </div>
  );
};

export default DbIndexesStats;
