/** Tooltip text for a "resultado" of the TSE */
export const getResultadoHelper = (resultado: string | null): string => {
  const value = resultado?.toLowerCase() || '';
  if (value === 'eleito') return 'Eleito!';
  if (value === 'não eleito') return 'Não Eleito';
  if (value === 'eleito por qp') return 'Eleito por Quociente Partidário';
  if (value === 'eleito por média') return 'Eleito por Média';
  if (value.includes('nulo')) return 'Dado não disponível';
  if (value === '2º turno') return 'Foi para o segundo Turno';
  if (value === 'suplente') return 'Foi eleito como suplente';
  return '';
};

/** Badge colors per outcome: elected = green, runoff = amber, suplente = sky, null = rose, everything else neutral */
export const getResultadoBadgeClass = (resultado: string | null): string => {
  const value = resultado?.toLowerCase() || '';
  if (value.startsWith('eleito')) return 'bg-emerald-50 text-emerald-700 ring-emerald-600/20';
  if (value === '2º turno') return 'bg-amber-50 text-amber-700 ring-amber-600/20';
  if (value === 'suplente') return 'bg-sky-50 text-sky-700 ring-sky-600/20';
  if (value.includes('nulo')) return 'bg-rose-50 text-rose-700 ring-rose-600/20';
  return 'bg-slate-100 text-slate-600 ring-slate-500/20';
};
