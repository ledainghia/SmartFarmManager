#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SmartFarmManager.API/SmartFarmManager.API.csproj", "SmartFarmManager.API/"]
COPY ["SmartFarmManager.Service/SmartFarmManager.Service.csproj", "SmartFarmManager.Service/"]
COPY ["SmartFarmManager.Repository/SmartFarmManager.Repository.csproj", "SmartFarmManager.Repository/"]
COPY ["SmartFarmManager.DataAccessObject/SmartFarmManager.DataAccessObject.csproj", "SmartFarmManager.DataAccessObject/"]
RUN dotnet restore "./SmartFarmManager.API/SmartFarmManager.API.csproj"
COPY . .
WORKDIR "/src/SmartFarmManager.API"
RUN dotnet build "./SmartFarmManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SmartFarmManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartFarmManager.API.dll"]