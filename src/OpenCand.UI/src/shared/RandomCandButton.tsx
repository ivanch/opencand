import React from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowPathIcon } from '@heroicons/react/24/outline';
import Button from './Button';
import { openCandApi } from '../api/openCandApi';

interface RandomCandButtonProps {
  className?: string;
  hasAnimation?: boolean;
}


const RandomCandButton: React.FC<RandomCandButtonProps> = ({ 
  className = '', 
  hasAnimation = false 
}) => {
  const navigate = useNavigate();
  const [loading, setLoading] = React.useState(false);

  const handleRandomCandidate = async () => {
    setLoading(true);
    try {
      const randomCandidate = await openCandApi.getRandomCandidate();
      navigate(`/candidato/${randomCandidate.idCandidato}`);
    } catch (error) {
      console.error('Erro ao buscar candidato aleatório:', error);
      // You might want to show a toast notification or error message to the user
    } finally {
      setLoading(false);
    }
  };

  return (
    <Button 
      onClick={handleRandomCandidate} 
      className={`flex items-center relative overflow-hidden ${className}`}
      hasAnimation={hasAnimation}
      disabled={loading}
    >
      <ArrowPathIcon 
        className={`h-4 w-4 transition-transform duration-500 ${loading ? 'animate-spin' : 'mr-2'}`}
        style={{ zIndex: 2 }}
      />
      <span
        className={`inline-block transition-all duration-300 origin-left whitespace-nowrap ${loading ? 'scale-x-0 max-w-0' : 'scale-x-100 max-w-xs'}`}
        style={{ zIndex: 1, transitionProperty: 'transform, max-width' }}
      >
        Candidato aleatório
      </span>
    </Button>
  );
};

export default RandomCandButton;
