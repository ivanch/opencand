#!/bin/sh
set -e

# Generate env.js with runtime secrets
ENV_DIR="/usr/share/nginx/html"
ENV_FILE="$ENV_DIR/env.js"

if [ ! -d "$ENV_DIR" ]; then
  echo "Error: Directory $ENV_DIR does not exist." >&2
  exit 1
fi

if [ ! -w "$ENV_DIR" ]; then
  echo "Error: Directory $ENV_DIR is not writable." >&2
  exit 1
fi

cat <<EOF > "$ENV_FILE"
window.__ENV__ = {
  VITE_API_BASE_URL: "${VITE_API_BASE_URL:-http://localhost:5299}",
  VITE_FIREBASE_API_KEY: "${VITE_FIREBASE_API_KEY}",
  VITE_FIREBASE_AUTH_DOMAIN: "${VITE_FIREBASE_AUTH_DOMAIN}",
  VITE_FIREBASE_STORAGE_BUCKET: "${VITE_FIREBASE_STORAGE_BUCKET}",
  VITE_FIREBASE_PROJECT_ID: "${VITE_FIREBASE_PROJECT_ID}",
  VITE_FIREBASE_MESSAGING_SENDER_ID: "${VITE_FIREBASE_MESSAGING_SENDER_ID}",
  VITE_FIREBASE_APP_ID: "${VITE_FIREBASE_APP_ID}",
  VITE_FIREBASE_MEASUREMENT_ID: "${VITE_FIREBASE_MEASUREMENT_ID}"
};
EOF

exec nginx -g 'daemon off;'
