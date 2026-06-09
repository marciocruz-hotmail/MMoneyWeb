#!/bin/sh
set -e

mkdir -p /app/keys
chown -R app:app /app /app/keys 2>/dev/null || chown -R 1654:1654 /app /app/keys 2>/dev/null || true

echo "[entrypoint] Iniciando MMoneyWeb como utilizador app..."
exec gosu app dotnet MMoneyWeb.Web.dll
