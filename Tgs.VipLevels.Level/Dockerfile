#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.VipLevels.Level/Tgs.VipLevels.Level.csproj", "Tgs.VipLevels.Level/"]
COPY ["Tgs.VipLevels.Level.Common/Tgs.VipLevels.Level.Common.csproj", "Tgs.VipLevels.Level.Common/"]
COPY ["Tgs.VipLevels.Level.Core/Tgs.VipLevels.Level.Core.csproj", "Tgs.VipLevels.Level.Core/"]
COPY ["Tgs.VipLevels.Level.Repositories/Tgs.VipLevels.Level.Repositories.csproj", "Tgs.VipLevels.Level.Repositories/"]
RUN dotnet restore "./Tgs.VipLevels.Level/./Tgs.VipLevels.Level.csproj"
COPY . .
WORKDIR "/src/Tgs.VipLevels.Level"
RUN dotnet build "./Tgs.VipLevels.Level.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.VipLevels.Level.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.VipLevels.Level.dll"]