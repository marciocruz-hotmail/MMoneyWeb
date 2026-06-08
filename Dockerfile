# MMoneyWeb — imagem de produção para Linux (Coolify / Docker)
# Blazor Interactive Server requer WebSocket no proxy reverso (Traefik no Coolify).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/MMoneyWeb.Web/MMoneyWeb.Web.csproj src/MMoneyWeb.Web/
RUN dotnet restore src/MMoneyWeb.Web/MMoneyWeb.Web.csproj

COPY src/MMoneyWeb.Web/ src/MMoneyWeb.Web/
RUN dotnet publish src/MMoneyWeb.Web/MMoneyWeb.Web.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl gosu \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /app/keys

COPY --from=build /app/publish .
COPY docker-entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0

EXPOSE 8080

ENTRYPOINT ["/entrypoint.sh"]
