import React, { useState } from 'react';
import { EyeIcon, UserIcon } from '@heroicons/react/24/outline';
import { type CandidateDetails, openCandApi } from '../../api';
import { formatCpf, formatDateToDDMMYYYY, maskCpf } from '../../utils/utils';
import Panel from '../../shared/Panel';
import Spinner, { LoadingBlock } from '../../shared/Spinner';

interface BasicCandidateInfoComponentProps {
  candidateDetails: CandidateDetails | null;
  isLoading: boolean;
}

const BasicCandidateInfoComponent: React.FC<BasicCandidateInfoComponentProps> = ({ 
  candidateDetails, 
  isLoading 
}) => {
  const [revealedCpf, setRevealedCpf] = useState<string | null>(null);
  const [isRevealingCpf, setIsRevealingCpf] = useState(false);

  React.useEffect(() => {
    if (candidateDetails) {
      setRevealedCpf(null);
    }
  }, [candidateDetails]);

  const formatDate = (dateString: string) => formatDateToDDMMYYYY(dateString) || dateString;

  const handleCpfClick = async () => {
    if (!candidateDetails?.idCandidato || isRevealingCpf || revealedCpf) return;
    
    setIsRevealingCpf(true);
    try {
      const cpfResult = await openCandApi.getCandidateCpf(candidateDetails?.idCandidato || '');
      if (cpfResult?.cpf) {
        setRevealedCpf(formatCpf(cpfResult.cpf));
      }
    } catch (error) {
      console.error('Failed to reveal CPF:', error);
    } finally {
      setIsRevealingCpf(false);
    }
  };

  const getHistoricalData = () => {
    if (!candidateDetails?.candidatoExt || candidateDetails.candidatoExt.length === 0) {
      return null;
    }

    const historicalData = {
      emails: new Map<string, number[]>(),
      estadosCivis: new Map<string, number[]>(),
      escolaridades: new Map<string, number[]>(),
      ocupacoes: new Map<string, number[]>()
    };

    // Group data by value and collect years
    candidateDetails.candidatoExt.forEach(ext => {
      if (ext.email) {
        const years = historicalData.emails.get(ext.email) || [];
        years.push(ext.ano);
        historicalData.emails.set(ext.email, years);
      }
      if (ext.estadoCivil) {
        const years = historicalData.estadosCivis.get(ext.estadoCivil) || [];
        years.push(ext.ano);
        historicalData.estadosCivis.set(ext.estadoCivil, years);
      }
      if (ext.escolaridade) {
        const years = historicalData.escolaridades.get(ext.escolaridade) || [];
        years.push(ext.ano);
        historicalData.escolaridades.set(ext.escolaridade, years);
      }
      if (ext.ocupacao) {
        const years = historicalData.ocupacoes.get(ext.ocupacao) || [];
        years.push(ext.ano);
        historicalData.ocupacoes.set(ext.ocupacao, years);
      }
    });

    // Sort years for each entry
    historicalData.emails.forEach((years, key) => {
      historicalData.emails.set(key, years.sort((a, b) => b - a));
    });
    historicalData.estadosCivis.forEach((years, key) => {
      historicalData.estadosCivis.set(key, years.sort((a, b) => b - a));
    });
    historicalData.escolaridades.forEach((years, key) => {
      historicalData.escolaridades.set(key, years.sort((a, b) => b - a));
    });
    historicalData.ocupacoes.forEach((years, key) => {
      historicalData.ocupacoes.set(key, years.sort((a, b) => b - a));
    });

    return historicalData;
  };

  const renderHistoricalField = (title: string, dataMap: Map<string, number[]>) => {
    if (dataMap.size === 0) return null;

    return (
      <div>
        <dt className="text-xs font-medium uppercase tracking-wider text-slate-500">{title}</dt>
        <dd className="mt-1.5 space-y-1.5">
          {Array.from(dataMap.entries()).map(([value, years]) => (
            <div key={value} className="flex flex-wrap items-baseline justify-between gap-x-3 gap-y-1">
              <span className="break-words text-slate-900">{value}</span>
              <span className="flex flex-wrap gap-1">
                {years.map(year => (
                  <span key={year} className="rounded-md bg-slate-100 px-1.5 py-0.5 text-xs font-medium text-slate-500 tabular-nums">
                    {year}
                  </span>
                ))}
              </span>
            </div>
          ))}
        </dd>
      </div>
    );
  };

  const renderField = (label: string, value: React.ReactNode) => (
    <div>
      <dt className="text-xs font-medium uppercase tracking-wider text-slate-500">{label}</dt>
      <dd className="mt-1 text-slate-900">{value || <span className="text-slate-400">Não informado</span>}</dd>
    </div>
  );

  const historicalData = getHistoricalData();

  return (
    <Panel title="Informações Básicas" icon={UserIcon} iconClassName="bg-blue-50 text-blue-600">
      {isLoading ? (
        <LoadingBlock heightClassName="h-48" colorClassName="border-blue-600" />
      ) : candidateDetails ? (
        <dl className="space-y-4">
          {renderField('Nome', <span className="font-medium">{candidateDetails.nome}</span>)}

          {candidateDetails.cpf && renderField('CPF', (
            revealedCpf ? (
              <span className="tabular-nums">{revealedCpf}</span>
            ) : (
              <button
                type="button"
                onClick={handleCpfClick}
                disabled={isRevealingCpf}
                className="group inline-flex items-center gap-2 rounded-lg text-left tabular-nums transition-colors hover:text-blue-600 disabled:cursor-wait"
                title="Mostrar CPF completo"
              >
                <span className="select-none">{maskCpf(candidateDetails.cpf)}</span>
                {isRevealingCpf ? (
                  <Spinner size="sm" colorClassName="border-blue-600" />
                ) : (
                  <span className="inline-flex items-center gap-1 rounded-full bg-blue-50 px-2 py-0.5 text-xs font-medium text-blue-700 group-hover:bg-blue-100">
                    <EyeIcon className="h-3.5 w-3.5" aria-hidden="true" />
                    Mostrar
                  </span>
                )}
              </button>
            )
          ))}

          <div className="grid grid-cols-2 gap-4">
            {renderField('Data de Nascimento', <span className="tabular-nums">{formatDate(candidateDetails.dataNascimento)}</span>)}
            {renderField('Sexo', candidateDetails.sexo)}
          </div>

          {historicalData && (
            <div className="space-y-4 border-t border-slate-100 pt-4">
              {renderHistoricalField('Email', historicalData.emails)}
              {renderHistoricalField('Estado Civil', historicalData.estadosCivis)}
              {renderHistoricalField('Escolaridade', historicalData.escolaridades)}
              {renderHistoricalField('Ocupação', historicalData.ocupacoes)}
            </div>
          )}
        </dl>
      ) : (
        <p className="py-8 text-center text-slate-500">Nenhuma informação encontrada</p>
      )}
    </Panel>
  );
};

export default BasicCandidateInfoComponent;
