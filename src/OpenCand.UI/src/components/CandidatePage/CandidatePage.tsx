import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeftIcon } from '@heroicons/react/24/outline';
import { openCandApi, type CandidateDetails, type CandidateAssets, type CandidateRedesSociais, ApiError, isRequestCancelled } from '../../api';
import ElectionsComponent from './ElectionsComponent';
import AssetsComponent from './AssetsComponent';
import BasicCandidateInfoComponent from './BasicCandidateInfoComponent';
import SocialMediaComponent from './SocialMediaComponent';
import IncomeExpenseComponent from './IncomeExpenseComponent';
import Button from '../../shared/Button';
import RandomCandButton from '../../shared/RandomCandButton';
import ErrorPage from '../ErrorPage';
import CandidateHeader from './CandidateHeader';
import { usePageTitle } from '../../hooks/usePageTitle';

const CandidatePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [candidateDetails, setCandidateDetails] = useState<CandidateDetails | null>(null);
  const [candidateAssets, setCandidateAssets] = useState<CandidateAssets | null>(null);
  const [candidateRedesSociais, setCandidateRedesSociais] = useState<CandidateRedesSociais | null>(null);
  const [isLoadingDetails, setIsLoadingDetails] = useState(true);
  const [isLoadingAssets, setIsLoadingAssets] = useState(true);
  const [isLoadingRedesSociais, setIsLoadingRedesSociais] = useState(true);
  const [error, setError] = useState<string | null>(null);

  usePageTitle(candidateDetails?.nome ?? 'Candidato');

  useEffect(() => {
    if (!id) {
      navigate('/');
      return;
    }

    // Everything below belongs to this id: the cleanup aborts all of its requests,
    // so a slow response for a previous candidate can never overwrite the current one.
    const controller = new AbortController();
    const { signal } = controller;

    // Reset ALL state (data, loading flags and error) when the route id changes
    setCandidateDetails(null);
    setCandidateAssets(null);
    setCandidateRedesSociais(null);
    setIsLoadingDetails(true);
    setIsLoadingAssets(true);
    setIsLoadingRedesSociais(true);
    setError(null);

    // Fetch candidate details
    const fetchCandidateDetails = async () => {
      try {
        const details = await openCandApi.getCandidateById(id, signal);
        if (signal.aborted) return;
        setCandidateDetails(details);
      } catch (err) {
        if (signal.aborted || isRequestCancelled(err)) return;
        console.error('Error fetching candidate details:', err);
        if (err instanceof ApiError) {
          setError(`Erro ao carregar dados do candidato: ${err.message}`);
        } else {
          setError('Erro inesperado ao carregar dados do candidato');
        }
      } finally {
        if (!signal.aborted) setIsLoadingDetails(false);
      }
    };

    // Secondary sections might not be available for all candidates, so a failure is not surfaced as page error
    const fetchOptional = async <T,>(
      label: string,
      request: () => Promise<T>,
      onData: (data: T) => void,
      onDone: () => void
    ) => {
      try {
        const data = await request();
        if (signal.aborted) return;
        onData(data);
      } catch (err) {
        if (signal.aborted || isRequestCancelled(err)) return;
        console.error(`Error fetching candidate ${label}:`, err);
      } finally {
        if (!signal.aborted) onDone();
      }
    };

    fetchCandidateDetails();
    fetchOptional('assets', () => openCandApi.getCandidateAssets(id, signal), setCandidateAssets, () => setIsLoadingAssets(false));
    fetchOptional('social media', () => openCandApi.getCandidateRedesSociais(id, signal), setCandidateRedesSociais, () => setIsLoadingRedesSociais(false));

    return () => controller.abort();
  }, [id, navigate]);

  if (error) {
    return (
      <ErrorPage
        title="Erro"
        description={error}
        helperText="Tente novamente ou volte para a página inicial."
      />
    );
  }

  return (
    <main className="mx-auto w-full max-w-7xl flex-grow px-4 pb-8 pt-6 sm:px-6">
      {/* Actions */}
      <div className="mb-5 flex flex-wrap items-center gap-3">
        <Button to="/" size="sm">
          <ArrowLeftIcon className="h-4 w-4" aria-hidden="true" />
          Voltar à busca
        </Button>
        <RandomCandButton size="sm" />
      </div>

      <div className="mb-6">
        <CandidateHeader key={id} candidateDetails={candidateDetails} isLoading={isLoadingDetails} />
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        {/* Left Column - Basic Information and Social Media */}
        <div className="space-y-6 lg:col-span-1">
          <BasicCandidateInfoComponent
            candidateDetails={candidateDetails}
            isLoading={isLoadingDetails}
          />

          <SocialMediaComponent
            redesSociais={candidateRedesSociais?.redesSociais || null}
            isLoading={isLoadingRedesSociais}
          />
        </div>

        {/* Right Column - Elections, Assets and Income/Expenses */}
        <div className="min-w-0 space-y-6 lg:col-span-2">
          <ElectionsComponent
            elections={candidateDetails?.eleicoes || null}
            isLoading={isLoadingDetails}
          />

          <AssetsComponent
            assets={candidateAssets?.bens || null}
            isLoading={isLoadingAssets}
          />

          {id && <IncomeExpenseComponent key={id} idCandidato={id} />}
        </div>
      </div>
    </main>
  );
};

export default CandidatePage;
