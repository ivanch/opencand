import React from 'react';
import { LinkIcon } from '@heroicons/react/24/outline';
import { 
  FaFacebook, 
  FaInstagram, 
  FaTwitter, 
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

interface SocialMediaComponentProps {
  redesSociais: RedeSocial[] | null;
  isLoading: boolean;
}

// Helper function to get social media icons
const getSocialMediaIcon = (rede: string, isRecent: boolean = true): React.ReactElement => {
  const iconClass = "h-5 w-5 mr-2";
  const opacityClass = isRecent ? "" : "opacity-50";
  
  switch (rede.toLowerCase()) {
    case 'facebook':
      return <FaFacebook className={`${iconClass} ${opacityClass} text-blue-600`} />;
    case 'instagram':
      return <FaInstagram className={`${iconClass} ${opacityClass} text-pink-600`} />;
    case 'x/twitter':
    case 'twitter':
      return <FaXTwitter className={`${iconClass} ${opacityClass} text-black`} />;
    case 'tiktok':
      return <FaTiktok className={`${iconClass} ${opacityClass} text-black`} />;
    case 'youtube':
      return <FaYoutube className={`${iconClass} ${opacityClass} text-red-600`} />;
    case 'linkedin':
      return <FaLinkedin className={`${iconClass} ${opacityClass} text-blue-700`} />;
    case 'whatsapp':
      return <FaWhatsapp className={`${iconClass} ${opacityClass} text-green-600`} />;
    case 'threads':
      return <FaThreads className={`${iconClass} ${opacityClass} text-black`} />;
    case 'telegram':
      return <FaTelegram className={`${iconClass} ${opacityClass} text-blue-500`} />;
    case 'spotify':
      return <FaSpotify className={`${iconClass} ${opacityClass} text-green-500`} />;
    case 'kwai':
    case 'outros':
    default:
      return <FaLink className={`${iconClass} ${opacityClass} text-gray-600`} />;
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
    <div className="bg-white/95 backdrop-blur-sm rounded-xl shadow-lg hover:shadow-xl transform hover:scale-[1.01] transition-all duration-200 p-6">
      <div className="flex items-center mb-6">
        <LinkIcon className="h-8 w-8 text-orange-600 mr-3" />
        <h2 className="text-2xl font-bold text-gray-900">Redes Sociais</h2>
      </div>

      {isLoading ? (
        <div className="flex items-center justify-center h-32">
          <div className="animate-spin rounded-full h-8 w-8 border-4 border-orange-600 border-t-transparent"></div>
        </div>
      ) : redesSociais && redesSociais.length > 0 ? (
        <div className="space-y-3">
          {redesSociais.map((redeSocial: RedeSocial, index: number) => {
            const isRecent = redeSocial.ano === mostRecentYear;
            return (
              <div 
                key={`${redeSocial.idCandidato}-${redeSocial.rede}-${index}`} 
                className={`border border-gray-200 rounded-lg p-3 hover:bg-gray-50 transition-colors ${
                  isRecent ? '' : 'opacity-95 hover:bg-gray-100'
                }`}
              >
                <div className="flex items-center justify-between">
                  <div className="flex-1">
                    <div className="flex items-center mb-2">
                      {getSocialMediaIcon(redeSocial.rede, isRecent)}
                      <span className={`font-semibold mr-2 ${isRecent ? 'text-gray-900' : 'text-gray-500'}`}>
                        {redeSocial.rede}
                      </span>
                      <span className="text-sm text-gray-500">({redeSocial.ano})</span>
                    </div>
                    <a 
                      href={redeSocial.link} 
                      target="_blank" 
                      rel="noopener noreferrer"
                      className={`text-sm break-all transition-colors ${
                        isRecent 
                          ? 'text-blue-600 hover:text-blue-800' 
                          : 'text-blue-400 hover:text-blue-600'
                      }`}
                    >
                      {redeSocial.link}
                    </a>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      ) : (
        <div className="text-center text-gray-500 py-8">
          Nenhuma rede social encontrada
        </div>
      )}
    </div>
  );
};

export default SocialMediaComponent;
