import React, { useMemo, useState } from 'react';
import { MapPinIcon } from '@heroicons/react/24/outline';
import { type CandidateDetails } from '../../api';
import { getInitials } from '../../utils/utils';
import { getResultadoBadgeClass } from './electionResult';

interface CandidateHeaderProps {
  candidateDetails: CandidateDetails | null;
  isLoading: boolean;
}

/** Profile header of the candidate page: photo, name and the most recent candidacy. */
const CandidateHeader: React.FC<CandidateHeaderProps> = ({ candidateDetails, isLoading }) => {
  const [photoFailed, setPhotoFailed] = useState(false);

  const latestElection = useMemo(() => {
    const elections = candidateDetails?.eleicoes ?? [];
    return elections.length > 0 ? [...elections].sort((a, b) => b.ano - a.ano)[0] : null;
  }, [candidateDetails]);

  if (isLoading || !candidateDetails) {
    return (
      <div className="flex items-center gap-5 rounded-2xl bg-white/[0.04] p-5 ring-1 ring-inset ring-white/10 md:p-6" aria-hidden="true">
        <div className="skeleton h-20 w-20 shrink-0 rounded-full md:h-24 md:w-24" />
        <div className="flex-1 space-y-3">
          <div className="skeleton h-7 w-2/3 max-w-sm" />
          <div className="skeleton h-5 w-1/2 max-w-xs" />
        </div>
      </div>
    );
  }

  const apelido = candidateDetails.apelido
    || [...(candidateDetails.candidatoExt ?? [])].sort((a, b) => b.ano - a.ano).find(ext => ext.apelido)?.apelido;
  const showPhoto = !!candidateDetails.fotoUrl && !photoFailed;

  return (
    <div className="relative overflow-hidden rounded-2xl bg-gradient-to-br from-indigo-600/25 via-white/[0.04] to-violet-600/15 p-5 ring-1 ring-inset ring-white/10 backdrop-blur-md md:p-6">
      <div className="flex flex-col gap-5 sm:flex-row sm:items-center">
        {showPhoto ? (
          <img
            src={candidateDetails.fotoUrl}
            alt={`Foto de ${candidateDetails.nome}`}
            className="h-24 w-24 shrink-0 rounded-full object-cover shadow-xl ring-4 ring-white/15 md:h-28 md:w-28"
            onError={() => setPhotoFailed(true)}
          />
        ) : (
          <div
            className="flex h-24 w-24 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-indigo-500 to-violet-600 text-3xl font-bold text-white shadow-xl ring-4 ring-white/15 md:h-28 md:w-28"
            aria-hidden="true"
          >
            {getInitials(candidateDetails.nome)}
          </div>
        )}

        <div className="min-w-0">
          <h1 className="text-2xl font-bold tracking-tight text-white text-balance md:text-3xl">
            {candidateDetails.nome}
          </h1>
          {apelido && apelido.toLowerCase() !== candidateDetails.nome.toLowerCase() && (
            <p className="mt-1 text-slate-300">"{apelido}"</p>
          )}

          {latestElection && (
            <div className="mt-4 flex flex-wrap items-center gap-2 text-sm">
              <span className="rounded-full bg-white/10 px-3 py-1 font-medium text-white ring-1 ring-inset ring-white/15">
                {latestElection.cargo} · {latestElection.ano}
              </span>
              {latestElection.partido?.sigla && (
                <span className="rounded-full bg-white/10 px-3 py-1 font-medium text-white ring-1 ring-inset ring-white/15">
                  {latestElection.partido.sigla}
                </span>
              )}
              {(latestElection.nomeUE || latestElection.siglaUF) && (
                <span className="inline-flex items-center gap-1 rounded-full bg-white/10 px-3 py-1 text-slate-200 ring-1 ring-inset ring-white/15">
                  <MapPinIcon className="h-3.5 w-3.5" aria-hidden="true" />
                  {[latestElection.nomeUE, latestElection.siglaUF].filter(Boolean).join(' / ')}
                </span>
              )}
              {latestElection.resultado && (
                <span className={`rounded-full px-3 py-1 font-semibold ring-1 ring-inset ${getResultadoBadgeClass(latestElection.resultado)}`}>
                  {latestElection.resultado}
                </span>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default CandidateHeader;
