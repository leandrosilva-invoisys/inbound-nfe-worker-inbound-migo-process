FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Plugin.InboundMigoProcess.Infra.Worker/Plugin.InboundMigoProcess.Infra.Worker.csproj", "Plugin.InboundMigoProcess.Infra.Worker/"]
COPY ["Plugin.InboundMigoProcess.Infra/Plugin.InboundMigoProcess.Infra.csproj", "Plugin.InboundMigoProcess.Infra/"]
COPY ["Plugin.InboundMigoProcess.Application/Plugin.InboundMigoProcess.Application.csproj", "Plugin.InboundMigoProcess.Application/"]
RUN dotnet restore "./Plugin.InboundMigoProcess.Infra.Worker/Plugin.InboundMigoProcess.Infra.Worker.csproj"
COPY . .
WORKDIR "/src/Plugin.InboundMigoProcess.Infra.Worker"
RUN dotnet build "./Plugin.InboundMigoProcess.Infra.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Plugin.InboundMigoProcess.Infra.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Plugin.InboundMigoProcess.Infra.Worker.dll"]
