import React, { useState, useEffect } from 'react';
import { BanknotesIcon, CalendarDaysIcon, LinkIcon, UsersIcon } from '@heroicons/react/24/outline';
import { openCandApi, type PlatformStats, ApiError } from '../api';
import Card from '../shared/Card';
import SectionHeading from '../shared/SectionHeading';
import { formatCompactNumber, formatCurrencyCompact } from '../utils/utils';

interface StatCardProps {
  icon: React.ElementType;
  title: string;
  value: string;
  description: string;
  isLoading?: boolean;
}

const StatCard: React.FC<StatCardProps> = ({ icon: Icon, title, value, description, isLoading = false }) => {
  return (
    <Card hasAnimation className="h-full text-left">
      <div className="mb-5 flex items-center gap-3">
        <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-indigo-500/15 text-indigo-300 ring-1 ring-inset ring-indigo-400/20">
          <Icon className="h-5 w-5" aria-hidden="true" />
        </span>
        <h3 className="text-sm font-medium text-slate-300">{title}</h3>
      </div>
      {isLoading ? (
        <div className="space-y-3" aria-hidden="true">
          <div className="skeleton h-9 w-32" />
          <div className="skeleton h-4 w-full" />
        </div>
      ) : (
        <>
          <p className="text-4xl font-bold tracking-tight text-white tabular-nums">{value}</p>
          <p className="mt-2 text-sm text-slate-400">{description}</p>
        </>
      )}
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
      } finally {
        setIsLoading(false);
      }
    };

    fetchStats();
  }, []);

  const statisticsData = [
    {
      icon: UsersIcon,
      title: 'Candidatos',
      value: `+${formatCompactNumber(stats?.totalCandidatos)}`,
      description: 'Registros de candidaturas na plataforma',
    },
    {
      icon: BanknotesIcon,
      title: 'Bens Registrados',
      value: `+${formatCompactNumber(stats?.totalBemCandidatos)}`,
      description: `Somando ${formatCurrencyCompact(stats?.totalValorBemCandidatos)} em patrimônio declarado`,
    },
    {
      icon: LinkIcon,
      title: 'Redes Sociais',
      value: formatCompactNumber(stats?.totalRedesSociais),
      description: 'Perfis vinculados aos candidatos',
    },
    {
      icon: CalendarDaysIcon,
      title: 'Eleições',
      value: formatCompactNumber(stats?.totalEleicoes),
      description: 'Eleições processadas no sistema',
    },
  ];

  return (
    <section id="stats" className="py-20 md:py-24">
      <div className="mx-auto max-w-7xl px-4 sm:px-6">
        <SectionHeading
          eyebrow="Em números"
          title="Dados em Números"
          subtitle="Estatísticas da nossa plataforma de dados eleitorais"
          className="mb-12"
        />

        {error && !isLoading ? (
          <p className="mx-auto max-w-xl rounded-2xl bg-rose-500/10 px-6 py-4 text-center text-sm text-rose-200 ring-1 ring-inset ring-rose-400/20">
            {error}
          </p>
        ) : (
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
            {statisticsData.map((stat) => (
              <StatCard
                key={stat.title}
                icon={stat.icon}
                title={stat.title}
                value={stat.value}
                description={stat.description}
                isLoading={isLoading}
              />
            ))}
          </div>
        )}
      </div>
    </section>
  );
};

export default StatisticsSection;
