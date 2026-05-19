ARG DOTNET_VERSION=10.0
ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS restore
WORKDIR /src

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

COPY PaymentService.csproj ./
COPY nuget.config ./

RUN --mount=type=secret,id=github_token \
    GITHUB_TOKEN=$(cat /run/secrets/github_token) \
    dotnet restore "PaymentService.csproj"

FROM restore AS publish
ARG BUILD_CONFIGURATION=Release

COPY . ./
RUN dotnet publish "PaymentService.csproj" \
    --configuration "${BUILD_CONFIGURATION}" \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false \
    /p:ContinuousIntegrationBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-noble-chiseled-extra AS production
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=5247=http://+:5247 

EXPOSE 5247

COPY --from=publish --chown=1654:1654 /app/publish ./

USER 1654
ENTRYPOINT ["dotnet", "PaymentService.dll"]
