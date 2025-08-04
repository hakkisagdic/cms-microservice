# API Referans Dokümantasyonu

Bu dokümant, CMS Mikroservis API'lerinin detaylı kullanım kılavuzudur.

## 📋 İçindekiler
1. [Genel Bilgiler](#genel-bilgiler)
2. [User Service API](#user-service-api)
3. [Content Service API](#content-service-api)
4. [Hata Kodları](#hata-kodları)
5. [Örnek Kullanım Senaryoları](#örnek-kullanım-senaryoları)

## 🌐 Genel Bilgiler

### Base URLs
- **User Service**: `http://localhost:5001` (Development)
- **Content Service**: `http://localhost:5002` (Development)

### Content Type
Tüm API istekleri ve yanıtları `application/json` formatındadır.

### Authentication
Şu anda authentication implementasyonu yoktur, ancak gelecekte JWT Bearer token implementasyonu planlanmaktadır.

### Response Format
Tüm API responses standart Result pattern kullanır:

```json
{
  "isSuccess": true,
  "value": { ... },
  "error": "",
  "errors": []
}
```

## 👤 User Service API

### Base URL: `/api/users`

---

### GET `/api/users`
Tüm kullanıcıları listeler.

**Response:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "phoneNumber": "1234567890",
    "dateOfBirth": "1990-01-01T00:00:00Z",
    "status": 1,
    "profileImageUrl": "https://example.com/profile.jpg",
    "bio": "Software Developer",
    "department": "IT",
    "position": "Senior Developer",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": null
  }
]
```

**Status Codes:**
- `200 OK`: Başarılı
- `500 Internal Server Error`: Sunucu hatası

---

### GET `/api/users/{id}`
Belirtilen ID'ye sahip kullanıcıyı getirir.

**Parameters:**
- `id` (path, required): Kullanıcı ID'si (GUID)

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "1234567890",
  "dateOfBirth": "1990-01-01T00:00:00Z",
  "status": 1,
  "profileImageUrl": "https://example.com/profile.jpg",
  "bio": "Software Developer",
  "department": "IT",
  "position": "Senior Developer",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

**Status Codes:**
- `200 OK`: Kullanıcı bulundu
- `404 Not Found`: Kullanıcı bulunamadı
- `500 Internal Server Error`: Sunucu hatası

---

### POST `/api/users`
Yeni kullanıcı oluşturur.

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "1234567890",
  "dateOfBirth": "1990-01-01T00:00:00Z",
  "profileImageUrl": "https://example.com/profile.jpg",
  "bio": "Software Developer",
  "department": "IT",
  "position": "Senior Developer"
}
```

**Validation Rules:**
- `firstName`: Required, max 50 characters
- `lastName`: Required, max 50 characters
- `email`: Required, valid email format, max 100 characters, unique
- `phoneNumber`: Optional, max 20 characters
- `dateOfBirth`: Optional, must be in the past
- `bio`: Optional, max 500 characters
- `department`: Optional, max 100 characters
- `position`: Optional, max 100 characters

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "1234567890",
  "dateOfBirth": "1990-01-01T00:00:00Z",
  "status": 1,
  "profileImageUrl": "https://example.com/profile.jpg",
  "bio": "Software Developer",
  "department": "IT",
  "position": "Senior Developer",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

**Status Codes:**
- `201 Created`: Kullanıcı oluşturuldu
- `400 Bad Request`: Validation hatası veya email zaten var
- `500 Internal Server Error`: Sunucu hatası

---

### PUT `/api/users/{id}`
Mevcut kullanıcıyı günceller.

**Parameters:**
- `id` (path, required): Kullanıcı ID'si (GUID)

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "1234567890",
  "dateOfBirth": "1990-01-01T00:00:00Z",
  "status": 1,
  "profileImageUrl": "https://example.com/profile.jpg",
  "bio": "Software Developer",
  "department": "IT",
  "position": "Senior Developer"
}
```

**User Status Values:**
- `1`: Active
- `2`: Inactive
- `3`: Suspended
- `4`: Pending

**Status Codes:**
- `200 OK`: Kullanıcı güncellendi
- `400 Bad Request`: Validation hatası
- `404 Not Found`: Kullanıcı bulunamadı
- `500 Internal Server Error`: Sunucu hatası

---

### DELETE `/api/users/{id}`
Kullanıcıyı siler (soft delete).

**Parameters:**
- `id` (path, required): Kullanıcı ID'si (GUID)

**Status Codes:**
- `204 No Content`: Kullanıcı silindi
- `404 Not Found`: Kullanıcı bulunamadı
- `500 Internal Server Error`: Sunucu hatası

## 📝 Content Service API

### Base URL: `/api/contents`

---

### GET `/api/contents`
Tüm içerikleri listeler.

**Response:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Sample Article",
    "body": "This is the article content...",
    "summary": "Article summary",
    "status": 2,
    "type": 1,
    "featuredImageUrl": "https://example.com/image.jpg",
    "metaTitle": "SEO Title",
    "metaDescription": "SEO Description",
    "tags": "tag1,tag2,tag3",
    "category": "Technology",
    "viewCount": 150,
    "publishedAt": "2024-01-01T00:00:00Z",
    "authorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "authorName": "John Doe",
    "slug": "sample-article",
    "sortOrder": 0,
    "isFeatured": true,
    "allowComments": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": null
  }
]
```

**Status Codes:**
- `200 OK`: Başarılı
- `500 Internal Server Error`: Sunucu hatası

---

### GET `/api/contents/{id}`
Belirtilen ID'ye sahip içeriği getirir.

**Parameters:**
- `id` (path, required): İçerik ID'si (GUID)

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Sample Article",
  "body": "This is the article content...",
  "summary": "Article summary",
  "status": 2,
  "type": 1,
  "featuredImageUrl": "https://example.com/image.jpg",
  "metaTitle": "SEO Title",
  "metaDescription": "SEO Description",
  "tags": "tag1,tag2,tag3",
  "category": "Technology",
  "viewCount": 150,
  "publishedAt": "2024-01-01T00:00:00Z",
  "authorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "authorName": "John Doe",
  "slug": "sample-article",
  "sortOrder": 0,
  "isFeatured": true,
  "allowComments": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

**Status Codes:**
- `200 OK`: İçerik bulundu
- `404 Not Found`: İçerik bulunamadı
- `500 Internal Server Error`: Sunucu hatası

---

### POST `/api/contents`
Yeni içerik oluşturur.

**Request Body:**
```json
{
  "title": "New Article",
  "body": "This is the article content...",
  "summary": "Article summary",
  "type": 1,
  "featuredImageUrl": "https://example.com/image.jpg",
  "metaTitle": "SEO Title",
  "metaDescription": "SEO Description",
  "tags": "tag1,tag2,tag3",
  "category": "Technology",
  "authorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "slug": "new-article",
  "sortOrder": 0,
  "isFeatured": false,
  "allowComments": true
}
```

**Content Type Values:**
- `1`: Article
- `2`: Page
- `3`: BlogPost
- `4`: News
- `5`: Event

**Validation Rules:**
- `title`: Required, max 200 characters
- `body`: Required
- `summary`: Optional, max 500 characters
- `type`: Required, valid enum value
- `authorId`: Required, must exist in User Service
- `slug`: Optional, unique if provided
- `tags`: Optional, comma-separated
- `category`: Optional, max 100 characters

**Status Codes:**
- `201 Created`: İçerik oluşturuldu
- `400 Bad Request`: Validation hatası veya author bulunamadı
- `500 Internal Server Error`: Sunucu hatası

---

### PUT `/api/contents/{id}`
Mevcut içeriği günceller.

**Parameters:**
- `id` (path, required): İçerik ID'si (GUID)

**Request Body:**
```json
{
  "title": "Updated Article",
  "body": "Updated content...",
  "summary": "Updated summary",
  "status": 2,
  "type": 1,
  "featuredImageUrl": "https://example.com/image.jpg",
  "metaTitle": "Updated SEO Title",
  "metaDescription": "Updated SEO Description",
  "tags": "tag1,tag2,tag3",
  "category": "Technology",
  "slug": "updated-article",
  "sortOrder": 0,
  "isFeatured": true,
  "allowComments": true
}
```

**Content Status Values:**
- `1`: Draft
- `2`: Published
- `3`: Archived
- `4`: Scheduled

**Status Codes:**
- `200 OK`: İçerik güncellendi
- `400 Bad Request`: Validation hatası
- `404 Not Found`: İçerik bulunamadı
- `500 Internal Server Error`: Sunucu hatası

---

### DELETE `/api/contents/{id}`
İçeriği siler (soft delete).

**Parameters:**
- `id` (path, required): İçerik ID'si (GUID)

**Status Codes:**
- `204 No Content`: İçerik silindi
- `404 Not Found`: İçerik bulunamadı
- `500 Internal Server Error`: Sunucu hatası

---

### POST `/api/contents/{id}/publish`
İçeriği yayınlar.

**Parameters:**
- `id` (path, required): İçerik ID'si (GUID)

**Request Body:**
```json
{
  "publishAt": "2024-01-01T00:00:00Z"
}
```

**Notes:**
- `publishAt` optional: Belirtilmezse şu anki zaman kullanılır
- Gelecek tarih belirtilirse içerik "Scheduled" durumuna geçer

**Status Codes:**
- `200 OK`: İçerik yayınlandı
- `400 Bad Request`: Validation hatası
- `404 Not Found`: İçerik bulunamadı
- `500 Internal Server Error`: Sunucu hatası

## ❌ Hata Kodları

### HTTP Status Codes

| Code | Description | Cause |
|------|-------------|-------|
| 200 | OK | Başarılı işlem |
| 201 | Created | Kaynak oluşturuldu |
| 204 | No Content | Başarılı silme işlemi |
| 400 | Bad Request | Validation hatası veya business rule violation |
| 404 | Not Found | Kaynak bulunamadı |
| 500 | Internal Server Error | Sunucu hatası |

### Error Response Format

```json
{
  "isSuccess": false,
  "value": null,
  "error": "User not found.",
  "errors": ["User not found."]
}
```

### Common Validation Errors

**User Service:**
- "First name is required."
- "Email is required."
- "Invalid email format."
- "A user with this email already exists."
- "Phone number cannot exceed 20 characters."

**Content Service:**
- "Title is required."
- "Body is required."
- "Author not found."
- "Invalid content type."
- "Slug already exists."

## 💡 Örnek Kullanım Senaryoları

### Senaryo 1: Yeni Blog Yazısı Oluşturma

1. **Kullanıcı Oluştur:**
```bash
curl -X POST http://localhost:5001/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Jane",
    "lastName": "Smith",
    "email": "jane.smith@example.com",
    "department": "Marketing",
    "position": "Content Writer"
  }'
```

2. **İçerik Oluştur:**
```bash
curl -X POST http://localhost:5002/api/contents \
  -H "Content-Type: application/json" \
  -d '{
    "title": "10 Tips for Better Content Writing",
    "body": "Content writing is an art...",
    "summary": "Learn the best practices for content writing",
    "type": 3,
    "category": "Marketing",
    "authorId": "USER_ID_FROM_STEP_1",
    "tags": "content,writing,marketing",
    "isFeatured": true
  }'
```

3. **İçeriği Yayınla:**
```bash
curl -X POST http://localhost:5002/api/contents/CONTENT_ID/publish \
  -H "Content-Type: application/json" \
  -d '{}'
```

### Senaryo 2: Kullanıcı Profili Güncelleme

```bash
curl -X PUT http://localhost:5001/api/users/USER_ID \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Jane",
    "lastName": "Smith",
    "email": "jane.smith@company.com",
    "phoneNumber": "+1234567890",
    "status": 1,
    "department": "Marketing",
    "position": "Senior Content Writer",
    "bio": "Experienced content writer with 5+ years in digital marketing"
  }'
```

### Senaryo 3: İçerik Arama ve Filtreleme

```bash
# Tüm yayınlanmış içerikleri getir
curl http://localhost:5002/api/contents

# Belirli bir yazarın içeriklerini getir (gelecekte eklenecek)
curl "http://localhost:5002/api/contents?authorId=USER_ID"

# Kategori bazlı filtreleme (gelecekte eklenecek)
curl "http://localhost:5002/api/contents?category=Technology"
```

## 🔧 Postman Collection

API'leri test etmek için Postman collection'ı aşağıdaki URL'den indirilebilir:

```json
{
  "info": {
    "name": "CMS Microservices API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "User Service",
      "item": [
        {
          "name": "Get All Users",
          "request": {
            "method": "GET",
            "url": "http://localhost:5001/api/users"
          }
        }
      ]
    }
  ]
}
```

## 📚 Gelecek API Geliştirmeleri

### Planlanan Endpoint'ler

**User Service:**
- `GET /api/users/search?q={term}` - Kullanıcı arama
- `GET /api/users?status={status}` - Duruma göre filtreleme
- `POST /api/users/{id}/activate` - Kullanıcı aktivasyonu
- `POST /api/users/{id}/suspend` - Kullanıcı askıya alma

**Content Service:**
- `GET /api/contents/published` - Sadece yayınlanmış içerikler
- `GET /api/contents/featured` - Öne çıkan içerikler
- `GET /api/contents/search?q={term}` - İçerik arama
- `GET /api/contents?category={category}` - Kategori filtresi
- `GET /api/contents?author={authorId}` - Yazar filtresi
- `POST /api/contents/{id}/archive` - İçerik arşivleme
- `POST /api/contents/{id}/duplicate` - İçerik kopyalama

### API Versioning

Gelecekte API versioning desteği eklenecek:
- Header-based: `Api-Version: 1.0`
- URL-based: `/api/v1/users`
- Query parameter: `/api/users?version=1.0`

---

Bu dokümantasyon, API'lerin mevcut durumunu yansıtmaktadır ve düzenli olarak güncellenecektir.
