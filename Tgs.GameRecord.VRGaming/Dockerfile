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
COPY ["Tgs.GameRecord.VRGaming/Tgs.GameRecord.VRGaming.csproj", "Tgs.GameRecord.VRGaming/"]
COPY ["Tgs.GameRecord.VRGaming.Common/Tgs.GameRecord.VRGaming.Common.csproj", "Tgs.GameRecord.VRGaming.Common/"]
COPY ["Tgs.GameRecord.VRGaming.Core/Tgs.GameRecord.VRGaming.Core.csproj", "Tgs.GameRecord.VRGaming.Core/"]
RUN dotnet restore "./Tgs.GameRecord.VRGaming/./Tgs.GameRecord.VRGaming.csproj"
COPY . .
WORKDIR "/src/Tgs.GameRecord.VRGaming"
RUN dotnet build "./Tgs.GameRecord.VRGaming.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.GameRecord.VRGaming.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameRecord.VRGaming.dll"]