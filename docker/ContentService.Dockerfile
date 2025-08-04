# Content Service Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/ContentService/ContentService.API/ContentService.API.csproj", "src/ContentService/ContentService.API/"]
COPY ["src/ContentService/ContentService.Core/ContentService.Core.csproj", "src/ContentService/ContentService.Core/"]
COPY ["src/ContentService/ContentService.Infrastructure/ContentService.Infrastructure.csproj", "src/ContentService/ContentService.Infrastructure/"]
RUN dotnet restore "src/ContentService/ContentService.API/ContentService.API.csproj"
COPY . .
WORKDIR "/src/src/ContentService/ContentService.API"
RUN dotnet build "ContentService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ContentService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContentService.API.dll"]
