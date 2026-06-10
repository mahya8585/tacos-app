# syntax=docker/dockerfile:1.7
# Build context: TacosApp.Admin/
# Build:  docker build -t tacosapp-admin:v1 -f TacosApp.Admin/Dockerfile TacosApp.Admin

ARG DOTNET_VERSION=10.0

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

COPY TacosApp.Admin.csproj ./
RUN dotnet restore TacosApp.Admin.csproj

COPY . ./
RUN dotnet publish TacosApp.Admin.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_NOLOGO=true

EXPOSE 8080

COPY --from=build /app/publish ./

USER $APP_UID
ENTRYPOINT ["dotnet", "TacosApp.Admin.dll"]
