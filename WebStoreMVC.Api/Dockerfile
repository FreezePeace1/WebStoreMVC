﻿FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WebStoreMVC.Api/WebStoreMVC.Api.csproj", "WebStoreMVC.Api/"]
RUN dotnet restore "WebStoreMVC.Api/WebStoreMVC.Api.csproj"
COPY . .
WORKDIR "/src/WebStoreMVC.Api"
RUN dotnet build "WebStoreMVC.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebStoreMVC.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebStoreMVC.Api.dll"]
