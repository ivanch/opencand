import React, { useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  ArrowTrendingUpIcon,
  BanknotesIcon,
  BuildingLibraryIcon,
  ChartBarIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  ExclamationTriangleIcon,
  MapIcon,
  UsersIcon,
} from '@heroicons/react/24/outline';
import type { StatisticsData } from './statisticsRequests';
import type { EnrichmentResponse, ValueSumResponse } from '../../api/apiStatisticsModels';
import Panel from '../../shared/Panel';
import { LoadingBlock } from '../../shared/Spinner';
import { formatCurrency } from '../../utils/utils';

interface StatisticsGraphsProps {
  isLoading: boolean;
  error: string | null;
  statisticsData: StatisticsData | null;
}

type RankingType = 'candidate' | 'party' | 'state';

const RANKING_ICONS: Record<RankingType, { icon: React.ElementType; className: string }> = {
  candidate: { icon: UsersIcon, className: 'bg-indigo-50 text-indigo-600' },
  party: { icon: BuildingLibraryIcon, className: 'bg-violet-50 text-violet-600' },
  state: { icon: MapIcon, className: 'bg-sky-50 text-sky-600' },
};

const RANK_BADGE = ['bg-amber-100 text-amber-700', 'bg-slate-200 text-slate-700', 'bg-orange-100 text-orange-700'];

const MessagePanel: React.FC<{ icon: React.ElementType; iconClassName: string; title: string; text: string }> = ({ icon: Icon, iconClassName, title, text }) => (
  <Panel className="flex min-h-64 items-center justify-center">
    <div className="text-center">
      <span className={`mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-2xl ${iconClassName}`}>
        <Icon className="h-6 w-6" aria-hidden="true" />
      </span>
      <h3 className="mb-1 text-lg font-semibold text-slate-900">{title}</h3>
      <p className="text-slate-500">{text}</p>
    </div>
  </Panel>
);

const EnrichmentCard: React.FC<{ title: string; data: ReadonlyArray<EnrichmentResponse> }> = ({ title, data }) => {
  const [currentIndex, setCurrentIndex] = React.useState(0);

  // New results (e.g. after changing the filters) may have fewer items: start over from the first one
  useEffect(() => {
    setCurrentIndex(0);
  }, [data]);

  const index = Math.min(currentIndex, data.length - 1);
  const enrichmentData = data[index];
  const hasMultipleItems = data.length > 1;
  const isGain = enrichmentData.enriquecimento >= 0;

  const navButton = 'inline-flex items-center gap-1 rounded-full px-3 py-1.5 text-sm font-medium text-slate-600 transition-colors hover:bg-slate-100 hover:text-slate-900 disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:bg-transparent';

  return (
    <Panel
      title={title}
      icon={ArrowTrendingUpIcon}
      iconClassName="bg-emerald-50 text-emerald-600"
      fullHeight
      actions={hasMultipleItems ? (
        <span className="rounded-full bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-500 tabular-nums">
          {index + 1} de {data.length}
        </span>
      ) : undefined}
    >
      <div className="flex flex-1 flex-col">
        <Link
          to={`/candidato/${enrichmentData.idCandidato}`}
          className="mb-4 block break-words text-lg font-semibold text-slate-900 transition-colors hover:text-indigo-600"
        >
          {enrichmentData.nome}
        </Link>

        <div className="grid grid-cols-2 gap-3">
          <div className="rounded-xl bg-slate-50 p-3 ring-1 ring-inset ring-slate-100">
            <p className="text-xs font-medium uppercase tracking-wider text-slate-500">Patrimônio {enrichmentData.anoInicial}</p>
            <p className="mt-1 font-semibold text-slate-900 tabular-nums">{formatCurrency(enrichmentData.patrimonioInicial)}</p>
          </div>
          <div className="rounded-xl bg-slate-50 p-3 ring-1 ring-inset ring-slate-100">
            <p className="text-xs font-medium uppercase tracking-wider text-slate-500">Patrimônio {enrichmentData.anoFinal}</p>
            <p className="mt-1 font-semibold text-slate-900 tabular-nums">{formatCurrency(enrichmentData.patrimonioFinal)}</p>
          </div>
        </div>

        <div className={`mt-3 flex items-center justify-between rounded-xl p-4 ring-1 ring-inset ${isGain ? 'bg-emerald-50/70 ring-emerald-100' : 'bg-rose-50/70 ring-rose-100'}`}>
          <span className="text-sm font-medium text-slate-700">Enriquecimento</span>
          <span className={`text-xl font-bold tabular-nums ${isGain ? 'text-emerald-600' : 'text-rose-600'}`}>
            {isGain ? '+' : ''}{formatCurrency(enrichmentData.enriquecimento)}
          </span>
        </div>

        {/* Navigation buttons */}
        {hasMultipleItems && (
          <div className="mt-auto flex items-center justify-between border-t border-slate-100 pt-3">
            <button type="button" onClick={() => setCurrentIndex(index - 1)} disabled={index === 0} className={navButton}>
              <ChevronLeftIcon className="h-4 w-4" aria-hidden="true" />
              Anterior
            </button>
            <button type="button" onClick={() => setCurrentIndex(index + 1)} disabled={index === data.length - 1} className={navButton}>
              Próximo
              <ChevronRightIcon className="h-4 w-4" aria-hidden="true" />
            </button>
          </div>
        )}
      </div>
    </Panel>
  );
};

const RankingTable: React.FC<{ title: string; data: ReadonlyArray<ValueSumResponse> | null; type: RankingType }> = ({ title, data, type }) => {
  const { icon, className } = RANKING_ICONS[type];
  const th = 'py-2.5 text-xs font-semibold uppercase tracking-wider text-slate-500';

  return (
    <Panel title={title} icon={icon} iconClassName={className} fullHeight>
      {!data || data.length === 0 ? (
        <p className="py-6 text-center text-sm text-slate-500">Nenhum dado disponível</p>
      ) : (
        <div className="-mx-5 overflow-x-auto md:-mx-6 custom-scrollbar">
          <table className="w-full text-sm">
            <thead className="border-y border-slate-100 bg-slate-50/80">
              <tr>
                <th className={`${th} w-12 pl-5 text-left md:pl-6`}>#</th>
                <th className={`${th} text-left`}>
                  {type === 'candidate' ? 'Nome' : type === 'party' ? 'Partido' : 'UF'}
                </th>
                <th className={`${th} px-3 text-right`}>Ano</th>
                <th className={`${th} pr-5 text-right md:pr-6`}>Valor</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {data.slice(0, 5).map((item, index) => (
                <tr key={`${item.idCandidato ?? item.sgpartido ?? item.siglaUf}-${item.ano}-${index}`} className="transition-colors hover:bg-slate-50">
                  <td className="py-3 pl-5 md:pl-6">
                    <span className={`inline-flex h-6 w-6 items-center justify-center rounded-full text-xs font-bold ${RANK_BADGE[index] ?? 'bg-slate-100 text-slate-500'}`}>
                      {index + 1}
                    </span>
                  </td>
                  <td className="py-3 pr-3 font-medium text-slate-900">
                    {type === 'candidate' ? (
                      <Link to={`/candidato/${item.idCandidato}`} className="transition-colors hover:text-indigo-600 hover:underline">
                        {item.nome || 'N/A'}
                      </Link>
                    ) : type === 'party' ? (
                      item.sgpartido || 'N/A'
                    ) : (
                      item.siglaUf || 'N/A'
                    )}
                  </td>
                  <td className="px-3 py-3 text-right text-slate-500 tabular-nums">{item.ano || '—'}</td>
                  <td className="whitespace-nowrap py-3 pr-5 text-right font-semibold text-slate-900 tabular-nums md:pr-6">
                    {formatCurrency(item.valor)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </Panel>
  );
};

const GroupHeading: React.FC<{ icon: React.ElementType; children: React.ReactNode }> = ({ icon: Icon, children }) => (
  <h2 className="mb-3 mt-2 flex items-center gap-2 text-sm font-semibold uppercase tracking-wider text-slate-400">
    <Icon className="h-4 w-4" aria-hidden="true" />
    {children}
  </h2>
);

const StatisticsGraphs: React.FC<StatisticsGraphsProps> = ({
  isLoading,
  error,
  statisticsData,
}) => {
  if (error) {
    return <MessagePanel icon={ExclamationTriangleIcon} iconClassName="bg-rose-50 text-rose-600" title="Erro" text={error} />;
  }

  if (isLoading) {
    return (
      <Panel className="flex min-h-64 items-center justify-center">
        <LoadingBlock text="Carregando dados..." />
      </Panel>
    );
  }

  if (!statisticsData) {
    return (
      <MessagePanel
        icon={ChartBarIcon}
        iconClassName="bg-slate-100 text-slate-500"
        title="Nenhum dado encontrado"
        text="Os dados estatísticos não puderam ser carregados"
      />
    );
  }

  return (
    <div className="space-y-8">
      <section>
        <GroupHeading icon={UsersIcon}>Candidatos</GroupHeading>
        <div className="grid grid-cols-1 gap-5 xl:grid-cols-2">
          <RankingTable title="Maiores Bens" data={statisticsData.candidatesWithMostAssets} type="candidate" />
          {statisticsData.enrichmentData && statisticsData.enrichmentData.length > 0 && (
            <EnrichmentCard title="Análise de Enriquecimento" data={statisticsData.enrichmentData} />
          )}
          <RankingTable title="Maiores Receitas" data={statisticsData.candidatesWithMostRevenue} type="candidate" />
          <RankingTable title="Maiores Despesas" data={statisticsData.candidatesWithMostExpenses} type="candidate" />
        </div>
      </section>

      <section>
        <GroupHeading icon={BuildingLibraryIcon}>Partidos</GroupHeading>
        <div className="grid grid-cols-1 gap-5 md:grid-cols-2 2xl:grid-cols-3">
          <RankingTable title="Maiores Bens" data={statisticsData.partiesWithMostAssets} type="party" />
          <RankingTable title="Maiores Despesas" data={statisticsData.partiesWithMostExpenses} type="party" />
          <RankingTable title="Maiores Receitas" data={statisticsData.partiesWithMostRevenue} type="party" />
        </div>
      </section>

      <section>
        <GroupHeading icon={BanknotesIcon}>Unidades Federativas</GroupHeading>
        <div className="grid grid-cols-1 gap-5 md:grid-cols-2 2xl:grid-cols-3">
          <RankingTable title="Maiores Bens" data={statisticsData.statesWithMostAssets} type="state" />
          <RankingTable title="Maiores Despesas" data={statisticsData.statesWithMostExpenses} type="state" />
          <RankingTable title="Maiores Receitas" data={statisticsData.statesWithMostRevenue} type="state" />
        </div>
      </section>
    </div>
  );
};

export default StatisticsGraphs;
