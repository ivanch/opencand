import React, { useState, useEffect } from 'react';
import { openCandApi, type PlatformStats, ApiError } from '../api';
import Card from '../shared/Card';

interface StatCardProps {
  title: string;
  value: string;
  description: string;
  isLoading?: boolean;
}

const StatCard: React.FC<StatCardProps> = ({ title, value, description, isLoading = false }) => {
  return (
    <Card hasAnimation={true} disableCursor={true} height={11} width={20}>
      <h3 className="text-indigo-400 text-xl font-semibold mb-2">{title}</h3>
      {isLoading ? (
        <div className="h-12 flex items-center justify-center">
          <div className="animate-spin rounded-full border-4 border-indigo-600 border-t-transparent"></div>
        </div>
      ) : (
        <p className="text-4xl font-bold text-white mb-3">{value}</p>
      )}
      <p className="text-gray-400 text-sm">{description}</p>
    </Card>
  );
};

const StatisticsSection: React.FC = () => {
  const [stats, setStats] = useState<PlatformStats | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        setIsLoading(true);
        setError(null);
        const platformStats = await openCandApi.getStats();
        setStats(platformStats);
      } catch (err) {
        console.error('Error fetching platform stats:', err);
        if (err instanceof ApiError) {
          setError(`Erro ao carregar estatísticas: ${err.message}`);
        } else {
          setError('Erro inesperado ao carregar estatísticas');
        }
        // Use default stats as fallback
        setStats({
          totalCandidatos: 0,
          totalBemCandidatos: 0,
          totalEleicoes: 0,
          totalValorBemCandidatos: 0,
          totalRedesSociais: 0
        });
      } finally {
        setIsLoading(false);
      }
    };

    fetchStats();
  }, []);

  const formatNumber = (num: number): string => {
    if (num >= 1000000000000) {
      return `${(num / 1000000000000).toFixed(1)} trilhão`;
    } else if (num >= 1000000000) {
      return `${(num / 1000000000).toFixed(1)} bilhão`;
    } else if (num >= 1000000) {
      return `${(num / 1000000).toFixed(1)} milhão`;
    } else if (num >= 1000) {
      return `${(num / 1000).toFixed(0)}K`;
    }
    return num.toLocaleString('pt-BR');
  };

  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
      notation: 'compact',
      maximumSignificantDigits: 3
    }).format(value);
  };

  const statisticsData = [
    {
      title: "Total de Candidatos",
      value: isLoading ? "" : `+${formatNumber(stats?.totalCandidatos || 0)}`,
      description: "Registros de candidaturas na plataforma"
    },
    {
      title: "Total de Bens Registrados", 
      value: isLoading ? "" : `+${formatNumber(stats?.totalBemCandidatos || 0)}`,
      description: isLoading ? "" : `Somando ${formatCurrency(stats?.totalValorBemCandidatos || 0)} em Patrimônio agregado declarado pelos candidatos`
    },
    {
      title: "Total de Redes Sociais",
      value: isLoading ? "" : formatNumber(stats?.totalRedesSociais || 0),
      description: "Redes sociais conectadas aos candidatos"
    },
    {
      title: "Total de Eleições",
      value: isLoading ? "" : formatNumber(stats?.totalEleicoes || 0),
      description: "Eleições processadas no sistema"
    }
  ];

  return (
    <section id="stats" className="py-20">
      <div className="max-w-7xl mx-auto px-4">
        <div className="text-center mb-12 animate-fade-in">
          <h2 className="text-4xl md:text-5xl font-bold text-white mb-4">
            Dados em Números
          </h2>
          <p className="text-xl text-gray-400">
            Estatísticas da nossa plataforma de dados eleitorais
          </p>
        </div>
        
        <div className="flex flex-wrap justify-center gap-8">
          {statisticsData.map((stat) => (
            <StatCard 
              title={stat.title} 
              value={stat.value} 
              description={stat.description}
              isLoading={isLoading}
            />
          ))}
        </div>
      </div>
    </section>
  );
};

export default StatisticsSection;
