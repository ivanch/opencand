import React, { useState, useEffect } from 'react';
import { FaGoogleDrive } from 'react-icons/fa';
import { CalendarDaysIcon, CircleStackIcon, ClockIcon, CloudArrowDownIcon, Squares2X2Icon } from '@heroicons/react/24/outline';
import { openCandApi } from '../api';
import type { OpenCandDataAvailabilityStats, OpenCandDatabaseStats } from '../api/apiModels';
import Card from '../shared/Card';
import ErrorPage from './ErrorPage';
import DataAvailabilityTable from './DataStats/DataAvailabilityTable';
import { dataTypes } from './DataStats/dataTypes';
import DbStatsTable from './DataStats/DbStatsTable';
import DbIndexesStats from './DataStats/DbIndexesStats';
import Button from '../shared/Button';
import SectionHeading from '../shared/SectionHeading';
import { LoadingBlock } from '../shared/Spinner';
import { usePageTitle } from '../hooks/usePageTitle';

const DataStatsPage: React.FC = () => {
  const [stats, setStats] = useState<OpenCandDataAvailabilityStats | null>(null);
  const [dbStats, setDbStats] = useState<OpenCandDatabaseStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [dbLoading, setDbLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [dbError, setDbError] = useState<string | null>(null);

  usePageTitle('Dados Disponíveis');

  useEffect(() => {
    const fetchStats = async () => {
      try {
        setLoading(true);
        const data = await openCandApi.getDataAvailabilityStats();
        setStats(data);
      } catch (err) {
        setError('Erro ao carregar matriz de disponibilidade de dados');
        console.error('Error fetching data availability stats:', err);
      } finally {
        setLoading(false);
      }
    };
    const fetchDbStats = async () => {
      try {
        setDbLoading(true);
        const dbData = await openCandApi.getDatabaseTechStats();
        setDbStats(dbData);
      } catch (err) {
        setDbError('Erro ao carregar estatísticas técnicas do banco de dados');
        console.error('Error fetching database tech stats:', err);
      } finally {
        setDbLoading(false);
      }
    };
    fetchStats();
    fetchDbStats();
  }, []);

  if (loading) {
    return <LoadingBlock heightClassName="min-h-[50vh]" colorClassName="border-indigo-400" text="Carregando dados..." />;
  }

  if (error) {
    return (
      <ErrorPage
        title="Erro"
        description={error}
        helperText="Tente novamente ou volte para a página inicial."
      />
    );
  }

  if (!stats) {
    return (
      <div className="flex min-h-[50vh] items-center justify-center">
        <p className="text-lg text-slate-400">Nenhum dado disponível</p>
      </div>
    );
  }

  // Get all unique years from all data types
  const allYears = new Set<number>();
  Object.values(stats).forEach((yearArray: number[]) => {
    yearArray.forEach((year: number) => allYears.add(year));
  });
  const sortedYears = Array.from(allYears).sort((a, b) => b - a);

  const summary = [
    { icon: Squares2X2Icon, value: String(dataTypes.length), label: 'Tipos de Dados' },
    { icon: CalendarDaysIcon, value: String(sortedYears.length), label: 'Anos Disponíveis' },
    {
      icon: ClockIcon,
      value: sortedYears.length > 0 ? `${Math.min(...sortedYears)} – ${Math.max(...sortedYears)}` : 'N/A',
      label: 'Período',
    },
  ];

  return (
    <div className="mx-auto max-w-7xl px-4 pb-8 pt-10 sm:px-6 md:pt-14">
      <SectionHeading
        as="h1"
        eyebrow="Transparência"
        title="Disponibilidade de Dados"
        subtitle="Visualize a disponibilidade dos dados por ano em nossa base"
        className="mb-10 md:mb-12"
      />

      {/* Download DB Dump Section */}
      <Card className="mx-auto mb-10 flex max-w-3xl flex-col items-center gap-5 text-center sm:flex-row sm:text-left">
        <span className="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-emerald-500/15 text-emerald-300 ring-1 ring-inset ring-emerald-400/20">
          <CloudArrowDownIcon className="h-6 w-6" aria-hidden="true" />
        </span>
        <div className="flex-1">
          <h2 className="text-lg font-semibold text-white">Download do Dump do Banco de Dados</h2>
          <p className="mt-1 text-sm text-slate-400">Baixe a base completa (PostgreSQL) para suas próprias análises.</p>
        </div>
        <Button
          href="https://drive.google.com/file/d/1cfMItrsAdv8y8YUNp04D33s6pYrRbmDn/view?usp=sharing"
          target="_blank"
          rel="noopener noreferrer"
          variant="primary"
        >
          <FaGoogleDrive aria-hidden="true" />
          Google Drive
        </Button>
      </Card>

      {/* Stats Cards */}
      <div className="mb-10 grid grid-cols-1 gap-5 md:grid-cols-3">
        {summary.map(({ icon: Icon, value, label }) => (
          <Card key={label} hasAnimation className="flex items-center gap-4">
            <span className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-indigo-500/15 text-indigo-300 ring-1 ring-inset ring-indigo-400/20">
              <Icon className="h-5 w-5" aria-hidden="true" />
            </span>
            <div>
              <div className="text-2xl font-bold text-white tabular-nums">{value}</div>
              <div className="text-sm text-slate-400">{label}</div>
            </div>
          </Card>
        ))}
      </div>

      {/* Data Availability Table */}
      <DataAvailabilityTable stats={stats} sortedYears={sortedYears} />

      {/* Database Tech Stats Section */}
      <div className="mx-auto mt-16 max-w-3xl overflow-hidden rounded-2xl bg-white/[0.04] ring-1 ring-inset ring-white/10 backdrop-blur-md">
        <div className="flex items-center gap-3 border-b border-white/10 p-6">
          <CircleStackIcon className="h-6 w-6 text-indigo-300" aria-hidden="true" />
          <div>
            <h2 className="text-xl font-semibold text-white">Dados Técnicos do Banco de Dados</h2>
            <p className="mt-1 text-sm text-slate-400">Informações sobre tabelas, views materializadas e índices</p>
          </div>
        </div>
        {dbLoading ? (
          <LoadingBlock colorClassName="border-indigo-400" text="Carregando dados do banco de dados..." />
        ) : dbError ? (
          <div className="p-8 text-rose-300">{dbError}</div>
        ) : dbStats ? (
          <div className="space-y-8 p-6">
            <DbStatsTable title="Tabelas" items={dbStats.tables} showTotal />
            <DbStatsTable title="Views Materializadas" items={dbStats.materializedViews} />
            <DbIndexesStats amount={dbStats.indexes.amount} size={dbStats.indexes.size} />
          </div>
        ) : null}
      </div>
    </div>
  );
};

export default DataStatsPage;
