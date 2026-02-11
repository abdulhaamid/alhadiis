# ASP.NET Core MVC ile Multi-Tenant Stock + Satış Takip SaaS Blueprint

Bu doküman, **25.000+ kullanıcı** ölçeğinde çalışan bir stock ve satış takip uygulamasını ASP.NET Core MVC + SQL Server ile SaaS olarak geliştirmek için başlangıç mimarisini özetler.

## 1) Hedef Mimari

- **Uygulama katmanı:** ASP.NET Core MVC (.NET 9)
- **Kimlik doğrulama:** ASP.NET Core Identity + JWT/Cookie (B2B için SSO opsiyonel)
- **Veri erişimi:** EF Core + SQL Server
- **Çok kiracılı yapı (multi-tenant):**
  - Tenant çözümleme: subdomain (`tenant1.app.com`) veya header tabanlı
  - `TenantId` temelli satır izolasyonu (global query filter)
  - Kurumsal tenant’lar için opsiyonel ayrı DB stratejisi
- **Önbellek:** Redis (tenant-aware cache key)
- **Arka plan işler:** Hangfire veya Quartz.NET
- **Mesajlaşma / event:** RabbitMQ veya Azure Service Bus (opsiyonel)
- **Dosya depolama:** Azure Blob / S3 uyumlu obje depolama

## 2) Çok Kiracılı Model Stratejisi

### A) Shared DB + Shared Schema (önerilen başlangıç)

- Tüm tablolarda `TenantId` zorunlu
- EF Core global filter: `HasQueryFilter(x => x.TenantId == _tenantContext.TenantId)`
- Avantaj: operasyon maliyeti düşük, hızlı geliştirme
- Dezavantaj: ileri düzey tenant bazlı performans tuning sınırlı

### B) Hybrid (büyümede önerilir)

- Küçük/orta tenant’lar shared DB
- Büyük tenant’lar dedicated DB
- Tenant metadata tablosunda `ConnectionStrategy` tutulur

## 3) Domain Modelleri (MVP)

- `Tenant`
- `AppUser` (Identity)
- `Product`
- `Warehouse`
- `StockTransaction` (In, Out, Transfer, Adjustment)
- `SalesOrder`
- `SalesOrderLine`
- `Customer`
- `Invoice`
- `AuditLog`

## 4) SQL Server Performans Kararları

- Her kritik tabloda `TenantId` + sorgu kolonlarına composite index
- `CreatedAt` üzerinden partition planı (yüksek transaction hacmi için)
- Sık raporlar için read model / materialized view yaklaşımı
- Transaction yoğun akışlarda optimistic concurrency (`rowversion`)
- Kritik sorgular için execution plan takibi + Query Store

## 5) Güvenlik

- Tenant boundary ihlallerine karşı servis katmanında ikinci doğrulama
- Role/claim bazlı yetkilendirme (`Admin`, `Sales`, `Warehouse`)
- Hassas alanlarda field-level encryption (gerekli ise)
- KVKK/GDPR için soft-delete + retention policy
- Audit trail: kim, ne zaman, hangi kaydı değiştirdi

## 6) Ölçekleme Yaklaşımı (25.000+ kullanıcı)

- Uygulama katmanı stateless, yatay ölçek (Kubernetes/App Service)
- Redis ile session/cache dağıtımı
- CQRS-lite: rapor sorgularını yazma trafiğinden ayırma
- Yoğun raporları async üretip kullanıcıya hazır bildirme
- CDN ile statik içerik dağıtımı

## 7) Operasyon ve İzleme

- Centralized log: Serilog + Elastic / Seq
- Metrics: Prometheus + Grafana veya Azure Monitor
- Distributed tracing: OpenTelemetry
- Uptime/SLA alarmları: API latency, error rate, DB DTU/CPU

## 8) MVP Sprint Planı (öneri)

1. Sprint 1: Tenant altyapısı + Identity + temel ürün/stok CRUD
2. Sprint 2: Satış siparişi + stok düşüm + fatura çekirdeği
3. Sprint 3: Raporlar + dashboard + audit log
4. Sprint 4: Performans tuning + cache + yük testleri
5. Sprint 5: Billing/abonelik + tenant onboarding self-service

## 9) Kodlama Prensipleri

- Tüm entity’lerde `TenantId`, `CreatedAt`, `UpdatedAt`
- Repository yerine doğrudan EF Core + temiz servis katmanı
- Transaction sınırları application service katmanında
- Idempotency: sipariş/ödeme gibi kritik endpoint’lerde zorunlu
- Test stratejisi:
  - Unit test: domain servisleri
  - Integration test: tenant izolasyonu + kritik transaction senaryoları
  - Load test: k6/JMeter ile eşzamanlı sipariş akışları

## 10) Bu repoda uygulanabilecek bir sonraki adım

- Mevcut projeye:
  - `Tenant` entity’si
  - `TenantContext` middleware
  - EF Core global query filters
  - `Product`, `StockTransaction`, `SalesOrder` modelleri
  - Basit bir MVC dashboard

Bu adımlarla teknik bir PoC hazırlanıp daha sonra production-grade hale genişletilebilir.
