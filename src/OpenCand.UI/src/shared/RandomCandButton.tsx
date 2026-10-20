import React from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowPathIcon } from '@heroicons/react/24/outline';
import Button from './Button';
import { openCandApi } from '../api/openCandApi';

interface RandomCandButtonProps {
  className?: string;
  hasAnimation?: boolean;
  size?: 'sm' | 'md' | 'lg';
}

const RandomCandButton: React.FC<RandomCandButtonProps> = ({
  className = '',
  hasAnimation = true,
  size = 'md',
}) => {
  const navigate = useNavigate();
  const [loading, setLoading] = React.useState(false);

  const handleRandomCandidate = async () => {
    setLoading(true);
    try {
      const randomCandidate = await openCandApi.getRandomCandidate();
      if (randomCandidate?.idCandidato) {
        navigate(`/candidato/${randomCandidate.idCandidato}`);
      }
    } catch (error) {
      console.error('Erro ao buscar candidato aleatório:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Button
      onClick={handleRandomCandidate}
      className={className}
      hasAnimation={hasAnimation}
      size={size}
      disabled={loading}
    >
      <ArrowPathIcon className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} aria-hidden="true" />
      Candidato aleatório
    </Button>
  );
};

export default RandomCandButton;
