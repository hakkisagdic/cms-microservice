# 🐳 Docker Deployment Guide

Bu rehber, CMS Mikroservis projesinin Docker ile deployment sürecini detaylı olarak açıklamaktadır.

## 📋 İçindekiler

- [Ön Gereksinimler](#ön-gereksinimler)
- [Quick Start](#quick-start)
- [Container Architecture](#container-architecture)
- [Network Configuration](#network-configuration)
- [Volume Management](#volume-management)
- [Environment Variables](#environment-variables)
- [Health Checks](#health-checks)
- [Monitoring](#monitoring)
- [Troubleshooting](#troubleshooting)
- [Production Deployment](#production-deployment)

## 🔧 Ön Gereksinimler

- **Docker Desktop**: 4.20+ (macOS/Windows) veya Docker Engine 24.0+ (Linux)
- **Docker Compose**: 2.17+ (Desktop ile birlikte gelir)
- **Minimum System Requirements**:
  - RAM: 4GB (8GB önerilen)
  - Disk: 2GB boş alan
  - CPU: 2 cores (4 cores önerilen)

### Docker Installation Verification
```bash
docker --version          # Docker 24.0.0+
docker-compose --version  # Docker Compose 2.17.0+
docker system info        # System information
```

## 🚀 Quick Start

### 1. Projeyi Klonlayın
```bash
git clone <repository-url>
cd cms-microservice
```

### 2. Environment Variables Oluşturun
```bash
# .env dosyası oluşturun (isteğe bağlı)
cp .env.example .env
```

### 3. Servisleri Başlatın
```bash
# Tüm servisleri arka planda başlat
docker-compose up -d

# İlerlemeyi takip et
docker-compose up --build
```

### 4. Sağlık Kontrolü
```bash
# Tüm container'ların durumunu kontrol et
docker-compose ps

# Health check endpoints
curl http://localhost:5000/health    # API Gateway
curl http://localhost:5001/health    # User Service  
curl http://localhost:5002/health    # Content Service
```

## 🏗️ Container Architecture

### Service Overview
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   API Gateway   │    │   User Service  │    │ Content Service │
│     Port: 5000  │    │     Port: 5001  │    │     Port: 5002  │
│   (Entry Point) │    │                 │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                    ┌─────────────────┐
                    │   PostgreSQL    │
                    │     Port: 5432  │
                    │   (Database)    │
                    └─────────────────┘
```

### Container Specifications

#### API Gateway Container
```yaml
apigateway:
  build:
    context: .
    dockerfile: docker/ApiGateway.Dockerfile
  ports:
    - "5000:5000"
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
    - ServiceUrls__UserService=http://userservice:5001
    - ServiceUrls__ContentService=http://contentservice:5002
  depends_on:
    - userservice
    - contentservice
  networks:
    - cms-network
```

#### User Service Container  
```yaml
userservice:
  build:
    context: .
    dockerfile: docker/UserService.Dockerfile
  ports:
    - "5001:5001"
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
    - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=UserServiceDb;Username=postgres;Password=postgres
  depends_on:
    - postgres
  networks:
    - cms-network
```

#### Content Service Container
```yaml
contentservice:
  build:
    context: .
    dockerfile: docker/ContentService.Dockerfile  
  ports:
    - "5002:5002"
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
    - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=ContentServiceDb;Username=postgres;Password=postgres
    - Services__UserService__BaseUrl=http://userservice:5001
  depends_on:
    - postgres
    - userservice
  networks:
    - cms-network
```

## 🌐 Network Configuration

### Default Network
Docker Compose otomatik olarak `cms-microservice_default` network'ü oluşturur.

### Custom Network Configuration
```yaml
networks:
  cms-network:
    driver: bridge
    ipam:
      driver: default
      config:
        - subnet: 172.20.0.0/16
```

### Service Discovery
Container'lar birbirleriyle service name üzerinden iletişim kurar:
- `apigateway` → `userservice:5001`
- `apigateway` → `contentservice:5002`  
- `contentservice` → `userservice:5001`
- `userservice` → `postgres:5432`
- `contentservice` → `postgres:5432`

## 💾 Volume Management

### Database Persistence
```yaml
volumes:
  postgres_data:
    driver: local
    
services:
  postgres:
    volumes:
      - postgres_data:/var/lib/postgresql/data
```

### Log Volume (İsteğe bağlı)
```yaml
volumes:
  app_logs:
    driver: local
    
services:
  apigateway:
    volumes:
      - app_logs:/app/logs
```

### Volume Commands
```bash
# Volume'ları listele
docker volume ls

# Volume detaylarını gör
docker volume inspect cms-microservice_postgres_data

# Volume'u temizle (DİKKAT: Veri silinir!)
docker volume rm cms-microservice_postgres_data
```

## 🔧 Environment Variables

### API Gateway Environment
```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5000
ServiceUrls__UserService=http://userservice:5001
ServiceUrls__ContentService=http://contentservice:5002
IpRateLimiting__EnableEndpointRateLimiting=true
```

### User Service Environment
```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5001
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=UserServiceDb;Username=postgres;Password=postgres
```

### Content Service Environment
```bash
ASPNETCORE_ENVIRONMENT=Development  
ASPNETCORE_URLS=http://+:5002
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=ContentServiceDb;Username=postgres;Password=postgres
Services__UserService__BaseUrl=http://userservice:5001
```

### PostgreSQL Environment
```bash
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=postgres
PGDATA=/var/lib/postgresql/data/pgdata
```

## 🏥 Health Checks

### Container Health Checks
Her servis için health check yapılandırması:

```dockerfile
# API Gateway Dockerfile
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1
```

### Health Check Monitoring
```bash
# Container health status
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# Detaylı health check logları
docker inspect cms_apigateway --format='{{json .State.Health}}'

# Health endpoint test
curl -f http://localhost:5000/health && echo "✅ Healthy" || echo "❌ Unhealthy"
```

### Custom Health Checks
```bash
# Script ile otomatik health check
#!/bin/bash
services=("apigateway:5000" "userservice:5001" "contentservice:5002")

for service in "${services[@]}"; do
  name=$(echo $service | cut -d: -f1)
  port=$(echo $service | cut -d: -f2)
  
  if curl -f http://localhost:$port/health > /dev/null 2>&1; then
    echo "✅ $name is healthy"
  else
    echo "❌ $name is unhealthy"  
  fi
done
```

## 📊 Monitoring

### Container Resource Usage
```bash
# Real-time resource monitoring
docker stats

# Specific container stats
docker stats cms_apigateway cms_userservice cms_contentservice

# Resource usage history
docker system df
```

### Log Monitoring
```bash
# Tüm servislerin logları (real-time)
docker-compose logs -f

# Spesifik servis logları
docker-compose logs -f apigateway

# Son 100 log satırı
docker-compose logs --tail=100 userservice

# Log filtering
docker-compose logs apigateway | grep "ERROR"
```

### Performance Metrics
```bash
# API Gateway metrics
curl http://localhost:5000/gateway/info

# Container inspection
docker inspect cms_apigateway --format='{{.Config.ExposedPorts}}'
docker inspect cms_apigateway --format='{{.NetworkSettings.Networks}}'
```

## 🔍 Troubleshooting

### Common Issues

#### 1. Port Already in Use  
```bash
# Port kullanımını kontrol et
lsof -i :5000

# Process'i sonlandır
kill -9 <PID>

# Alternatif port kullan
ASPNETCORE_URLS="http://localhost:5003" docker-compose up
```

#### 2. Database Connection Issues
```bash
# PostgreSQL container'ın çalıştığını kontrol et
docker-compose ps postgres

# Database logs
docker-compose logs postgres

# Manual database connection test
docker exec -it cms_postgres psql -U postgres -d postgres
```

#### 3. Network Connectivity
```bash
# Network'leri listele
docker network ls

# Network detayları
docker network inspect cms-microservice_default

# Container'dan network test
docker exec cms_apigateway ping userservice
```

#### 4. Container Debugging
```bash
# Container'a shell access
docker exec -it cms_apigateway /bin/bash

# Container file system
docker exec cms_apigateway ls -la /app

# Environment variables
docker exec cms_apigateway printenv

# Process list
docker exec cms_apigateway ps aux
```

### Debug Commands
```bash
# Container build debug
docker-compose build --no-cache --progress=plain

# Verbose startup
docker-compose up --build --verbose

# Service dependency issues
docker-compose up --remove-orphans

# Full system reset
docker-compose down -v
docker system prune -a -f
docker-compose up -d --build
```

## 🚀 Production Deployment

### Production docker-compose.yml
```yaml
version: '3.8'

services:
  apigateway:
    build:
      context: .
      dockerfile: src/ApiGateway/Dockerfile
      target: final
    ports:
      - "80:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5000
    restart: unless-stopped
    deploy:
      resources:
        limits:
          memory: 512M
          cpus: '0.5'
        reservations:
          memory: 256M
          cpus: '0.25'
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"
```

### Security Best Practices
```yaml
# secrets.yml (separate file)
services:
  postgres:
    environment:
      - POSTGRES_PASSWORD_FILE=/run/secrets/db_password
    secrets:
      - db_password

secrets:
  db_password:
    file: ./secrets/db_password.txt
```

### Scaling Configuration
```bash
# Scale specific services
docker-compose up -d --scale userservice=3

# Load balancer configuration
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### Backup Strategy
```bash
# Database backup
docker exec cms_postgres pg_dump -U postgres -d UserServiceDb > backup_user_$(date +%Y%m%d).sql

# Automated backup script
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
docker exec cms_postgres pg_dumpall -U postgres > "backup_all_$DATE.sql"
```

## 📚 Additional Resources

- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [PostgreSQL Docker Hub](https://hub.docker.com/_/postgres)

---

**Not**: Bu rehber sürekli güncellenmektedir. Production deployment öncesinde latest version'ı kontrol ediniz.
