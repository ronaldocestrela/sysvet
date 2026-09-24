#!/bin/sh
set -e

api_base="${API_PUBLIC_BASE_URL:-http://localhost:8080/}"
case "$api_base" in
  */) ;;
  *) api_base="${api_base}/" ;;
esac

# Runtime config for Blazor WASM (read by CreateDefault in Program.cs).
printf '%s\n' "{\"ApiBaseUrl\":\"${api_base}\"}" > /usr/share/nginx/html/appsettings.json

exec nginx -g 'daemon off;'
