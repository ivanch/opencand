import type { ComponentType } from 'react';
import { ArrowTrendingDownIcon, ArrowTrendingUpIcon } from '@heroicons/react/24/outline';
import { openCandApi, type Expense, type Income } from '../../../api';

export interface FinanceRow {
  id: string;
  ano: number;
  data: string | null;
  counterpart: string;
  isPerson: boolean;
  descricao: string | null;
  valor: number;
  details: Array<{ label: string; value: string }>;
}

export interface FinancePage {
  rows: FinanceRow[];
  total: number;
}

export type FinanceAccent = 'red' | 'green';

/** Everything that differs between "despesas" and "receitas" (labels, colors, endpoint and row mapping). */
export interface FinanceKind {
  title: string;
  totalLabel: string;
  counterpartHeader: string;
  detailsTitle: string;
  emptyText: string;
  accent: FinanceAccent;
  icon: ComponentType<{ className?: string }>;
  fetchPage: (idCandidato: string, ano: number, page: number, pageSize: number, signal: AbortSignal) => Promise<FinancePage>;
}

const optionalDetails = (entries: Array<[string, string | null | undefined]>) =>
  entries.filter(([, value]) => !!value).map(([label, value]) => ({ label, value: value as string }));

const expenseRow = (item: Expense): FinanceRow => ({
  id: item.idDespesa,
  ano: item.ano,
  data: item.dataDespesa,
  counterpart: item.nomeFornecedor || item.nomeFornecedorRFB || 'Fornecedor não informado',
  isPerson: !!item.cpfFornecedor,
  descricao: item.descricao,
  valor: item.valor,
  details: [
    { label: 'ID', value: item.idDespesa },
    { label: 'Ano', value: String(item.ano) },
    { label: 'Turno', value: String(item.turno) },
    { label: 'Partido', value: item.sgPartido },
    { label: 'Tipo Fornecedor', value: item.tipoFornecedor || '-' },
    { label: 'Tipo Documento', value: item.tipoDocumento || '-' },
    { label: 'Origem', value: item.origemDespesa || '-' },
    ...optionalDetails([
      ['CPF', item.cpfFornecedor],
      ['CNPJ', item.cnpjFornecedor],
      ['Município', item.municipioFornecedor],
    ]),
  ],
});

const incomeRow = (item: Income): FinanceRow => ({
  id: item.idReceita,
  ano: item.ano,
  data: item.dataReceita,
  counterpart: item.nomeDoador || item.nomeDoadorRFB || 'Doador não informado',
  isPerson: !!item.cpfDoador,
  descricao: item.descricao,
  valor: item.valor,
  details: [
    { label: 'ID', value: item.idReceita },
    { label: 'Ano', value: String(item.ano) },
    { label: 'Turno', value: String(item.turno) },
    { label: 'Fonte', value: item.fonteReceita || '-' },
    { label: 'Origem', value: item.origemReceita || '-' },
    { label: 'Natureza', value: item.naturezaReceita || '-' },
    { label: 'Espécie', value: item.especieReceita || '-' },
    ...optionalDetails([
      ['CPF', item.cpfDoador],
      ['CNPJ', item.cnpjDoador],
      ['Município', item.municipioDoador],
    ]),
  ],
});

export const EXPENSE_KIND: FinanceKind = {
  title: 'Despesas',
  totalLabel: 'Total de Despesas',
  counterpartHeader: 'Fornecedor',
  detailsTitle: 'Detalhes da Despesa',
  emptyText: 'Nenhuma despesa encontrada',
  accent: 'red',
  icon: ArrowTrendingDownIcon,
  fetchPage: async (id, ano, page, pageSize, signal) => {
    const result = await openCandApi.getCandidateDespesasPage(id, ano, page, pageSize, signal);
    return { rows: (result.despesas ?? []).map(expenseRow), total: result.total ?? 0 };
  },
};

export const INCOME_KIND: FinanceKind = {
  title: 'Receitas',
  totalLabel: 'Total de Receitas',
  counterpartHeader: 'Doador',
  detailsTitle: 'Detalhes da Receita',
  emptyText: 'Nenhuma receita encontrada',
  accent: 'green',
  icon: ArrowTrendingUpIcon,
  fetchPage: async (id, ano, page, pageSize, signal) => {
    const result = await openCandApi.getCandidateReceitasPage(id, ano, page, pageSize, signal);
    return { rows: (result.receitas ?? []).map(incomeRow), total: result.total ?? 0 };
  },
};
