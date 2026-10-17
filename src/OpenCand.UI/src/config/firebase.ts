
import { initializeApp } from "firebase/app";
import { getAnalytics } from "firebase/analytics";

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

let app = null;
let analytics = null;

if (firebaseConfig.projectId) {
  app = initializeApp(firebaseConfig);
  analytics = getAnalytics(app);
} else {
  console.warn("Firebase project ID is not set. Firebase will not be initialized.");
}

export { app, analytics };
