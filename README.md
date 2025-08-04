# CMS Mikroservis Projesi

Bu proje, modern .NET 8 teknolojileri kullanılarak geliştirilmiş bir İçerik Yönetim Sistemi (CMS) mikroservis mimarisidir. Clean Architecture prensiplerine uygun olarak tasarlanmış ve production ortamında kullanıma hazır bir sistem sunar.

## 🏗️ Mimari

Proje, aşağıdaki bileşenlerden oluşmaktadır:

### 1. API Gateway
- **Port**: 5000
- **Teknoloji**: YARP (Yet Another Reverse Proxy) 2.3.0
- **Sorumluluklar**:
  - Tüm istek trafiğini yönetme ve yönlendirme
  - Mikroservisler arasında geçiş (routing)
  - JWT Authentication ve Authorization
  - Rate Limiting: IP-tabanlı hız sınırlama (dakikada 100, saatte 1000 istek)
  - Swagger Aggregation: Tüm mikroservislerin dökümantasyonunu tek yerde toplama
  - Health Checks: Kapsamlı sistem sağlık kontrolü
  - CORS yapılandırması
  - Security headers

### 2. Identity Service (Kimlik Doğrulama Servisi)
- **Port**: 5128
- **Veritabanı**: IdentityServiceDb (PostgreSQL) / In-Memory fallback
- **Sorumluluklar**:
  - Kullanıcı kayıt ve giriş işlemleri
  - JWT token oluşturma ve doğrulama
  - Refresh token yönetimi
  - Kullanıcı kimlik bilgileri saklama
  - Role-based authentication
  - Şifre güvenlik politikaları

### 3. User Service (Kullanıcı Servisi)
- **Port**: 5001
- **Veritabanı**: UserServiceDb (PostgreSQL) / In-Memory fallback
- **Sorumluluklar**:
  - Kullanıcı profil yönetimi
  - Kullanıcı bilgilerini detaylı olarak alma
  - Kullanıcı durumu yönetimi (Active, Inactive, Suspended, Pending)
  - Email uniqueness validation
  - JWT Authorization ile korumalı endpointler

### 4. Content Service (İçerik Servisi)
- **Port**: 5002
- **Veritabanı**: ContentServiceDb (PostgreSQL) / In-Memory fallback
- **Sorumluluklar**:
  - İçerik oluşturma, güncelleme, silme ve listeleme
  - İçerik yayınlama ve durumu yönetimi
  - Author validation (User Service ile entegrasyon)
  - SEO-friendly slug generation
  - İçerik kategorilendirme ve etiketleme
  - JWT Authorization ile korumalı endpointler

## 🛠️ Teknoloji Stack

- **.NET 8 LTS**: Modern C# özellikleri ve performans iyileştirmeleri
- **ASP.NET Core 8**: Web API framework
- **YARP 2.3.0**: .NET için hafif, yüksek performanslı reverse proxy
- **JWT Bearer Authentication**: JSON Web Token tabanlı kimlik doğrulama
- **ASP.NET Core Identity**: Kullanıcı kimlik yönetimi
- **Serilog**: Yapılandırılmış loglama ve dosya tabanlı log yönetimi
- **PostgreSQL**: Güçlü ve güvenilir ilişkisel veritabanı
- **Entity Framework Core 8**: ORM ve veritabanı yönetimi
- **MediatR**: CQRS pattern implementation
- **FluentValidation**: Model validation
- **Swagger/OpenAPI**: API documentation
- **Docker & Docker Compose**: Containerization
- **Health Checks**: Service monitoring
- **In-Memory Database**: Development ve test ortamları için fallback

## 🚀 Hızlı Başlangıç

### Ön Gereksinimler
- .NET 8 SDK
- PostgreSQL (opsiyonel - in-memory fallback mevcut)
- Docker ve Docker Compose (opsiyonel)

### Script ile Başlatma
```bash
# Tüm servisleri ayrı terminallerde başlat
./scripts/start.sh

# Sistem testini çalıştır
./scripts/test.sh

# Tüm servisleri durdur
./scripts/stop.sh
```

### Manuel Başlatma
```bash
# 1. Identity Service
cd src/IdentityService/IdentityService.API
dotnet run --urls="http://localhost:5128"

# 2. User Service
cd src/UserService/UserService.API
dotnet run --urls="http://localhost:5001"

# 3. Content Service
cd src/ContentService/ContentService.API
dotnet run --urls="http://localhost:5002"

# 4. API Gateway
cd src/ApiGateway
dotnet run --urls="http://localhost:5000"
```

### Docker ile Başlatma
```bash
# Production ortamı için tüm servisleri başlat
docker-compose up -d

# Development ortamı için başlat
docker-compose -f docker-compose.dev.yml up -d

# Specific service başlat
docker-compose up -d postgres identityservice

# Build ve başlat (değişiklikler sonrası)
docker-compose up -d --build

# Logları izle
docker-compose logs -f

# Specific service logları
docker-compose logs -f apigateway

# Servisleri durdur
docker-compose down

# Tüm volume'ları da sil
docker-compose down -v
```

## 📋 API Endpoints

### API Gateway (http://localhost:5000)
- **Swagger**: `/swagger`
- **Health Check**: `/status`
- **JWT Test**: `/jwt-test`
- **Protected Test**: `/protected`

### Identity Service Endpoints (via Gateway)
- **Register**: `POST /api/auth/register`
- **Login**: `POST /api/auth/login`
- **Public Test**: `GET /api/test/public`
- **Protected Test**: `GET /api/test/protected`

### User Service Endpoints (via Gateway)
- **Get All Users**: `GET /api/users`
- **Get User by ID**: `GET /api/users/{id}`
- **Create User**: `POST /api/users`
- **Update User**: `PUT /api/users/{id}`
- **Delete User**: `DELETE /api/users/{id}`

### Content Service Endpoints (via Gateway)
- **Get All Contents**: `GET /api/contents`
- **Get Content by ID**: `GET /api/contents/{id}`
- **Create Content**: `POST /api/contents`
- **Update Content**: `PUT /api/contents/{id}`
- **Delete Content**: `DELETE /api/contents/{id}`
- **Publish Content**: `POST /api/contents/{id}/publish`

## 🔐 Kimlik Doğrulama

### JWT Authentication Flow
1. **Kayıt**: `POST /api/auth/register`
2. **Giriş**: `POST /api/auth/login` - JWT token döner
3. **API Kullanımı**: `Authorization: Bearer {token}` header'ı ile

### Örnek Authentication
```bash
# Kullanıcı kayıt
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@cms.com",
    "password": "Admin123!",
    "confirmPassword": "Admin123!",
    "firstName": "Admin",
    "lastName": "User"
  }'

# Giriş yapma
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@cms.com",
    "password": "Admin123!"
  }'

# Token ile API kullanımı
curl -H "Authorization: Bearer {token}" \
  http://localhost:5000/api/users
```

## 🎯 Tasarım Desenleri ve Prensipler

### Clean Architecture
- **Core Layer**: Domain entities, interfaces, DTOs
- **Infrastructure Layer**: Data access, external services
- **API Layer**: Controllers, middlewares, configuration

### Repository Pattern
- Generic repository implementations
- Dependency injection
- Testable data access layer

## 🔧 Konfigürasyon

### JWT Konfigürasyonu
```json
{
  "JwtSettings": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong",
    "Issuer": "CMS-ApiGateway",
    "Audience": "CMS-Services",
    "ExpiryMinutes": 60
  }
}
```

### Database Konfigürasyonu
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=CmsDb;Username=postgres;Password=yourpassword"
  }
}
```

## 📊 Monitoring ve Health Checks

### Health Check Endpoints
- **API Gateway**: `http://localhost:5000/status`
- **Identity Service**: `http://localhost:5128/health`
- **User Service**: `http://localhost:5001/health`
- **Content Service**: `http://localhost:5002/health`

### Logging
Tüm servisler Serilog ile yapılandırılmış logging kullanır:
- Console output (development)
- File logging (production)
- Structured logging format
- Error tracking ve monitoring

## 🚀 Deployment

### Docker Compose ile Production Deployment
```bash
# Production deployment
docker-compose up -d

# Health check
curl http://localhost:5000/status
curl http://localhost:5128/health
curl http://localhost:5001/health
curl http://localhost:5002/health

# Logları kontrol et
docker-compose logs -f apigateway
```

### Manuel Production Deployment
```bash
# 1. Tüm servisleri build et
dotnet publish src/IdentityService/IdentityService.API -c Release -o deploy/identity
dotnet publish src/UserService/UserService.API -c Release -o deploy/user
dotnet publish src/ContentService/ContentService.API -c Release -o deploy/content
dotnet publish src/ApiGateway -c Release -o deploy/gateway

# 2. Production ortam değişkenleri
export ASPNETCORE_ENVIRONMENT=Production
export JWT_KEY=YourProductionSecretKey
export CONNECTION_STRING=YourProductionConnectionString

# 3. Servisleri başlat (reverse proxy arkasında)
systemctl start cms-identity
systemctl start cms-user
systemctl start cms-content
systemctl start cms-gateway
```

### Kubernetes Deployment (Opsiyonel)
```bash
# Kubernetes manifests oluştur
kubectl create namespace cms-microservice
kubectl apply -f k8s/
kubectl get pods -n cms-microservice
```

## 🧪 Testing

### Unit Tests
```bash
# Tüm testleri çalıştır
dotnet test

# Coverage raporu
dotnet test --collect:"XPlat Code Coverage"
```

### API Testing
API Gateway üzerinden tüm servislere erişim mümkündür:
- Swagger UI: `http://localhost:5000/swagger`
- Postman koleksiyonu mevcut
- Manuel test komutları README'de

## 🤝 Katkıda Bulunma

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için `LICENSE` dosyasına bakınız.
- **API Layer**: Controllers, middleware, configuration

### CQRS (Command Query Responsibility Segregation)
- Commands: Veri değiştirme işlemleri
- Queries: Veri okuma işlemleri
- MediatR ile implementation
- ValidationBehavior pipeline integration

### Smart Database Selection
- Automatic PostgreSQL connection testing at startup
- Graceful fallback to in-memory database when PostgreSQL unavailable
- Zero configuration required for development environments
- Production-ready database switching

### Repository Pattern
- Generic repository pattern
- Specific repository implementations
- Dependency injection

### Result Pattern
- Consistent error handling
- Type-safe operation results
- Failure and success states

### Validation Pattern
- FluentValidation integration
- Pipeline behavior for automatic validation
- Comprehensive error messaging
- RFC-compliant validations (email, phone numbers)

## 📊 API Endpoints

### API Gateway Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/status` | API Gateway durumunu kontrol et |
| GET | `/gateway/info` | Gateway hakkında detaylı bilgi |
| GET | `/health` | Tüm servislerin sağlık durumunu kontrol et |
| GET | `/` | Swagger UI (tüm mikroservisler için) |
| GET | `/proxy/userservice/swagger/v1/swagger.json` | User Service Swagger JSON |
| GET | `/proxy/contentservice/swagger/v1/swagger.json` | Content Service Swagger JSON |

### User Service Endpoints (Gateway üzerinden erişim)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/users` | Tüm kullanıcıları listele |
| POST | `/api/users` | Yeni kullanıcı oluştur |
| GET | `/api/users/{id}` | Kullanıcı detaylarını getir |
| PUT | `/api/users/{id}` | Kullanıcıyı güncelle |
| DELETE | `/api/users/{id}` | Kullanıcıyı sil |

### Content Service Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/contents` | Tüm içerikleri listele |
| POST | `/api/contents` | Yeni içerik oluştur |
| GET | `/api/contents/{id}` | İçerik detaylarını getir |
| PUT | `/api/contents/{id}` | İçeriği güncelle |
| DELETE | `/api/contents/{id}` | İçeriği sil |
| POST | `/api/contents/{id}/publish` | İçeriği yayınla |

## 🚀 Kurulum ve Çalıştırma

### Ön Gereksinimler
- .NET 9 SDK
- Docker Desktop (önerilen)
- PostgreSQL (isteğe bağlı - otomatik fallback mevcut)

### 1. Projeyi Klonlayın
```bash
git clone <repository-url>
cd cms-microservice
```

### 2. Docker ile Çalıştırma (Önerilen)

#### Tüm Servisleri Başlatma
```bash
# Tüm mikroservisler ve veritabanı
docker-compose up -d

# Container'ları listele ve durumu kontrol et
docker ps

# Servislerin health durumunu kontrol et
curl http://localhost:5000/health

# API Gateway loglarını takip et
docker logs cms_apigateway -f

# Tüm servislerin loglarını göster
docker-compose logs -f
```

#### Container Yönetimi
```bash
# Servisleri durdur
docker-compose down

# Volume'lar ile birlikte temizle
docker-compose down -v

# Rebuild (değişiklik sonrası)
docker-compose up -d --build

# Tek servisi yeniden başlat
docker-compose restart apigateway
```

#### Development Mode
```bash
# Sadece PostgreSQL (manuel development için)
docker-compose -f docker-compose.dev.yml up -d

# Development port'unda (5003) çalıştırmak için
export ASPNETCORE_URLS="http://localhost:5003"
cd src/ApiGateway
dotnet run
```

### 3. Manuel Çalıştırma

#### Veritabanını Başlatın
```bash
docker-compose -f docker-compose.dev.yml up -d postgres
```

#### API Gateway'i Çalıştırın
```bash
cd src/ApiGateway
dotnet run
```

#### User Service'i Çalıştırın (yeni terminal)
```bash
cd src/UserService/UserService.API
dotnet run
```

#### Content Service'i Çalıştırın (yeni terminal)
```bash
cd src/ContentService/ContentService.API
dotnet run
```

### 4. API'lere Erişim

#### Swagger UI (Birleştirilmiş Dökümantasyon)
- **API Gateway + Tüm Servisler**: `http://localhost:5000` (production)
- **Development Mode**: `http://localhost:5003` (manuel çalıştırma)
- **Özellikler**:
  - Tüm mikroservislerin API'leri tek sayfada
  - CORS proxy ile sorunsuz erişim
  - Try-it-out özelliği aktif
  - Request duration gösterimi
  - Real-time API testing

#### Doğrudan Servis Erişimi
- User Service: `http://localhost:5001/swagger`
- Content Service: `http://localhost:5002/swagger`

#### Gateway Monitoring
```bash
# API Gateway durumu ve özellikleri
curl http://localhost:5000/gateway/info

# Rate limiting test
for i in {1..5}; do curl http://localhost:5000/status; done

# Circuit breaker test (hatalı servis durumunda)
curl http://localhost:5000/api/users
```

#### Health Checks
```bash
# Tüm servislerin durumu
curl http://localhost:5000/health

# Tek servis durumu
curl http://localhost:5001/health  # User Service
curl http://localhost:5002/health  # Content Service
```

#### pgAdmin (Veritabanı Yönetimi)
- URL: `http://localhost:8080`
- Email: `admin@cms.com`
- Password: `admin123`

## 🔧 Yapılandırma

### Docker Multi-Stage Build
Her servis için optimize edilmiş Docker container'ları:

```dockerfile
# API Gateway Dockerfile örneği
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/ApiGateway/ApiGateway.csproj", "src/ApiGateway/"]
RUN dotnet restore "src/ApiGateway/ApiGateway.csproj"
COPY . .
WORKDIR "/src/src/ApiGateway"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
```

**Docker Özellikleri:**
- ✅ Multi-stage builds (optimize edilmiş image boyutu)
- ✅ Health checks (tüm servislerde)
- ✅ Network isolation
- ✅ Volume mounting
- ✅ Environment-based configuration
- ✅ Production-ready containers

### Akıllı Veritabanı Seçimi
Sistem, başlangıçta otomatik olarak PostgreSQL bağlantısını test eder ve duruma göre veritabanı sağlayıcısını seçer:

```csharp
// PostgreSQL mevcut ise
Console.WriteLine("Using PostgreSQL database for UserService");

// PostgreSQL mevcut değilse (sessiz geçiş)
Console.WriteLine("Using in-memory database for UserService");
```

**Özellikler:**
- ✅ **Sessiz Bağlantı Testi**: Connection error'lar loglanmaz
- ✅ **Otomatik Fallback**: PostgreSQL'den in-memory'ye geçiş
- ✅ **Zero Configuration**: Geliştirici müdahalesi gerektirmez
- ✅ **Production Ready**: Hem development hem production için optimize

### Veritabanı Bağlantı Strings
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=<DbName>;Username=postgres;Password=postgres"
  }
}
```

### Servis İletişimi
Content Service, User Service ile HTTP üzerinden iletişim kurar:
```json
{
  "Services": {
    "UserService": {
      "BaseUrl": "http://localhost:5001"
    }
  }
}
```

## 🧪 Test Çalıştırma

### Test Kapsamı
- **Toplam Test**: 192 test
- **UserService**: 71 test (%100 başarı)
- **ContentService**: 121 test (%100 başarı)
- **Başarı Oranı**: %100
- **Code Coverage**: %86.5 (industry standard %80'in üzerinde)
- **Test Süresi**: ~2.2 saniye

### Tüm Testleri Çalıştır
```bash
dotnet test
```

### Belirli Test Projesini Çalıştır
```bash
dotnet test tests/UserService.Tests
dotnet test tests/ContentService.Tests
```

### Test Detayları ile Çalıştır
```bash
dotnet test --verbosity detailed
```

### Coverage Raporu ile Test Çalıştırma
```bash
# Tüm testleri coverage ile çalıştır
dotnet test --collect:"XPlat Code Coverage" --results-directory:"TestResults/Full"

# Coverage raporu oluştur
reportgenerator -reports:"TestResults/Full/**/coverage.cobertura.xml" -targetdir:"TestResults/coverage-report" -reporttypes:"Html;TextSummary"

# Raporu görüntüle
open TestResults/coverage-report/index.html  # macOS
start TestResults/coverage-report/index.html # Windows
```

## 📖 API Dokümantasyonu

Servisler çalışırken Swagger UI'ya erişebilirsiniz:

- **User Service**: http://localhost:5001
- **Content Service**: http://localhost:5002
- **pgAdmin**: http://localhost:8080 (admin@cms.com / admin123)

## 🐳 Docker Komutları

### Container Yönetimi
```bash
# Tüm servisleri build et
docker-compose build

# Spesifik servisi build et
docker-compose build apigateway

# Build cache'siz rebuild
docker-compose build --no-cache

# Servisleri başlat
docker-compose up -d

# Servisleri durdur
docker-compose down

# Volume'lar ile birlikte durdur
docker-compose down -v
```

### Container Monitoring
```bash
# Tüm container logları
docker-compose logs -f

# Spesifik servis logları
docker-compose logs -f apigateway
docker-compose logs -f userservice
docker-compose logs -f contentservice

# Container resource kullanımı
docker stats

# Container'ların durumunu kontrol et
docker-compose ps
```

### Health Checks
```bash
# Docker container health status
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# Application health checks
curl http://localhost:5000/health
curl http://localhost:5001/health
curl http://localhost:5002/health
```

### Development Workflow
```bash
# Development sırasında değişiklikleri test etmek için
docker-compose up -d --build

# Sadece değişen servisi rebuild et
docker-compose up -d --build apigateway

# Cache temizliği
docker system prune -f
docker volume prune -f
```

### Troubleshooting
```bash
# Container'a bash ile bağlan
docker exec -it cms_apigateway /bin/bash

# Container içindeki dosyaları listele
docker exec cms_apigateway ls -la /app

# Environment variables kontrol et
docker exec cms_apigateway printenv

# Network bağlantılarını kontrol et
docker network ls
docker network inspect cms-microservice_default
```

## 📝 Logging

Serilog kullanılarak structured logging implementasyonu:
- Console output
- File logging (`logs/` dizini)
- JSON formatında log yapısı

## 🔒 Güvenlik Özellikleri

- **Comprehensive Input Validation**: FluentValidation ile detaylı doğrulama
- **ValidationBehavior Pipeline**: Otomatik request validation
- **Email Format Validation**: RFC 5321 uyumlu email doğrulama
- **Phone Number Validation**: Türkiye telefon numarası formatı
- **SQL Injection Prevention**: EF Core parametrized queries
- **Soft Delete Implementation**: Veri bütünlüğü koruması
- **CORS Configuration**: Cross-origin request yönetimi
- **Health Checks**: Sistem sağlık kontrolü
- **Exception Handling**: Merkezi hata yönetimi

## 🌐 Mikroservis İletişimi

Content Service, User Service ile HTTP client üzerinden senkron iletişim kurar:
- **Author Verification**: İçerik yazarının doğrulanması
- **User Information Retrieval**: Kullanıcı bilgilerinin alınması
- **Smart Database Fallback**: Veritabanı bağlantı hatalarında otomatik geçiş
- **Graceful Error Handling**: Hata durumlarında temiz yönetim
- **Connection Testing**: Başlangıçta bağlantı testleri
- **Timeout Configuration**: İstek zaman aşımı ayarları

## 📈 Performans Optimizasyonları

- EF Core query optimization
- Async/await pattern
- Connection pooling
- Index optimization
- Pagination support (ready for implementation)

## 🔄 CI/CD Ready

Proje aşağıdaki CI/CD araçları için hazır:
- GitHub Actions
- Azure DevOps
- Docker Hub
- Kubernetes deployment

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit yapın (`git commit -m 'Add amazing feature'`)
4. Push yapın (`git push origin feature/amazing-feature`)
5. Pull Request oluşturun

## 📄 Lisans

Bu proje [MIT License](LICENSE) altında lisanslanmıştır.

## 📞 İletişim

Sorularınız için lütfen issue açın veya pull request gönderin.

---

**Not**: Bu proje production ortamında kullanıma hazır olacak şekilde tasarlanmıştır. Güvenlik ve performans best practice'leri uygulanmıştır.