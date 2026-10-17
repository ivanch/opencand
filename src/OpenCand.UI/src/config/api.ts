// API configuration

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

export const API_CONFIG = {
  baseUrl: getEnv('VITE_API_BASE_URL') || (import.meta.env.DEV ? 'http://localhost:5299' : 'API_BASE_URL_NOT_SET'),
  timeout: 30000, // 30 seconds
} as const;

// Environment configuration

export const ENV_CONFIG = {
  isDevelopment: typeof import.meta.env !== 'undefined' ? import.meta.env.DEV : false,
  isProduction: typeof import.meta.env !== 'undefined' ? import.meta.env.PROD : false,
} as const;
