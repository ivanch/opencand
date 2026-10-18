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

# Security headers (included by nginx.conf). The CSP is built at start-up because connect-src
# must allow the API origin configured in VITE_API_BASE_URL, which is only known at runtime.
HEADERS_FILE="/etc/nginx/security-headers.conf"
API_BASE_URL="${VITE_API_BASE_URL:-http://localhost:5299}"

# Keep only scheme://host[:port]; relative URLs (e.g. /api) are covered by 'self'.
API_ORIGIN=$(printf '%s' "$API_BASE_URL" | sed -n 's#^\(https\{0,1\}://[A-Za-z0-9._:-]*\).*#\1#p')
if [ -z "$API_ORIGIN" ] && [ "${API_BASE_URL#/}" = "$API_BASE_URL" ]; then
  echo "Warning: VITE_API_BASE_URL '$API_BASE_URL' is not a plain http(s) URL; it is NOT added to the CSP connect-src." >&2
fi

# Firebase / Google Analytics: gtag script plus its beacon and config endpoints.
CSP="default-src 'self'; script-src 'self' https://*.googletagmanager.com https://*.google-analytics.com; connect-src 'self' $API_ORIGIN https://*.googleapis.com https://*.google-analytics.com https://*.analytics.google.com https://*.googletagmanager.com; img-src 'self' data: https://*.google-analytics.com https://*.googletagmanager.com; style-src 'self' 'unsafe-inline'; font-src 'self' data:; object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'self'"

cat <<EOF > "$HEADERS_FILE"
add_header X-Content-Type-Options "nosniff" always;
add_header Referrer-Policy "strict-origin-when-cross-origin" always;
add_header Content-Security-Policy "$CSP" always;
EOF

exec nginx -g 'daemon off;'
