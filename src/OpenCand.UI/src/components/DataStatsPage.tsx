import React, { useState, useEffect } from 'react';
import { FaCloudDownloadAlt, FaGoogleDrive } from 'react-icons/fa';
import { openCandApi } from '../api';
import type { OpenCandDataAvailabilityStats, OpenCandDatabaseStats } from '../api/apiModels';
import Card from '../shared/Card';
import ErrorPage from './ErrorPage';
import DataAvailabilityTable from './DataStats/DataAvailabilityTable';
import DbStatsTable from './DataStats/DbStatsTable';
import GradientButton from '../shared/GradientButton';
import DbIndexesStats from './DataStats/DbIndexesStats';

const DataStatsPage: React.FC = () => {
  const [stats, setStats] = useState<OpenCandDataAvailabilityStats | null>(null);
  const [dbStats, setDbStats] = useState<OpenCandDatabaseStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [dbLoading, setDbLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [dbError, setDbError] = useState<string | null>(null);

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
    return (
      <div className="flex items-center justify-center min-h-[40vh]">
        <div className="backdrop-blur-md bg-white/90 rounded-2xl shadow-xl p-8 max-w-md w-full border border-white/30 flex flex-col items-center">
          <div className="animate-spin rounded-full h-10 w-10 border-4 border-indigo-600 border-t-transparent mb"></div>
          <p className="text-base text-gray-700">Carregando dados...</p>
        </div>
      </div>
    );
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
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <p className="text-lg text-gray-700">Nenhum dado disponível</p>
      </div>
    );
  }

  // Get all unique years from all data types
  const allYears = new Set<number>();
  Object.values(stats).forEach((yearArray: number[]) => {
    yearArray.forEach((year: number) => allYears.add(year));
  });
  const sortedYears = Array.from(allYears).sort((a, b) => b - a);

  return (
    <div className="min-h-screen py-20 px-4 hover:cursor-default">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="text-center mb-12 animate-slide-in-left">
          <h1 className="text-4xl md:text-6xl font-bold mb-4 text-white">
            Disponibilidade de Dados
          </h1>
          <p className="text-xl text-gray-400">
            Visualize a disponibilidade dos dados por ano em nossa base
          </p>
          <div className="w-32 h-1 bg-gradient-to-r from-indigo-400 to-purple-400 mx-auto mt-6 rounded-full"></div>
        </div>

        {/* Download DB Dump Section */}
        <div className="flex justify-center mb-10 animate-slide-in-left backdrop-blur-xs bg-gray-800/10 rounded-xl shadow-lg hover:shadow-xl transform transition-all duration-200 overflow-hidden ring-1 ring-gray-700 hover:ring-indigo-300 px-8 py-6 flex flex-col md:flex-row items-center gap-4 max-w-xl-auto">
          <div className="flex items-center gap-3 mb-2 md:mb-0">
            <FaCloudDownloadAlt className="text-3xl text-green-400" />
            <span className="text-lg font-semibold text-white">Download do Dump do Banco de Dados</span>
          </div>
          <GradientButton
            href="https://drive.google.com/file/d/1cfMItrsAdv8y8YUNp04D33s6pYrRbmDn/view?usp=sharing"
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center gap-2 py-2 px-5"
          >
            <FaGoogleDrive className="text-xl" />
            Google Drive
          </GradientButton>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
          <Card hasAnimation={true} className="bg-gray-800/10 rounded-xl">
            <div className="text-center">
              <div className="text-3xl mb-2">📊</div>
              <div className="text-2xl font-bold text-white">6</div>
              <div className="text-gray-400">Tipos de Dados</div>
            </div>
          </Card>
          <Card hasAnimation={true} className="bg-gray-800/10 rounded-xl">
            <div className="text-center">
              <div className="text-3xl mb-2">📅</div>
              <div className="text-2xl font-bold text-white">{sortedYears.length}</div>
              <div className="text-gray-400">Anos Disponíveis</div>
            </div>
          </Card>
          <Card hasAnimation={true} className="bg-gray-800/10 rounded-xl">
            <div className="text-center">
              <div className="text-3xl mb-2">🗓️</div>
              <div className="text-2xl font-bold text-white">
                {sortedYears.length > 0 ? `${Math.min(...sortedYears)} - ${Math.max(...sortedYears)}` : 'N/A'}
              </div>
              <div className="text-gray-400">Período</div>
            </div>
          </Card>
        </div>

        {/* Data Availability Table */}
        <DataAvailabilityTable stats={stats} sortedYears={sortedYears} />
      </div>

      {/* Database Tech Stats Section */}
      <div className="mt-20 flex justify-center">
        <div className="bg-gray-800/10 backdrop-blur-xs rounded-xl shadow-lg hover:shadow-xl transform hover:scale-[1.005] transition-all duration-200 overflow-hidden ring-1 ring-gray-700 hover:ring-indigo-300 mb-12 w-full max-w-2xl">
          <div className="p-6 border-b border-gray-700/30 bg-gray-800/10">
            <h2 className="text-2xl font-bold text-white">Dados Técnicas do Banco de Dados</h2>
            <p className="text-gray-400 mt-2">Informações sobre tabelas, views materializadas e índices</p>
          </div>
          {dbLoading ? (
            <div className="flex items-center justify-center p-8">
              <div className="animate-spin rounded-full h-8 w-8 border-4 border-indigo-600 border-t-transparent mr-4"></div>
              <span className="text-gray-300">Carregando dados do banco de dados...</span>
            </div>
          ) : dbError ? (
            <div className="p-8 text-red-400">{dbError}</div>
          ) : dbStats ? (
            <div className="p-6 space-y-10">
              <DbStatsTable title="Tabelas" items={dbStats.tables} showTotal />
              <DbStatsTable title="Views Materializadas" items={dbStats.materializedViews} />
              <DbIndexesStats amount={dbStats.indexes.amount} size={dbStats.indexes.size} />
            </div>
          ) : null}
        </div>
      </div>
    </div>
  );
};

export default DataStatsPage;
