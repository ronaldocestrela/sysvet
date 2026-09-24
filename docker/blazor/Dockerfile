# Blazor WebAssembly PWA — build arg selects the client project (staff, tutor, platform, clinic site).
ARG BLAZOR_PROJECT=src/Clients/BlazorWeb/BlazorWeb.csproj

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BLAZOR_PROJECT
WORKDIR /src

COPY global.json ./
COPY src/ ./src/

RUN dotnet workload install wasm-tools --skip-manifest-update
RUN dotnet restore "$BLAZOR_PROJECT"
# WasmBuildNative=false avoids emscripten/python in the SDK image; same pattern as TutorPortal/PlatformWeb csproj defaults.
RUN dotnet publish "$BLAZOR_PROJECT" -c Release -o /app/publish --no-restore -p:WasmBuildNative=false

FROM nginx:1.27-alpine AS final
WORKDIR /usr/share/nginx/html

COPY docker/blazor/nginx.conf /etc/nginx/conf.d/default.conf
COPY docker/blazor/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

COPY --from=build /app/publish/wwwroot ./

EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget -qO- http://127.0.0.1:8080/ >/dev/null || exit 1

ENTRYPOINT ["/entrypoint.sh"]
