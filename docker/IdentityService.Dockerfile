# Identity Service Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5128

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/IdentityService/IdentityService.API/IdentityService.API.csproj", "src/IdentityService/IdentityService.API/"]
COPY ["src/IdentityService/IdentityService.Core/IdentityService.Core.csproj", "src/IdentityService/IdentityService.Core/"]
COPY ["src/IdentityService/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj", "src/IdentityService/IdentityService.Infrastructure/"]
RUN dotnet restore "src/IdentityService/IdentityService.API/IdentityService.API.csproj"
COPY . .
WORKDIR "/src/src/IdentityService/IdentityService.API"
RUN dotnet build "IdentityService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IdentityService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5128

# Add health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5128/health || exit 1

ENTRYPOINT ["dotnet", "IdentityService.API.dll"]
