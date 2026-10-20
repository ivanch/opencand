import type { ComponentType } from 'react';
import { CameraIcon, CurrencyDollarIcon, DevicePhoneMobileIcon, ReceiptPercentIcon, UserIcon, WalletIcon } from '@heroicons/react/24/outline';
import type { OpenCandDataAvailabilityStats } from '../../api/apiModels';

/** Rows of the availability matrix, one per kind of data the ETL loads */
export const dataTypes: Array<{ key: keyof OpenCandDataAvailabilityStats; label: string; icon: ComponentType<{ className?: string }> }> = [
  { key: 'candidatos', label: 'Candidatos', icon: UserIcon },
  { key: 'bemCandidatos', label: 'Bens de Candidatos', icon: CurrencyDollarIcon },
  { key: 'despesaCandidatos', label: 'Despesas de Candidatos', icon: ReceiptPercentIcon },
  { key: 'receitaCandidatos', label: 'Receitas de Candidatos', icon: WalletIcon },
  { key: 'redeSocialCandidatos', label: 'Redes Sociais', icon: DevicePhoneMobileIcon },
  { key: 'fotosCandidatos', label: 'Fotos de Candidatos (API)', icon: CameraIcon },
];
