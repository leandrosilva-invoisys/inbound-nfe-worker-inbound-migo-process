# Substitua HelloWorld pelo nome do seu projeto (ex: NFeEpecHandler).

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["HelloWorld.Infra.Worker/HelloWorld.Infra.Worker.csproj", "HelloWorld.Infra.Worker/"]
COPY ["HelloWorld.Infra/HelloWorld.Infra.csproj", "HelloWorld.Infra/"]
COPY ["HelloWorld.Application/HelloWorld.Application.csproj", "HelloWorld.Application/"]
RUN dotnet restore "./HelloWorld.Infra.Worker/HelloWorld.Infra.Worker.csproj"
COPY . .
WORKDIR "/src/HelloWorld.Infra.Worker"
RUN dotnet build "./HelloWorld.Infra.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HelloWorld.Infra.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HelloWorld.Infra.Worker.dll"]
