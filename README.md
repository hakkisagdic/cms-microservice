# CMS Mikroservis Projesi

Bu proje, modern .NET 8 teknolojileri kullanılarak geliştirilmiş bir İçerik Yönetim Sistemi (CMS) mikroservis mimarisidir. Clean Architecture prensiplerine uygun olarak tasarlanmış ve production ortamında kullanıma hazır bir sistem sunar.

## 🏗️ Mimari

Proje, aşağıdaki bileşenlerden oluşmaktadır:

### 1. API Gateway
- **Port**: 5003 (development), 5000 (production/docker)
- **Teknoloji**: YARP (Yet Another Reverse Proxy) 2.3.0
- **Sorumluluklar**:
  - Tüm istek trafiğini yönetme ve yönlendirme
  - Mikroservisler arasında geçiş (routing)
  - **Rate Limiting**: IP-tabanlı hız sınırlama (dakikada 100, saatte 1000 istek)
  - **Circuit Breaker**: Polly ile hata toleransı ve otomatik iyileşme
  - **Advanced Logging**: Korelasyon ID'leri ile yapılandırılmış loglama
  - **Swagger Aggregation**: Tüm mikroservislerin dökümantasyonunu tek yerde toplama
  - **CORS Proxy**: Mikroservis Swagger JSON'larını CORS sorunları olmadan sunma
  - **Health Checks**: Kapsamlı sistem sağlık kontrolü
  - **Request/Response Monitoring**: Detaylı performans izleme
  - **Resilience Patterns**: Retry, timeout ve circuit breaker patterns

### 2. User Service (Kullanıcı Servisi)
- **Port**: 5001
- **Veritabanı**: UserServiceDb (PostgreSQL) / In-Memory fallback
- **Sorumluluklar**:
  - Kullanıcı oluşturma, güncelleme, silme ve listeleme
  - Kullanıcı bilgilerini detaylı olarak alma
  - Kullanıcı durumu yönetimi (Active, Inactive, Suspended, Pending)
  - Email uniqueness validation
  - Comprehensive input validation with FluentValidation

### 3. Content Service (İçerik Servisi)
- **Port**: 5002
- **Veritabanı**: ContentServiceDb (PostgreSQL) / In-Memory fallback
- **Sorumluluklar**:
  - İçerik oluşturma, güncelleme, silme ve listeleme
  - İçerik yayınlama ve durumu yönetimi
  - İçerik arama ve filtreleme
  - User Service ile entegrasyon
  - SEO-friendly slug generation

## 🛠️ Teknoloji Stack

- **.NET 9**: Modern C# özellikleri ve performans iyileştirmeleri
- **ASP.NET Core 9**: Web API framework
- **YARP 2.3.0**: .NET için hafif, yüksek performanslı reverse proxy
- **AspNetCoreRateLimit**: IP-tabanlı hız sınırlama ve throttling
- **Polly**: Circuit breaker, retry patterns ve resilience
- **Serilog**: Yapılandırılmış loglama, korelasyon trackingve gelişmiş log enrichment
- **PostgreSQL**: Güçlü ve güvenilir ilişkisel veritabanı
- **Smart Database Selection**: Otomatik PostgreSQL/In-Memory veritabanı seçimi
- **Intelligent Connection Testing**: Başlangıçta sessiz bağlantı testleri
- **Graceful Database Fallback**: PostgreSQL'den in-memory'ye sorunsuz geçiş
- **Entity Framework Core 9**: ORM ve veritabanı yönetimi
- **MediatR**: CQRS pattern implementation with pipeline behaviors
- **FluentValidation**: Comprehensive model validation with pipeline integration
- **ValidationBehavior**: Automatic request validation pipeline
- **Swagger/OpenAPI**: API documentation with cross-service aggregation
- **Docker & Docker Compose**: Full containerization with multi-stage builds
- **Health Checks**: Comprehensive service monitoring
- **xUnit**: Unit testing framework (192 tests with 100% success rate)
- **Moq**: Mocking framework
- **FluentAssertions**: Test assertions

## 🎯 API Gateway Gelişmiş Özellikleri

### Rate Limiting & Throttling
```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      },
      {
        "Endpoint": "*",
        "Period": "1h", 
        "Limit": 1000
      }
    ]
  }
}
```
**Özellikler:**
- IP-tabanlı hız sınırlama
- Endpoint-specific limitler
- Sliding window algorithm
- Custom HTTP status codes
- Real-time monitoring

### Circuit Breaker Pattern
```csharp
// Polly ile circuit breaker configuration
CircuitBreakerPolicy
    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30)
    );
```
**Özellikler:**
- 5 başarısız istekten sonra devreye girer
- 30 saniye break süresi
- Otomatik recovery
- Detailed logging ve monitoring

### Advanced Logging & Monitoring
```csharp
// Correlation ID ile request tracking
Log.Information("Request {CorrelationId}: {Method} {Path} completed in {Duration}ms",
    correlationId, method, path, duration);
```
**Özellikler:**
- Correlation ID tracking
- Request/response duration monitoring
- Structured logging with Serilog
- Development mode'da header logging
- Production-ready log formatting

### Swagger Aggregation
**CORS Proxy Çözümü:**
- `/proxy/userservice/swagger/v1/swagger.json`
- `/proxy/contentservice/swagger/v1/swagger.json`
- Cross-origin requests için proxy endpoints
- Tek sayfada tüm mikroservis API'leri
- Real-time API testing capability

## 🎯 Tasarım Desenleri ve Prensipler

### Clean Architecture
- **Core Layer**: Domain entities, interfaces, DTOs
- **Infrastructure Layer**: Data access, external services
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