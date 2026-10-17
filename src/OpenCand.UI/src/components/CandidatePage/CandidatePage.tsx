import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { ArrowLeftIcon } from '@heroicons/react/24/outline';
import { openCandApi, type CandidateDetails, type CandidateAssets, type CandidateRedesSociais, type CandidateExpenses, type CandidateIncome, ApiError } from '../../api';
import ElectionsComponent from './ElectionsComponent';
import AssetsComponent from './AssetsComponent';
import BasicCandidateInfoComponent from './BasicCandidateInfoComponent';
import SocialMediaComponent from './SocialMediaComponent';
import IncomeExpenseComponent from './IncomeExpenseComponent';
import Button from '../../shared/Button';
import RandomCandButton from '../../shared/RandomCandButton';
import ErrorPage from '../ErrorPage';

const CandidatePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [candidateDetails, setCandidateDetails] = useState<CandidateDetails | null>(null);
  const [candidateAssets, setCandidateAssets] = useState<CandidateAssets | null>(null);
  const [candidateRedesSociais, setCandidateRedesSociais] = useState<CandidateRedesSociais | null>(null);
  const [candidateExpenses, setCandidateExpenses] = useState<CandidateExpenses | null>(null);
  const [candidateIncome, setCandidateIncome] = useState<CandidateIncome | null>(null);
  const [isLoadingDetails, setIsLoadingDetails] = useState(true);
  const [isLoadingAssets, setIsLoadingAssets] = useState(true);
  const [isLoadingRedesSociais, setIsLoadingRedesSociais] = useState(true);
  const [isLoadingExpenses, setIsLoadingExpenses] = useState(true);
  const [isLoadingIncome, setIsLoadingIncome] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) {
      navigate('/');
      return;
    }

    // Fetch candidate details
    const fetchCandidateDetails = async () => {
      try {
        setIsLoadingDetails(true);
        const details = await openCandApi.getCandidateById(id);
        setCandidateDetails(details);
      } catch (err) {
        console.error('Error fetching candidate details:', err);
        if (err instanceof ApiError) {
          setError(`Erro ao carregar dados do candidato: ${err.message}`);
        } else {
          setError('Erro inesperado ao carregar dados do candidato');
        }
      } finally {
        setIsLoadingDetails(false);
      }
    };

    // Fetch candidate assets
    const fetchCandidateAssets = async () => {
      try {
        setIsLoadingAssets(true);
        const assets = await openCandApi.getCandidateAssets(id);
        setCandidateAssets(assets);
      } catch (err) {
        console.error('Error fetching candidate assets:', err);
        // Assets might not be available for all candidates, so we don't set error here
      } finally {
        setIsLoadingAssets(false);
      }
    };

    // Fetch candidate social media
    const fetchCandidateRedesSociais = async () => {
      try {
        setIsLoadingRedesSociais(true);
        const redesSociais = await openCandApi.getCandidateRedesSociais(id);
        setCandidateRedesSociais(redesSociais);
      } catch (err) {
        console.error('Error fetching candidate social media:', err);
        // Social media might not be available for all candidates, so we don't set error here
      } finally {
        setIsLoadingRedesSociais(false);
      }
    };

    // Fetch candidate expenses
    const fetchCandidateExpenses = async () => {
      try {
        setIsLoadingExpenses(true);
        const expenses = await openCandApi.getCandidateDepesas(id);
        setCandidateExpenses(expenses);
      } catch (err) {
        console.error('Error fetching candidate expenses:', err);
        // Expenses might not be available for all candidates, so we don't set error here
      } finally {
        setIsLoadingExpenses(false);
      }
    };

    // Fetch candidate income
    const fetchCandidateIncome = async () => {
      try {
        setIsLoadingIncome(true);
        const income = await openCandApi.getCandidateReceitas(id);
        setCandidateIncome(income);
      } catch (err) {
        console.error('Error fetching candidate income:', err);
        // Income might not be available for all candidates, so we don't set error here
      } finally {
        setIsLoadingIncome(false);
      }
    };

    fetchCandidateDetails();
    fetchCandidateAssets();
    fetchCandidateRedesSociais();
    fetchCandidateExpenses();
    fetchCandidateIncome();
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
    <main className="flex-grow p-6 max-w-7xl mx-auto">
      {/* Header with back button */}
      <div className="mb-6">
        <div className="flex items-center gap-4 mb-4">
          <Button
            onClick={() => navigate('/')}
            className="flex items-center text-white"
            hasAnimation={false}
          >
            <ArrowLeftIcon className="h-5 w-5 mr-2" />
            Voltar à busca
          </Button>
          
          <RandomCandButton 
            className="flex items-center text-white"
            hasAnimation={false}
          />
        </div>
        
        {candidateDetails && (
          <h1 className="text-3xl font-bold text-white">{candidateDetails.nome}</h1>
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 h-full">
        {/* Left Column - Basic Information and Social Media */}
        <div className="lg:col-span-1 space-y-6">
          {/* Basic Information Panel */}
          <BasicCandidateInfoComponent 
            candidateDetails={candidateDetails}
            isLoading={isLoadingDetails}
          />

          {/* Social Media Panel */}
          <SocialMediaComponent 
            redesSociais={candidateRedesSociais?.redesSociais || null}
            isLoading={isLoadingRedesSociais}
          />
        </div>

        {/* Right Column - Elections and Assets */}
        <div className="lg:col-span-2 space-y-6">
          {/* Elections Panel */}
          <ElectionsComponent 
            elections={candidateDetails?.eleicoes || null}
            isLoading={isLoadingDetails}
          />

          {/* Assets Panel */}
          <AssetsComponent 
            assets={candidateAssets?.bens || null}
            isLoading={isLoadingAssets}
          />
          
          {/* Income and Expenses Panel */}
          <IncomeExpenseComponent 
            expenses={candidateExpenses}
            income={candidateIncome}
            isLoadingExpenses={isLoadingExpenses}
            isLoadingIncome={isLoadingIncome}
          />
        </div>
      </div>
    </main>
  );
};

export default CandidatePage;
