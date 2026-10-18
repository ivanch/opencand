
import { initializeApp, type FirebaseApp } from "firebase/app";
import type { Analytics } from "firebase/analytics";

declare global {
  interface Window {
    __ENV__?: Record<string, string>;
  }
}

const getEnv = (key: string): string | undefined => {
  if (typeof window !== 'undefined' && window.__ENV__) {
    return window.__ENV__[key];
  }
  return import.meta.env[key];
};

const firebaseConfig = {
  apiKey: getEnv('VITE_FIREBASE_API_KEY'),
  authDomain: getEnv('VITE_FIREBASE_AUTH_DOMAIN'),
  projectId: getEnv('VITE_FIREBASE_PROJECT_ID'),
  storageBucket: getEnv('VITE_FIREBASE_STORAGE_BUCKET'),
  messagingSenderId: getEnv('VITE_FIREBASE_MESSAGING_SENDER_ID'),
  appId: getEnv('VITE_FIREBASE_APP_ID'),
  measurementId: getEnv('VITE_FIREBASE_MEASUREMENT_ID')
};

let app: FirebaseApp | null = null;
let analytics: Analytics | null = null;

if (firebaseConfig.projectId) {
  app = initializeApp(firebaseConfig);
} else {
  console.warn("Firebase project ID is not set. Firebase will not be initialized.");
}

/** True when Firebase is configured with a measurement id, i.e. analytics could be started. */
export const analyticsAvailable = !!app && !!firebaseConfig.measurementId;

/**
 * Starts Firebase Analytics. Must only be called after the visitor consented (see ConsentBanner):
 * the analytics SDK is loaded lazily here, so nothing is fetched or sent before that.
 */
export const initAnalytics = async (): Promise<void> => {
  if (!app || !analyticsAvailable || analytics) return;
  const { getAnalytics } = await import("firebase/analytics");
  analytics ??= getAnalytics(app);
};

export { app };
