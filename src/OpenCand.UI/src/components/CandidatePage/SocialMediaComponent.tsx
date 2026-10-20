import React from 'react';
import { ArrowTopRightOnSquareIcon, LinkIcon } from '@heroicons/react/24/outline';
import {
  FaFacebook,
  FaInstagram,
  FaTiktok,
  FaYoutube,
  FaLinkedin,
  FaWhatsapp,
  FaTelegram,
  FaSpotify,
  FaLink
} from 'react-icons/fa';
import { FaXTwitter, FaThreads } from 'react-icons/fa6';
import { type RedeSocial } from '../../api';
import Panel from '../../shared/Panel';
import { LoadingBlock } from '../../shared/Spinner';

interface SocialMediaComponentProps {
  redesSociais: RedeSocial[] | null;
  isLoading: boolean;
}

// Only http(s) links become anchors: the link text comes from the API, so e.g. a javascript: URL must never be clickable
const isHttpUrl = (value: string): boolean => {
  try {
    const { protocol } = new URL(value);
    return protocol === 'http:' || protocol === 'https:';
  } catch {
    return false;
  }
};

// Shows the link without the protocol and trailing slash, e.g. "instagram.com/fulano"
const prettyLink = (value: string): string => value.replace(/^https?:\/\/(www\.)?/i, '').replace(/\/$/, '');

// Helper function to get social media icons
const getSocialMediaIcon = (rede: string): React.ReactElement => {
  const iconClass = 'h-5 w-5';

  switch (rede.toLowerCase()) {
    case 'facebook':
      return <FaFacebook className={`${iconClass} text-blue-600`} />;
    case 'instagram':
      return <FaInstagram className={`${iconClass} text-pink-600`} />;
    case 'x/twitter':
    case 'twitter':
      return <FaXTwitter className={`${iconClass} text-slate-900`} />;
    case 'tiktok':
      return <FaTiktok className={`${iconClass} text-slate-900`} />;
    case 'youtube':
      return <FaYoutube className={`${iconClass} text-red-600`} />;
    case 'linkedin':
      return <FaLinkedin className={`${iconClass} text-blue-700`} />;
    case 'whatsapp':
      return <FaWhatsapp className={`${iconClass} text-green-600`} />;
    case 'threads':
      return <FaThreads className={`${iconClass} text-slate-900`} />;
    case 'telegram':
      return <FaTelegram className={`${iconClass} text-sky-500`} />;
    case 'spotify':
      return <FaSpotify className={`${iconClass} text-green-500`} />;
    case 'kwai':
    case 'outros':
    default:
      return <FaLink className={`${iconClass} text-slate-500`} />;
  }
};

const SocialMediaComponent: React.FC<SocialMediaComponentProps> = ({
  redesSociais,
  isLoading
}) => {
  // Calculate the most recent year from all social media entries
  const mostRecentYear = redesSociais && redesSociais.length > 0
    ? Math.max(...redesSociais.map(rede => rede.ano))
    : null;

  return (
    <Panel title="Redes Sociais" icon={LinkIcon} iconClassName="bg-orange-50 text-orange-600">
      {isLoading ? (
        <LoadingBlock colorClassName="border-orange-600" />
      ) : redesSociais && redesSociais.length > 0 ? (
        <ul className="space-y-2">
          {redesSociais.map((redeSocial: RedeSocial, index: number) => {
            const isRecent = redeSocial.ano === mostRecentYear;
            const isLink = isHttpUrl(redeSocial.link);
            const content = (
              <>
                <span className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-slate-50 ring-1 ring-inset ring-slate-100 ${isRecent ? '' : 'opacity-60'}`}>
                  {getSocialMediaIcon(redeSocial.rede)}
                </span>
                <span className="min-w-0 flex-1">
                  <span className="flex items-center gap-2">
                    <span className={`text-sm font-semibold ${isRecent ? 'text-slate-900' : 'text-slate-500'}`}>{redeSocial.rede}</span>
                    <span className="rounded-md bg-slate-100 px-1.5 py-0.5 text-xs font-medium text-slate-500 tabular-nums">{redeSocial.ano}</span>
                  </span>
                  <span className={`block truncate text-sm ${isLink ? (isRecent ? 'text-blue-600' : 'text-blue-400') : 'text-slate-500'}`}>
                    {prettyLink(redeSocial.link)}
                  </span>
                </span>
                {isLink && (
                  <ArrowTopRightOnSquareIcon className="h-4 w-4 shrink-0 text-slate-300 transition-colors group-hover:text-blue-600" aria-hidden="true" />
                )}
              </>
            );

            return (
              <li key={`${redeSocial.idCandidato}-${redeSocial.rede}-${index}`}>
                {isLink ? (
                  <a
                    href={redeSocial.link}
                    target="_blank"
                    rel="noopener noreferrer"
                    title={redeSocial.link}
                    className="group flex items-center gap-3 rounded-xl p-2.5 ring-1 ring-inset ring-slate-200 transition-colors hover:bg-slate-50 hover:ring-slate-300"
                  >
                    {content}
                  </a>
                ) : (
                  <div className="flex items-center gap-3 rounded-xl p-2.5 ring-1 ring-inset ring-slate-200" title={redeSocial.link}>
                    {content}
                  </div>
                )}
              </li>
            );
          })}
        </ul>
      ) : (
        <p className="py-8 text-center text-slate-500">Nenhuma rede social encontrada</p>
      )}
    </Panel>
  );
};

export default SocialMediaComponent;
