import React, { useMemo } from 'react';
import { DocumentTextIcon } from '@heroicons/react/24/outline';
import { type Election } from '../../api';
import Tooltip from '../../shared/Tooltip';
import Panel from '../../shared/Panel';
import { LoadingBlock } from '../../shared/Spinner';
import { getResultadoBadgeClass, getResultadoHelper } from './electionResult';

interface ElectionsComponentProps {
  elections: Election[] | null;
  isLoading: boolean;
}

const th = 'px-3 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500 whitespace-nowrap';
const td = 'px-3 py-3 text-slate-700 whitespace-nowrap';

const ElectionsComponent: React.FC<ElectionsComponentProps> = ({ elections, isLoading }) => {
  const sortedElections = useMemo(
    () => (elections ? [...elections].sort((a, b) => b.ano - a.ano) : []),
    [elections]
  );

  return (
    <Panel
      title="Histórico de Eleições"
      subtitle={sortedElections.length > 0 ? `${sortedElections.length} ${sortedElections.length === 1 ? 'candidatura' : 'candidaturas'}` : undefined}
      icon={DocumentTextIcon}
      iconClassName="bg-emerald-50 text-emerald-600"
    >
      {isLoading ? (
        <LoadingBlock colorClassName="border-emerald-600" />
      ) : sortedElections.length > 0 ? (
        <div className="-mx-5 overflow-x-auto md:-mx-6 custom-scrollbar">
          <table className="w-full text-sm">
            <thead className="border-y border-slate-100 bg-slate-50/80">
              <tr>
                <th className={`${th} pl-5 md:pl-6`}>Ano</th>
                <th className={th}>Cargo</th>
                <th className={th}>Localidade</th>
                <th className={th}>Partido</th>
                <th className={th}>Número</th>
                <th className={th}>Tipo</th>
                <th className={`${th} pr-5 md:pr-6`}>Resultado</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {sortedElections.map((election: Election, index: number) => (
                <tr key={election.sqCandidato || index} className="transition-colors hover:bg-slate-50">
                  <td className={`${td} pl-5 font-semibold text-slate-900 tabular-nums md:pl-6`}>{election.ano}</td>
                  <td className={`${td} font-medium text-slate-900`}>{election.cargo}</td>
                  <td className={td}>
                    {election.nomeUE}
                    {election.siglaUF && <span className="ml-1.5 text-xs text-slate-400">{election.siglaUF}</span>}
                  </td>
                  <td className={td}>
                    <Tooltip content={election.partido?.nome ?? ''} position="top" delay={200}>
                      <span className="font-medium">{election.partido?.sigla}</span>
                    </Tooltip>
                  </td>
                  <td className={`${td} tabular-nums`}>{election.nrCandidato}</td>
                  <td className={`${td} text-slate-500`}>{election.tipoEleicao}</td>
                  <td className={`${td} pr-5 md:pr-6`}>
                    {election.resultado && (
                      <Tooltip
                        content={getResultadoHelper(election.resultado)}
                        position="top"
                        delay={200}
                      >
                        <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-semibold ring-1 ring-inset ${getResultadoBadgeClass(election.resultado)}`}>
                          {election.resultado}
                        </span>
                      </Tooltip>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <p className="py-8 text-center text-slate-500">Nenhum histórico de eleições encontrado</p>
      )}
    </Panel>
  );
};

export default ElectionsComponent;
