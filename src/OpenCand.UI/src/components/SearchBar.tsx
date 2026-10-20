import React, { useState, useEffect, useCallback, useRef } from 'react';
import { MagnifyingGlassIcon, XMarkIcon, MapPinIcon } from '@heroicons/react/24/solid';
import { useNavigate } from 'react-router-dom';
import { openCandApi, type Candidate, ApiError, isRequestCancelled } from '../api';
import { formatDateToDDMMYYYY, getInitials, maskCpf } from '../utils/utils';
import Spinner from '../shared/Spinner';

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
  const abortRef = useRef<AbortController | null>(null);
  const resultsRef = useRef<HTMLDivElement>(null);
  const resultsContainerRef = useRef<HTMLDivElement>(null);

  // Cancels the in-flight search (if any) so its response can never overwrite a newer one
  const cancelPendingSearch = useCallback(() => {
    abortRef.current?.abort();
    abortRef.current = null;
  }, []);

  // Debounced search function
  const performSearch = useCallback(async (query: string) => {
    cancelPendingSearch();

    if (query.trim().length < 3) {
      setSearchResults([]);
      setShowResults(false);
      setError(null);
      setIsLoading(false);
      return;
    }

    const controller = new AbortController();
    abortRef.current = controller;

    setIsLoading(true);
    setError(null);

    try {
      const result = await openCandApi.searchCandidates(query.trim(), controller.signal);
      setSearchResults(result.candidatos);
      setShowResults(true);
    } catch (err) {
      if (controller.signal.aborted || isRequestCancelled(err)) return;
      if (err instanceof ApiError) {
        setError(`Erro na busca: ${err.message}`);
      } else {
        setError('Erro inesperado na busca');
      }
      setSearchResults([]);
      setShowResults(false);
    } finally {
      if (!controller.signal.aborted) {
        setIsLoading(false);
      }
    }
  }, [cancelPendingSearch]);

  // Handle input change with debouncing
  const handleInputChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const query = e.target.value;
    setSearchQuery(query);

    // Clear previous timeout and drop any request that is now outdated
    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current);
    }
    cancelPendingSearch();
    setIsLoading(false);

    // Set new timeout for debounced search
    searchTimeoutRef.current = window.setTimeout(() => {
      performSearch(query);
    }, 300); // 300ms delay
  }, [performSearch, cancelPendingSearch]);

  // Handle candidate selection
  const handleCandidateSelect = useCallback((candidate: Candidate) => {
    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current);
    }
    cancelPendingSearch();
    setIsLoading(false);
    navigate(`/candidato/${candidate.idCandidato}`);
    setShowResults(false);
    setSearchQuery('');
  }, [navigate, cancelPendingSearch]);

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
    const desc: string[] = [];

    if (candidate.apelido) desc.push(`"${candidate.apelido}"`);
    if (candidate.cpf) desc.push(`CPF ${maskCpf(candidate.cpf)}`);
    if (desc.length === 0 && candidate.dataNascimento) {
      desc.push(`Nascimento ${formatDateToDDMMYYYY(candidate.dataNascimento)}`);
    }

    return desc.join(' · ');
  };

  // Clear timeout and abort the in-flight request on unmount
  useEffect(() => {
    return () => {
      if (searchTimeoutRef.current) {
        clearTimeout(searchTimeoutRef.current);
      }
      abortRef.current?.abort();
    };
  }, []);

  const clearSearch = () => {
    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current);
    }
    cancelPendingSearch();
    setIsLoading(false);
    setSearchQuery('');
    setSearchResults([]);
    setShowResults(false);
    setError(null);
  };

  const trimmedLength = searchQuery.trim().length;
  const showDropdown = showResults && (searchResults.length > 0 || !!error);
  const showNoResults = showResults && searchResults.length === 0 && !error && !isLoading && trimmedLength >= 3;
  const listboxId = 'search-results';

  return (
    <div className={`relative mx-auto w-full max-w-2xl text-left ${className}`} ref={resultsRef}>
      <form onSubmit={handleSubmit} role="search">
        <div
          className={`flex items-center rounded-2xl bg-white/[0.08] p-1.5 shadow-2xl shadow-black/30 ring-1 ring-inset backdrop-blur-xl transition-all duration-300 ease-out
            ${isFocused ? 'bg-white/[0.12] ring-indigo-300/40 shadow-indigo-950/50' :'ring-white/15 hover:bg-white/[0.1] hover:ring-white/25'}
          `}
        >
          <MagnifyingGlassIcon
            className={`ml-3 h-5 w-5 shrink-0 transition-colors duration-300 ${isFocused ? 'text-indigo-300' : 'text-slate-400'}`}
            aria-hidden="true"
          />
          <input
            value={searchQuery}
            onChange={handleInputChange}
            onFocus={() => {
              setIsFocused(true);
              if (searchResults.length > 0) setShowResults(true);
            }}
            onBlur={() => setIsFocused(false)}
            onKeyDown={handleKeyDown}
            placeholder="Pesquise um candidato pelo nome..."
            aria-label="Pesquisar candidatos"
            role="combobox"
            aria-expanded={showDropdown}
            aria-controls={listboxId}
            aria-autocomplete="list"
            aria-activedescendant={activeIndex > -1 ? `search-result-${activeIndex}` : undefined}
            className="min-w-0 flex-grow bg-transparent p-3 text-base text-white placeholder-slate-400 focus:outline-none focus-visible:outline-none"
            autoComplete="off"
            spellCheck={false}
          />
          {isLoading && (
            <Spinner size="sm" colorClassName="border-indigo-300" className="mr-3" label="Buscando" />
          )}
          {searchQuery && !isLoading && (
            <button
              type="button"
              onClick={clearSearch}
              className="mr-1 rounded-full p-2 text-slate-400 transition-colors hover:bg-white/10 hover:text-white"
              aria-label="Limpar busca"
            >
              <XMarkIcon className="h-4 w-4" />
            </button>
          )}
          <button
            type="submit"
            className="hidden rounded-xl bg-indigo-500 px-5 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-indigo-400 disabled:opacity-50 sm:block"
            disabled={searchResults.length === 0}
          >
            Buscar
          </button>
        </div>
      </form>

      {isFocused && trimmedLength > 0 && trimmedLength < 3 && (
        <p className="mt-2 pl-4 text-xs text-slate-400">Digite pelo menos 3 caracteres para buscar.</p>
      )}

      {/* Search Results Dropdown */}
      {showDropdown && (
        <div className="absolute left-0 right-0 top-full z-50 mt-2 overflow-hidden rounded-2xl bg-white shadow-2xl shadow-black/40 ring-1 ring-slate-900/10 animate-fadeIn">
          {error ? (
            <div className="p-4 text-center text-sm text-rose-600">
              {error}
            </div>
          ) : (
            <>
              <div ref={resultsContainerRef} id={listboxId} role="listbox" aria-label="Resultados da busca" className="max-h-96 overflow-y-auto p-1.5 custom-scrollbar">
                {searchResults.map((candidate, index) => {
                  const description = getCandidateDescription(candidate);
                  const isActive = index === activeIndex;
                  return (
                    <button
                      key={candidate.idCandidato}
                      id={`search-result-${index}`}
                      role="option"
                      aria-selected={isActive}
                      type="button"
                      onClick={() => handleCandidateSelect(candidate)}
                      onMouseEnter={() => setActiveIndex(index)}
                      className={`flex w-full items-center gap-3 rounded-xl p-3 text-left transition-colors focus:outline-none ${isActive ? 'bg-indigo-50' : 'hover:bg-slate-50'}`}
                    >
                      <span
                        className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-full text-sm font-semibold ${isActive ? 'bg-indigo-600 text-white' : 'bg-slate-100 text-slate-600'}`}
                        aria-hidden="true"
                      >
                        {getInitials(candidate.nome)}
                      </span>
                      <span className="min-w-0 flex-1">
                        <span className="block truncate font-semibold text-slate-900">{candidate.nome}</span>
                        {description && (
                          <span className="block truncate text-sm text-slate-500">{description}</span>
                        )}
                      </span>
                      {candidate.localidade && (
                        <span className="hidden shrink-0 items-center gap-1 rounded-full bg-slate-100 px-2.5 py-1 text-xs text-slate-600 sm:inline-flex">
                          <MapPinIcon className="h-3 w-3" aria-hidden="true" />
                          {candidate.localidade}
                        </span>
                      )}
                    </button>
                  );
                })}
              </div>

              {searchResults.length === 10 && (
                <div className="border-t border-slate-100 bg-slate-50 p-3 text-center text-xs text-slate-500">
                  Mostrando apenas os primeiros <strong>10 resultados</strong> — refine a busca para encontrar outros
                </div>
              )}
            </>
          )}
        </div>
      )}

      {/* No results message */}
      {showNoResults && (
        <div className="absolute left-0 right-0 top-full z-50 mt-2 overflow-hidden rounded-2xl bg-white shadow-2xl shadow-black/40 ring-1 ring-slate-900/10 animate-fadeIn">
          <div className="p-5 text-center text-sm text-slate-600">
            Nenhum candidato encontrado para <strong className="text-slate-900">"{searchQuery}"</strong>
          </div>
        </div>
      )}
    </div>
  );
};

export default SearchBar;
