import React, { useState, useEffect, useCallback, useRef } from 'react';
import { MagnifyingGlassIcon, XMarkIcon, MapPinIcon } from '@heroicons/react/24/solid';
import { useNavigate } from 'react-router-dom';
import { openCandApi, type Candidate, ApiError } from '../api';
import { formatDateToDDMMYYYY, maskCpf } from '../utils/utils';

interface SearchBarProps {
  className?: string;
}

const SearchBar: React.FC<SearchBarProps> = ({ className = '' }) => {
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<Candidate[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [showResults, setShowResults] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isFocused, setIsFocused] = useState(false);
  const [activeIndex, setActiveIndex] = useState(-1);
  const navigate = useNavigate();
  const searchTimeoutRef = useRef<number | null>(null);
  const resultsRef = useRef<HTMLDivElement>(null);
  const resultsContainerRef = useRef<HTMLDivElement>(null);

  // Debounced search function
  const performSearch = useCallback(async (query: string) => {
    if (query.trim().length < 2) {
      setSearchResults([]);
      setShowResults(false);
      setError(null);
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const result = await openCandApi.searchCandidates(query.trim());
      setSearchResults(result.candidatos); // Limit to 8 results
      setShowResults(true);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(`Erro na busca: ${err.message}`);
      } else {
        setError('Erro inesperado na busca');
      }
      setSearchResults([]);
      setShowResults(false);
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Handle input change with debouncing
  const handleInputChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const query = e.target.value;
    setSearchQuery(query);

    // Clear previous timeout
    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current);
    }

    // Set new timeout for debounced search
    searchTimeoutRef.current = window.setTimeout(() => {
      performSearch(query);
    }, 300); // 300ms delay
  }, [performSearch]);

  // Handle candidate selection
  const handleCandidateSelect = useCallback((candidate: Candidate) => {
    navigate(`/candidato/${candidate.idCandidato}`);
    setShowResults(false);
    setSearchQuery('');
  }, [navigate]);

  // Handle form submission
  const handleSubmit = useCallback((e: React.FormEvent) => {
    e.preventDefault();
    if (activeIndex > -1 && searchResults[activeIndex]) {
      handleCandidateSelect(searchResults[activeIndex]);
    } else if (searchResults.length > 0) {
      handleCandidateSelect(searchResults[0]);
    }
  }, [searchResults, handleCandidateSelect, activeIndex]);

  // Close results when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (resultsRef.current && !resultsRef.current.contains(event.target as Node)) {
        setShowResults(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  // Reset active index when search results change
  useEffect(() => {
    setActiveIndex(-1);
  }, [searchResults]);

  // Scroll active item into view
  useEffect(() => {
    if (activeIndex < 0 || !resultsContainerRef.current) return;

    const resultsContainer = resultsContainerRef.current;
    const activeButton = resultsContainer.children[activeIndex] as HTMLElement;

    if (activeButton) {
      activeButton.scrollIntoView({
        behavior: 'smooth',
        block: 'nearest',
      });
    }
  }, [activeIndex]);

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Escape') {
      setShowResults(false);
      return;
    }

    if (showResults && searchResults.length > 0) {
      if (e.key === 'ArrowDown') {
        e.preventDefault();
        setActiveIndex(prevIndex => (prevIndex + 1) % searchResults.length);
      } else if (e.key === 'ArrowUp') {
        e.preventDefault();
        setActiveIndex(prevIndex => (prevIndex - 1 + searchResults.length) % searchResults.length);
      }
    }
  };

  const getCandidateDescription = (candidate: Candidate) => {
    const desc = [''];

    if (candidate.cpf) desc.push(`CPF: ${maskCpf(candidate.cpf)}`);
    if (candidate.apelido) desc.push(`"${candidate.apelido}"`);
    if (desc.length == 0)
      if (candidate.dataNascimento) desc.push(`${formatDateToDDMMYYYY(candidate.dataNascimento)}`);

    return desc.filter(Boolean).join(' | ');
  }

  // Clear timeout on unmount
  useEffect(() => {
    return () => {
      if (searchTimeoutRef.current) {
        clearTimeout(searchTimeoutRef.current);
      }
    };
  }, []);

  return (
    <div className={`w-full max-w-2xl mx-auto relative ${className}`} ref={resultsRef}>
      <form onSubmit={handleSubmit}>
        <div className={`flex items-center bg-white/10 backdrop-blur-sm rounded-full shadow-xl p-2 
          transition-all duration-300 ease-in-out 
          ${isFocused ? 'bg-white/20 scale-[1.01] shadow-2xl ring-2 ring-white/20' : 'hover:bg-white/15 hover:scale-[1.01]'}
        `}>
          <MagnifyingGlassIcon className={`h-6 w-6 transition-all duration-300 ${isFocused ? 'text-white' : 'text-gray-400'} ml-3`} />
          <input
            value={searchQuery}
            onChange={handleInputChange}
            onFocus={() => setIsFocused(true)}
            onBlur={() => setIsFocused(false)}
            onKeyDown={handleKeyDown}
            placeholder="Pesquisar candidatos..." 
            className="flex-grow bg-transparent text-white placeholder-gray-400 p-3 focus:outline-none"
            autoComplete="off"
          />
          {isLoading && (
            <div className="mr-3">
              <div className="animate-spin rounded-full h-5 w-5 border-2 border-white border-t-transparent"></div>
            </div>
          )}
          {searchQuery && !isLoading && (
            <button
              type="button"
              onClick={() => {
                setSearchQuery('');
                setSearchResults([]);
                setShowResults(false);
                setError(null);
              }}
              className="mr-3 p-1 hover:bg-white/20 rounded-full transition-colors"
            >
              <XMarkIcon className="h-4 w-4 text-gray-400" />
            </button>
          )}
        </div>
      </form>

      {/* Search Results Dropdown */}
      {showResults && (searchResults.length > 0 || error) && (
        <div className="absolute top-full left-0 right-0 mt-2 bg-white rounded-xl shadow-2xl border border-gray-200 overflow-hidden z-50 animate-fadeIn">
          {error ? (
            <div className="p-4 text-red-600 text-center bg-white">
              {error}
            </div>
          ) : (
            <div ref={resultsContainerRef} className="max-h-80 overflow-y-auto custom-scrollbar">
              {searchResults.map((candidate, index) => (
                <button
                  key={candidate.idCandidato}
                  onClick={() => handleCandidateSelect(candidate)}
                  onMouseEnter={() => setActiveIndex(index)}
                  className={`w-full p-4 text-left hover:bg-gray-100 transition-colors border-b border-gray-100 last:border-b-0 focus:outline-none focus:bg-gray-100 ${index === activeIndex ? 'bg-gray-100' : ''}`}
                >
                  <div className="text-black font-semibold text-base">{candidate.nome}</div>
                  <div className="text-gray-600 text-sm mt-1">
                    {getCandidateDescription(candidate)}
                  </div>
                  {candidate.localidade && (
                    <div className="flex items-center text-gray-500 text-xs mt-1">
                      <MapPinIcon className="h-3 w-3 mr-1" />
                      {candidate.localidade}
                    </div>
                  )}
                </button>
              ))}
              
              {searchResults.length === 10 && (
                <div className="p-3 text-center text-gray-500 text-sm bg-gray-50">
                  Mostrando apenas os primeiros <strong>10 resultados</strong>
                </div>
              )}
            </div>
          )}
        </div>
      )}

      {/* No results message */}
      {showResults && searchResults.length === 0 && !error && !isLoading && searchQuery.length >= 2 && (
        <div className="absolute top-full left-0 right-0 mt-2 bg-white rounded-xl shadow-2xl border border-gray-200 overflow-hidden z-50 animate-fadeIn">
          <div className="p-4 text-gray-600 text-center">
            Nenhum candidato encontrado para "{searchQuery}"
          </div>
        </div>
      )}
    </div>
  );
};

export default SearchBar;
