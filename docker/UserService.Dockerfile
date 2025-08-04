# User Service Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/UserService/UserService.API/UserService.API.csproj", "src/UserService/UserService.API/"]
COPY ["src/UserService/UserService.Core/UserService.Core.csproj", "src/UserService/UserService.Core/"]
COPY ["src/UserService/UserService.Infrastructure/UserService.Infrastructure.csproj", "src/UserService/UserService.Infrastructure/"]
RUN dotnet restore "src/UserService/UserService.API/UserService.API.csproj"
COPY . .
WORKDIR "/src/src/UserService/UserService.API"
RUN dotnet build "UserService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserService.API.dll"]
