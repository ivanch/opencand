import { useEffect } from 'react';

const APP_NAME = 'OpenCand';

/** Sets the browser tab title to "<title> · OpenCand" (or just "OpenCand" without a title). */
export function usePageTitle(title?: string | null) {
  useEffect(() => {
    document.title = title ? `${title} · ${APP_NAME}` : APP_NAME;
  }, [title]);
}
