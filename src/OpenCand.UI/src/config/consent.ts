// Analytics consent (LGPD): the choice is kept in localStorage; nothing is sent before it is granted.

export type AnalyticsConsent = 'granted' | 'denied';

const STORAGE_KEY = 'opencand.analytics-consent';
export const OPEN_CONSENT_EVENT = 'opencand:open-consent';

export const getStoredConsent = (): AnalyticsConsent | null => {
  try {
    const value = window.localStorage.getItem(STORAGE_KEY);
    return value === 'granted' || value === 'denied' ? value : null;
  } catch {
    // Storage may be unavailable (private mode, blocked cookies): treat as "not answered yet"
    return null;
  }
};

export const storeConsent = (consent: AnalyticsConsent | null): void => {
  try {
    if (consent === null) {
      window.localStorage.removeItem(STORAGE_KEY);
    } else {
      window.localStorage.setItem(STORAGE_KEY, consent);
    }
  } catch {
    // Ignore: the choice just won't be remembered across visits
  }
};
