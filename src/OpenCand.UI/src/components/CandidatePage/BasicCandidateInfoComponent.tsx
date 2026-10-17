import React, { use, useState } from 'react';
import { UserIcon } from '@heroicons/react/24/outline';
import { type CandidateDetails, openCandApi } from '../../api';
import { formatCpf, maskCpf } from '../../utils/utils';

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

  const formatDate = (dateString: string) => {
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('pt-BR');
    } catch {
      return dateString;
    }
  };

  const handleCpfClick = async () => {
    if (!candidateDetails?.idCandidato || isRevealingCpf || revealedCpf) return;
    
    setIsRevealingCpf(true);
    try {
      const cpfResult = await openCandApi.getCandidateCpf(candidateDetails?.idCandidato || '');
      setRevealedCpf(formatCpf(cpfResult.cpf));
    } catch (error) {
      console.error('Failed to reveal CPF:', error);
    } finally {
      setIsRevealingCpf(false);
    }
  };

  const getDetailsList = () => {
    if (!candidateDetails) return null;
  }

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
        <label className="text-sm font-semibold text-gray-600 uppercase tracking-wide">{title}</label>
        <div className="mt-1 space-y-1">
          {Array.from(dataMap.entries()).map(([value, years], index) => (
            <div key={index} className="text-center">
              <p className="text-lg text-gray-900">
                {value} <span className="text-xs text-gray-500 ml-2">{years.join(', ')}</span>
              </p>
            </div>
          ))}
        </div>
      </div>
    );
  };

  return (
    <div className="bg-white/95 backdrop-blur-sm rounded-xl shadow-lg hover:shadow-xl transform hover:scale-[1.01] transition-all duration-200 p-6">
      <div className="flex items-center mb-6">
        <UserIcon className="h-8 w-8 text-blue-600 mr-3" />
        <h2 className="text-2xl font-bold text-gray-900">Informações Básicas</h2>
      </div>

      {isLoading ? (
        <div className="flex items-center justify-center h-64">
          <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-600 border-t-transparent"></div>
        </div>
      ) : candidateDetails ? (
        <div className="space-y-4">
          {/* Candidate Photo */}
          {candidateDetails.fotoUrl && (
            <div className="flex justify-center mb-6">
              <div className="relative">
                <img
                  src={candidateDetails.fotoUrl}
                  alt={`Foto de ${candidateDetails.nome}`}
                  className="w-36 h-36 rounded-full object-cover border-4 border-blue-200 shadow-lg"
                  onError={(e) => {
                    const target = e.target as HTMLImageElement;
                    target.style.display = 'none';
                  }}
                />
              </div>
            </div>
          )}

          <div>
            <label className="text-sm font-semibold text-gray-600 uppercase tracking-wide">Nome</label>
            <p className="text-lg font-medium text-gray-900 mt-1">{candidateDetails.nome}</p>
          </div>
          
          {candidateDetails.cpf && 
            <div>
              <label className="text-sm font-semibold text-gray-600 uppercase tracking-wide">CPF</label>
              <div 
                className={`text-lg text-gray-900 mt-1 ${
                  !revealedCpf && !isRevealingCpf ? 'cursor-pointer hover:text-blue-600 transition-colors duration-200' : ''
                }`}
                onClick={handleCpfClick}
              >
                {isRevealingCpf ? (
                  <div className="flex items-center justify-center space-x-2 py-1">
                    <div className="animate-spin rounded-full h-4 w-4 border-2 border-blue-200 border-t-blue-600"></div>
                    <span className="animate-pulse text-blue-600">Carregando...</span>
                  </div>
                ) : (
                  <span className={revealedCpf ? '' : 'select-none'}>
                    {revealedCpf || maskCpf(candidateDetails.cpf)}
                  </span>
                )}
              </div>
            </div>
          }

          <div>
            <label className="text-sm font-semibold text-gray-600 uppercase tracking-wide">Data de Nascimento</label>
            <p className="text-lg text-gray-900 mt-1">{formatDate(candidateDetails.dataNascimento)}</p>
          </div>

          <div>
            <label className="text-sm font-semibold text-gray-600 uppercase tracking-wide">Sexo</label>
            <p className="text-lg text-gray-900 mt-1">{candidateDetails.sexo}</p>
          </div>

          {/* Historical Data Section */}
          {(() => {
            const historicalData = getHistoricalData();
            if (!historicalData) return null;

            return (
              <div className="border-t border-gray-200 pt-4 mt-6">
                <div className="space-y-4">
                  {renderHistoricalField("Email", historicalData.emails)}
                  {renderHistoricalField("Estado Civil", historicalData.estadosCivis)}
                  {renderHistoricalField("Escolaridade", historicalData.escolaridades)}
                  {renderHistoricalField("Ocupação", historicalData.ocupacoes)}
                </div>
              </div>
            );
          })()}
        </div>
      ) : (
        <div className="text-center text-gray-500 py-8">
          Nenhuma informação encontrada
        </div>
      )}
    </div>
  );
};

export default BasicCandidateInfoComponent;
