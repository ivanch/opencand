import React, { useCallback, useEffect, useState } from 'react';
import { analyticsAvailable, initAnalytics } from '../config/firebase';
import { OPEN_CONSENT_EVENT, getStoredConsent, storeConsent, type AnalyticsConsent } from '../config/consent';

/**
 * Asks for consent to analytics and starts Firebase Analytics only once it is granted.
 * The footer link dispatches OPEN_CONSENT_EVENT so the visitor can change the choice later.
 */
const ConsentBanner: React.FC = () => {
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    if (!analyticsAvailable) return;

    if (getStoredConsent() === 'granted') {
      void initAnalytics();
    } else if (getStoredConsent() === null) {
      setVisible(true);
    }

    const open = () => setVisible(true);
    window.addEventListener(OPEN_CONSENT_EVENT, open);
    return () => window.removeEventListener(OPEN_CONSENT_EVENT, open);
  }, []);

  const choose = useCallback((consent: AnalyticsConsent) => {
    storeConsent(consent);
    setVisible(false);
    if (consent === 'granted') {
      void initAnalytics();
    }
    // Withdrawing consent takes full effect on the next page load, when analytics is no longer started
  }, []);

  if (!visible) return null;

  return (
    <div
      role="dialog"
      aria-label="Preferências de privacidade"
      className="fixed bottom-0 inset-x-0 z-50 p-4"
    >
      <div className="max-w-3xl mx-auto bg-surface-900/95 backdrop-blur-xl text-slate-200 rounded-2xl shadow-2xl shadow-black/40 ring-1 ring-white/10 p-4 sm:p-5 flex flex-col sm:flex-row sm:items-center gap-4 text-left animate-fade-in-up">
        <p className="text-sm flex-1">
          Usamos o Google Analytics (via Firebase) para entender como o site é usado. Os dados só são coletados se você permitir.
        </p>
        <div className="flex gap-2 shrink-0">
          <button
            type="button"
            onClick={() => choose('denied')}
            className="px-4 py-2 rounded-full text-sm font-medium bg-white/10 hover:bg-white/20 transition-colors cursor-pointer"
          >
            Recusar
          </button>
          <button
            type="button"
            onClick={() => choose('granted')}
            className="px-4 py-2 rounded-full text-sm font-semibold bg-indigo-500 hover:bg-indigo-400 text-white transition-colors cursor-pointer"
          >
            Aceitar
          </button>
        </div>
      </div>
    </div>
  );
};

export default ConsentBanner;
