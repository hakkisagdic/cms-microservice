# Deployment Kılavuzu

Bu dokümant, CMS Mikroservis projesinin farklı ortamlarda deployment yapılması için gerekli adımları içerir.

## 📋 İçindekiler
1. [Genel Gereksinimler](#genel-gereksinimler)
2. [Development Deployment](#development-deployment)
3. [Production Deployment](#production-deployment)
4. [Docker Deployment](#docker-deployment)
5. [Kubernetes Deployment](#kubernetes-deployment)
6. [Environment Variables](#environment-variables)
7. [Database Migration](#database-migration)
8. [Monitoring Setup](#monitoring-setup)
9. [Troubleshooting](#troubleshooting)

## 📋 Genel Gereksinimler

### Sistem Gereksinimleri
- **CPU**: 2+ cores
- **RAM**: 4GB+ (8GB önerilen)
- **Disk**: 20GB+ available space
- **Network**: HTTP/HTTPS erişimi

### Yazılım Gereksinimleri
- .NET 8.0 SDK/Runtime
- PostgreSQL 15+
- Docker 24.0+
- Docker Compose 2.0+

## 🛠️ Development Deployment

### 1. Yerel Geliştirme Ortamı

#### Veritabanı Kurulumu
```bash
# PostgreSQL Docker ile başlat
docker-compose -f docker-compose.dev.yml up -d postgres

# Veritabanı bağlantısını doğrula
docker exec -it cms_postgres_dev psql -U postgres -c "\l"
```

#### User Service Başlatma
```bash
cd src/UserService/UserService.API

# NuGet paketlerini geri yükle
dotnet restore

# Veritabanı migrations
dotnet ef database update

# Servisi başlat
dotnet run
```

#### Content Service Başlatma
```bash
cd src/ContentService/ContentService.API

# NuGet paketlerini geri yükle
dotnet restore

# Veritabanı migrations
dotnet ef database update

# Servisi başlat
dotnet run
```

### 2. Development Environment Variables

**User Service (appsettings.Development.json):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=UserServiceDb_Dev;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

**Content Service (appsettings.Development.json):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ContentServiceDb_Dev;Username=postgres;Password=postgres"
  },
  "Services": {
    "UserService": {
      "BaseUrl": "http://localhost:5001"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

## 🚀 Production Deployment

### 1. Manual Production Deployment

#### Sunucu Hazırlığı
```bash
# .NET 8 Runtime kurulumu (Ubuntu/Debian)
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-8.0

# PostgreSQL kurulumu
sudo apt-get install -y postgresql postgresql-contrib
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

#### Uygulama Build ve Deploy
```bash
# Repository klonla
git clone <repository-url>
cd cms-microservice

# User Service build
cd src/UserService/UserService.API
dotnet publish -c Release -o /opt/userservice

# Content Service build
cd ../../../src/ContentService/ContentService.API
dotnet publish -c Release -o /opt/contentservice
```

#### Systemd Service Dosyaları

**User Service (/etc/systemd/system/userservice.service):**
```ini
[Unit]
Description=CMS User Service
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /opt/userservice/UserService.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=userservice
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5001

[Install]
WantedBy=multi-user.target
```

**Content Service (/etc/systemd/system/contentservice.service):**
```ini
[Unit]
Description=CMS Content Service
After=network.target userservice.service

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /opt/contentservice/ContentService.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=contentservice
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5002

[Install]
WantedBy=multi-user.target
```

#### Service'leri Başlatma
```bash
# Service dosyalarını yeniden yükle
sudo systemctl daemon-reload

# Service'leri etkinleştir ve başlat
sudo systemctl enable userservice
sudo systemctl enable contentservice
sudo systemctl start userservice
sudo systemctl start contentservice

# Durum kontrolü
sudo systemctl status userservice
sudo systemctl status contentservice
```

### 2. Nginx Reverse Proxy

**Nginx Configuration (/etc/nginx/sites-available/cms-api):**
```nginx
upstream userservice {
    server localhost:5001;
}

upstream contentservice {
    server localhost:5002;
}

server {
    listen 80;
    server_name api.yourdomain.com;

    # User Service
    location /api/users {
        proxy_pass http://userservice;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    # Content Service
    location /api/contents {
        proxy_pass http://contentservice;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    # Health checks
    location /health {
        access_log off;
        return 200 "healthy\n";
    }
}
```

```bash
# Nginx configuration'ı etkinleştir
sudo ln -s /etc/nginx/sites-available/cms-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

## 🐳 Docker Deployment

### 1. Development ile Docker

```bash
# Database için
docker-compose -f docker-compose.dev.yml up -d

# Servisleri local'de çalıştır
cd src/UserService/UserService.API && dotnet run &
cd src/ContentService/ContentService.API && dotnet run &
```

### 2. Production ile Docker

```bash
# Tüm stack'i başlat
docker-compose up -d

# Logları takip et
docker-compose logs -f

# Health check
curl http://localhost:5001/health
curl http://localhost:5002/health
```

### 3. Docker Build Optimizasyonu

**Multi-stage Dockerfile örneği:**
```dockerfile
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
```

## ☸️ Kubernetes Deployment

### 1. Namespace Oluşturma

```yaml
# namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: cms-microservices
```

### 2. ConfigMap ve Secrets

```yaml
# configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: cms-config
  namespace: cms-microservices
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  UserService__BaseUrl: "http://userservice:80"
---
apiVersion: v1
kind: Secret
metadata:
  name: cms-secrets
  namespace: cms-microservices
type: Opaque
data:
  postgres-password: cG9zdGdyZXM= # base64 encoded "postgres"
```

### 3. PostgreSQL Deployment

```yaml
# postgres.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: postgres
  namespace: cms-microservices
spec:
  replicas: 1
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
    spec:
      containers:
      - name: postgres
        image: postgres:15-alpine
        env:
        - name: POSTGRES_DB
          value: "postgres"
        - name: POSTGRES_USER
          value: "postgres"
        - name: POSTGRES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: cms-secrets
              key: postgres-password
        ports:
        - containerPort: 5432
        volumeMounts:
        - name: postgres-data
          mountPath: /var/lib/postgresql/data
      volumes:
      - name: postgres-data
        persistentVolumeClaim:
          claimName: postgres-pvc
---
apiVersion: v1
kind: Service
metadata:
  name: postgres
  namespace: cms-microservices
spec:
  selector:
    app: postgres
  ports:
  - port: 5432
    targetPort: 5432
```

### 4. User Service Deployment

```yaml
# userservice.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: userservice
  namespace: cms-microservices
spec:
  replicas: 2
  selector:
    matchLabels:
      app: userservice
  template:
    metadata:
      labels:
        app: userservice
    spec:
      containers:
      - name: userservice
        image: your-registry/userservice:latest
        env:
        - name: ASPNETCORE_ENVIRONMENT
          valueFrom:
            configMapKeyRef:
              name: cms-config
              key: ASPNETCORE_ENVIRONMENT
        - name: ConnectionStrings__DefaultConnection
          value: "Host=postgres;Port=5432;Database=UserServiceDb;Username=postgres;Password=postgres"
        ports:
        - containerPort: 80
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: userservice
  namespace: cms-microservices
spec:
  selector:
    app: userservice
  ports:
  - port: 80
    targetPort: 80
```

### 5. Content Service Deployment

```yaml
# contentservice.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: contentservice
  namespace: cms-microservices
spec:
  replicas: 2
  selector:
    matchLabels:
      app: contentservice
  template:
    metadata:
      labels:
        app: contentservice
    spec:
      containers:
      - name: contentservice
        image: your-registry/contentservice:latest
        env:
        - name: ASPNETCORE_ENVIRONMENT
          valueFrom:
            configMapKeyRef:
              name: cms-config
              key: ASPNETCORE_ENVIRONMENT
        - name: ConnectionStrings__DefaultConnection
          value: "Host=postgres;Port=5432;Database=ContentServiceDb;Username=postgres;Password=postgres"
        - name: Services__UserService__BaseUrl
          valueFrom:
            configMapKeyRef:
              name: cms-config
              key: UserService__BaseUrl
        ports:
        - containerPort: 80
---
apiVersion: v1
kind: Service
metadata:
  name: contentservice
  namespace: cms-microservices
spec:
  selector:
    app: contentservice
  ports:
  - port: 80
    targetPort: 80
```

### 6. Ingress Configuration

```yaml
# ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: cms-ingress
  namespace: cms-microservices
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
spec:
  rules:
  - host: api.yourdomain.com
    http:
      paths:
      - path: /api/users
        pathType: Prefix
        backend:
          service:
            name: userservice
            port:
              number: 80
      - path: /api/contents
        pathType: Prefix
        backend:
          service:
            name: contentservice
            port:
              number: 80
```

### 7. Kubernetes Deployment Komutları

```bash
# Tüm resources'ları deploy et
kubectl apply -f namespace.yaml
kubectl apply -f configmap.yaml
kubectl apply -f postgres.yaml
kubectl apply -f userservice.yaml
kubectl apply -f contentservice.yaml
kubectl apply -f ingress.yaml

# Pod durumlarını kontrol et
kubectl get pods -n cms-microservices

# Service'leri kontrol et
kubectl get services -n cms-microservices

# Logs
kubectl logs -f deployment/userservice -n cms-microservices
kubectl logs -f deployment/contentservice -n cms-microservices
```

## 🔧 Environment Variables

### User Service Environment Variables

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `ASPNETCORE_ENVIRONMENT` | Application environment | Development | Yes |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | - | Yes |
| `ASPNETCORE_URLS` | URLs to listen on | http://localhost:5001 | No |
| `Logging__LogLevel__Default` | Default log level | Information | No |

### Content Service Environment Variables

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `ASPNETCORE_ENVIRONMENT` | Application environment | Development | Yes |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | - | Yes |
| `Services__UserService__BaseUrl` | User Service base URL | http://localhost:5001 | Yes |
| `ASPNETCORE_URLS` | URLs to listen on | http://localhost:5002 | No |

### Database Migration

#### Manual Migration

```bash
# User Service
cd src/UserService/UserService.API
dotnet ef database update

# Content Service
cd src/ContentService/ContentService.API
dotnet ef database update
```

#### Docker Migration

```bash
# Migration container çalıştır
docker run --rm --network cms_network \
  -e ConnectionStrings__DefaultConnection="Host=postgres;..." \
  your-registry/userservice:latest \
  dotnet ef database update
```

#### Kubernetes Migration Job

```yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: userservice-migration
  namespace: cms-microservices
spec:
  template:
    spec:
      containers:
      - name: migration
        image: your-registry/userservice:latest
        command: ["dotnet", "ef", "database", "update"]
        env:
        - name: ConnectionStrings__DefaultConnection
          value: "Host=postgres;Port=5432;Database=UserServiceDb;Username=postgres;Password=postgres"
      restartPolicy: Never
```

## 📊 Monitoring Setup

### 1. Health Checks

Her servis `/health` endpoint'i sağlar:

```bash
# Health check test
curl http://localhost:5001/health
curl http://localhost:5002/health
```

### 2. Logging

Serilog ile structured logging:

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/service-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

### 3. Metrics (Gelecek)

Prometheus/Grafana entegrasyonu için:

```csharp
// Program.cs
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddPrometheusExporter();
        metrics.AddMeter("UserService");
    });
```

## 🔍 Troubleshooting

### Common Issues

#### 1. Database Connection Issues

**Problem**: `Npgsql.NpgsqlException: Connection refused`

**Solution**:
```bash
# PostgreSQL durumunu kontrol et
docker ps | grep postgres
sudo systemctl status postgresql

# Connection string'i doğrula
echo $ConnectionStrings__DefaultConnection
```

#### 2. Service Discovery Issues

**Problem**: Content Service, User Service'e bağlanamıyor

**Solution**:
```bash
# Docker network'ü kontrol et
docker network ls
docker network inspect cms_network

# Service URL'lerini doğrula
curl http://userservice:80/health  # Docker içinden
curl http://localhost:5001/health  # Host'tan
```

#### 3. Memory Issues

**Problem**: Out of memory exceptions

**Solution**:
```bash
# Memory usage kontrol
docker stats
kubectl top pods -n cms-microservices

# Limit ayarla
resources:
  limits:
    memory: "512Mi"
    cpu: "500m"
  requests:
    memory: "256Mi"
    cpu: "250m"
```

#### 4. Port Conflicts

**Problem**: Port already in use

**Solution**:
```bash
# Port kullanımını kontrol et
netstat -tulpn | grep :5001
lsof -i :5001

# Farklı port kullan
export ASPNETCORE_URLS="http://localhost:5003"
```

### Log Analysis

```bash
# Docker logs
docker-compose logs -f userservice
docker-compose logs -f contentservice

# Systemd logs
sudo journalctl -u userservice -f
sudo journalctl -u contentservice -f

# Kubernetes logs
kubectl logs -f deployment/userservice -n cms-microservices
kubectl logs -f deployment/contentservice -n cms-microservices
```

### Performance Tuning

#### Database Optimization

```sql
-- Index'leri kontrol et
SELECT indexname, tablename FROM pg_indexes WHERE schemaname = 'public';

-- Slow query'leri bul
SELECT query, mean_time, calls FROM pg_stat_statements ORDER BY mean_time DESC LIMIT 10;
```

#### Application Optimization

```csharp
// Connection pooling
services.AddDbContext<UserDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(30);
    });
}, ServiceLifetime.Scoped);
```

---

Bu deployment kılavuzu, CMS mikroservis projesinin farklı ortamlarda başarılı bir şekilde deploy edilmesi için gerekli tüm adımları içermektedir. Herhangi bir sorun yaşandığında troubleshooting bölümünü kontrol edin veya issue oluşturun.
