#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["WalletAPI.Services.API/WalletAPI.Services.API.csproj", "WalletAPI.Services.API/"]
COPY ["WalletAPI.Services.Core/WalletAPI.Services.Core.csproj", "WalletAPI.Services.Core/"]
COPY ["WalletAPI.Commons.Utilities/WalletAPI.Commons.Utilities.csproj", "WalletAPI.Commons.Utilities/"]
COPY ["WalletAPI.Services.Models/WalletAPI.Services.Models.csproj", "WalletAPI.Services.Models/"]
COPY ["WalletAPI.Services.Data/WalletAPI.Services.Data.csproj", "WalletAPI.Services.Data/"]
COPY ["WalletAPI.Services.DTOs/WalletAPI.Services.DTOs.csproj", "WalletAPI.Services.DTOs/"]
RUN dotnet restore "WalletAPI.Services.API/WalletAPI.Services.API.csproj"
COPY . .
WORKDIR "/src/WalletAPI.Services.API"
RUN dotnet build "WalletAPI.Services.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WalletAPI.Services.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WalletAPI.Services.API.dll"]