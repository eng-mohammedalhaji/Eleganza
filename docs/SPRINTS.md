# Eleganza — خارطة تنفيذ المشروع

## الافتراضات

- النسخة الأولى بيع فقط، بدون تأجير.
- كل طلب في النسخة الأولى تابع لمورد واحد.
- الدفع عند الاستلام هو المسار الأساسي.
- كل مورد يربط حساب Vanex الخاص به عبر Access Token.
- الواجهة الحالية Next.js + React هي نقطة البداية، وسيتم ربطها تدريجيًا بـ ASP.NET Core API.
- لا يوجد Convex في هذه المنظومة، ولن يتم إدخاله لاحقًا؛ PostgreSQL هو مصدر الحقيقة الوحيد خلف الـ API.
- الفريق المفترض: مطور Backend، مطور Frontend، ومراجعة QA/تصميم بدوام جزئي.

المدة التقديرية للفريق أعلاه: **16–17 أسبوعًا** حتى الإطلاق الأول. لمطور واحد: **24–28 أسبوعًا** تقريبًا.

## التقنية القياسية

### Backend

- .NET 10 LTS.
- ASP.NET Core Web API.
- Modular Monolith + Clean Architecture + Vertical Slices.
- Entity Framework Core.
- PostgreSQL.
- ASP.NET Core Identity.
- OpenAPI/Swagger.
- FluentValidation أو Validators داخل Application حسب حجم الـ Feature.
- Outbox Pattern وBackground Worker بدل إدخال Message Broker مبكرًا.

### Frontend

- Next.js الموجود حاليًا.
- React وTypeScript.
- تصميم RTL وعربي من البداية.
- REST API typed contracts من OpenAPI.
- إدارة حالة السلة محليًا مع مصدر الحقيقة في السيرفر عند Checkout.

### التشغيل والاختبار

- Docker Compose أو .NET Aspire للتطوير المحلي.
- GitHub Actions أو أي CI/CD مكافئ.
- xUnit.
- FluentAssertions.
- Testcontainers لـ PostgreSQL.
- WireMock.Net لمحاكاة Vanex.
- OpenTelemetry وHealth Checks.
- Structured Logging بدون تسجيل Access Tokens أو بيانات حساسة.

## قواعد معمارية لا تتغير

1. الـ Frontend لا يتصل بقاعدة البيانات مباشرة.
2. السعر والإجمالي والعمولة تحسب في الـ Backend فقط.
3. كل منتج تابع لمورد واحد.
4. كل مورد يرى بياناته وطلباته فقط.
5. كل تغيير في حالة الطلب له Transition واضح وصلاحية محددة.
6. إنشاء الطلب المحلي يسبق الإرسال إلى Vanex.
7. الإرسال إلى Vanex يتم بالخلفية مع Retry وIdempotency.
8. لا نحذف السجلات التي دخلت في طلبات؛ نستخدم Archive/Soft Delete.
9. Access Token الخاص بالمورد لا يصل للواجهة ولا يظهر في Logs.
10. أي تكامل خارجي يمر عبر Interface/Adapter مستقل.

---

## Sprint 0 — تثبيت الرؤية والتحليل

**المدة:** 3 أيام عمل

### الهدف

تحويل الفكرة إلى قرارات مكتوبة قبل بناء الكود.

### التقنيات والأدوات

- Git وBranching Strategy.
- OpenAPI contract draft.
- ERD/Domain Map.
- ADRs للقرارات المعمارية.

### المخرجات

- تعريف المستخدمين: Customer، Vendor Owner، Vendor Staff، Admin.
- تحديد حالات المورد والمنتج والطلب والشحن.
- تعريف سياسة العمولة.
- تعريف سياسة الإلغاء.
- تحديد بيانات طلب Vanex.
- تثبيت قرار: مورد واحد لكل سلة.
- قائمة الحقول المطلوبة من Vanex:
  - Base URL.
  - Endpoint إنشاء الطلب.
  - Endpoint الإلغاء.
  - Endpoint التتبع.
  - Webhook أو Polling.
  - طريقة إرسال Bearer Token.
  - بيئة اختبار.

### معيار القبول

- لا توجد قاعدة تجارية رئيسية غير موثقة.
- يستطيع الفريق شرح دورة الطلب كاملة من Checkout إلى Delivered.
- يتم الحصول على Swagger/Postman من Vanex قبل تنفيذ Adapter النهائي.

---

## Sprint 1 — تأسيس الحل والبنية التحتية

**المدة:** أسبوع واحد

### الهدف

إنشاء أساس .NET قابل للنمو، مع استمرار واجهة Next.js في العمل أثناء ربطها بالـ API.

### التقنيات

- .NET 10.
- ASP.NET Core Web API.
- PostgreSQL.
- EF Core migrations.
- Docker Compose أو Aspire للتطوير.
- OpenAPI.
- Health Checks.
- CI لتشغيل Build وTypecheck وTests.

### المخرجات

```text
src/
  Eleganza.Api
  Eleganza.Application
  Eleganza.Domain
  Eleganza.Infrastructure
  Eleganza.Contracts

tests/
  Eleganza.UnitTests
  Eleganza.IntegrationTests
```

- إعداد Configuration لكل بيئة.
- إعداد قاعدة البيانات.
- أول Migration.
- Global Error Handling.
- ProblemDetails responses.
- Correlation ID لكل Request.
- `/health` و`/health/ready`.
- إعداد CORS للواجهة.
- إعداد `dotnet format` وAnalyzers.

### معيار القبول

- تشغيل API وقاعدة البيانات بأمر واحد.
- Build ناجح في CI.
- Endpoint تجريبي محمي وغير محمي يعمل.
- لا توجد Connection Strings أو Secrets داخل Git.

---

## Sprint 2 — الهوية والموردون والصلاحيات

**المدة:** أسبوع ونصف

### الهدف

بناء الحسابات، الموردين، والصلاحيات متعددة الأطراف.

### التقنيات

- ASP.NET Core Identity.
- JWT Access Token مع Refresh Token آمن، أو HttpOnly Cookies للويب.
- Role/Policy Authorization.
- EF Core.
- Rate Limiting لمحاولات الدخول.

### المخرجات

- التسجيل وتسجيل الدخول.
- تأكيد الهاتف أو البريد حسب القرار التجاري.
- نسيان كلمة المرور.
- إنشاء Vendor Profile.
- طلب اعتماد المورد.
- حالات المورد:

```text
Pending → UnderReview → Approved
                    ↘ Rejected
Approved → Suspended
```

- أدوار:

```text
Customer
VendorOwner
VendorStaff
Admin
```

- Ownership checks على كل مورد ومنتج وطلب.
- Audit Log لعمليات الإدارة.

### معيار القبول

- المورد لا يصل إلى بيانات مورد آخر.
- الزبون لا يدخل لوحة الإدارة أو المورد.
- Admin يستطيع اعتماد أو إيقاف المورد.
- توجد اختبارات سلبية للصلاحيات.

---

## Sprint 3 — الكتالوج والمنتجات والمخزون

**المدة:** أسبوع ونصف

### الهدف

بناء المنتجات بشكل صحيح حسب المورد والمقاس واللون والكمية.

### التقنيات

- EF Core relationships وIndexes.
- Object Storage للصور عبر `IFileStorage` abstraction.
- Image validation وsize limits.
- Slugs وSEO metadata.

### المخرجات

الجداول الأساسية:

```text
Vendor
Store
Category
Product
ProductVariant
ProductMedia
InventoryAdjustment
```

- المنتج تابع لمورد واحد.
- المنتج يمر بالمراحل:

```text
Draft → PendingApproval → Published
                         ↘ Rejected
Published → Paused → Archived
```

- المقاس واللون والكمية في `ProductVariant`، وليس Arrays عامة.
- رفع صور آمن.
- مراجعة المنتج من Admin.
- تعديل المخزون بسجل حركة.
- عدم حذف منتج دخل في طلب سابق.

### معيار القبول

- المورد يدير منتجاته فقط.
- لا يظهر المنتج للزبائن قبل النشر.
- السعر والمخزون مرتبطان بالـ Variant الصحيح.
- اختبارات منع التعديل غير المصرح به موجودة.

---

## Sprint 4 — واجهة المتجر والبحث

**المدة:** أسبوع ونصف

### الهدف

ربط واجهة Next.js بالـ API وعرض منتجات حقيقية.

### التقنيات

- Next.js الموجود.
- TypeScript API client.
- OpenAPI-generated types أو Contracts يدوية مؤقتًا.
- Server-side rendering للصفحات العامة.
- RTL وSEO.

### المخرجات

- الصفحة الرئيسية.
- قائمة المنتجات.
- صفحة تفاصيل المنتج.
- صفحة متجر المورد.
- البحث.
- الفلترة حسب:
  - المدينة.
  - التصنيف.
  - السعر.
  - المقاس.
  - اللون.
- حالات Loading وEmpty وError.
- إزالة الاعتماد على `lib/products.ts` في المسار الأساسي.

### معيار القبول

- الزبون يرى المنتجات المنشورة من API.
- لا يظهر منتج موقوف أو غير معتمد.
- الروابط والـ Slugs تعمل بعد Refresh مباشر.
- الواجهة متوافقة مع الهاتف والعربية.

---

## Sprint 5 — السلة والتسعير وCheckout

**المدة:** أسبوع ونصف

### الهدف

إنشاء طلب بيع آمن، بمورد واحد، وحسابات صحيحة.

### التقنيات

- Server-side Cart validation.
- Decimal Money calculations.
- Idempotency Key.
- Transaction boundaries.
- FluentValidation.

### المخرجات

- إضافة وحذف وتعديل عناصر السلة.
- منع خلط موردين في السلة.
- التحقق من الكمية.
- حفظ عنوان الزبون ورقم الهاتف.
- حساب:

```text
Subtotal
+ ShippingFee
- Discount
= Total
```

- الدفع عند الاستلام.
- إنشاء Order وOrderItems.
- حفظ Snapshot للاسم والسعر والكمية.
- منع إرسال السعر النهائي من الواجهة كمصدر ثقة.
- منع إنشاء طلب مكرر عند إعادة إرسال Checkout.

### معيار القبول

- لا يمكن تعديل السعر من أدوات المتصفح.
- لا يمكن شراء كمية غير متاحة.
- الطلب يتكرر مرة واحدة فقط عند إعادة الطلب بنفس Idempotency Key.
- الطلب محفوظ قبل أي اتصال خارجي.

---

## Sprint 6 — دورة الطلب والتشغيل الداخلي

**المدة:** أسبوع ونصف

### الهدف

تطبيق State Machine حقيقية للطلبات.

### التقنيات

- Domain Services.
- State Transition validators.
- Order History.
- Outbox Messages.

### حالات الطلب

```text
Pending
Confirmed
Rejected
Cancelled
Completed
```

### حالات الشحن

```text
NotSubmitted
Submitting
Submitted
Accepted
OutForDelivery
Delivered
Failed
Cancelled
```

### المخرجات

- تأكيد الطلب من المورد.
- رفض الطلب مع سبب.
- إلغاء من الزبون حسب الحالة.
- سجل `OrderStatusHistory`.
- صلاحيات منفصلة للزبون والمورد والإدارة.
- أوامر واضحة:

```text
ConfirmOrder
RejectOrder
CancelOrder
MarkCompleted
RetryShippingSubmission
```

### معيار القبول

- لا يوجد تغيير حالة عشوائي.
- كل انتقال يسجل المستخدم والوقت والسبب.
- لا يستطيع المورد إكمال طلب غير تابع له.
- الطلبات القديمة لا تتأثر بتعديل المنتج.

---

## Sprint 7 — تكامل Vanex

**المدة:** أسبوعان

### الهدف

إرسال طلب كل مورد إلى حساب Vanex الخاص به، مع إعادة المحاولة والتتبع.

### التقنيات

- `IShippingProvider` abstraction.
- `VanexShippingProvider` adapter.
- HttpClientFactory.
- Polly أو Resilience Pipelines للـ Retry/Timeout.
- Outbox Worker.
- WireMock.Net للاختبار.
- Secret Manager/Key Vault أو تشفير آمن للـ Token.

### الجداول

```text
VendorShippingAccount
Shipment
ShippingIntegrationAttempt
OutboxMessage
WebhookEvent
```

### التدفق

```text
Order Created
→ Outbox Message
→ SubmitToVanex Worker
→ Vanex Response
→ Save ExternalOrderId
→ Update Shipment Status
```

### قواعد مهمة

- لا يتم إرسال Access Token للواجهة.
- لا يتم وضع Access Token في Logs.
- `OrderNumber` يستخدم كـ Idempotency Key إن دعمته Vanex.
- الطلب الفاشل يعاد إرساله تلقائيًا.
- الفشل لا يحذف الطلب المحلي.
- إذا لم يوجد Token صالح، يظهر الطلب في قائمة تحتاج تدخلًا.
- Webhook يجب أن يكون Idempotent.

### معيار القبول

- يمكن للمورد ربط وفصل حساب Vanex.
- يمكن اختبار Token قبل تفعيله.
- طلب ناجح ينتج رقم شحنة Vanex محفوظًا محليًا.
- طلب فاشل يظهر سبب الفشل مع إمكانية Retry.
- الاختبارات تعمل دون الاتصال الحقيقي بـ Vanex.
- اختبار Sandbox حقيقي ينجح قبل الإنتاج.

---

## Sprint 8 — لوحة الإدارة والعمولات

**المدة:** أسبوع واحد

### الهدف

إدارة المنصة من مكان واحد.

### التقنيات

- Admin Policies.
- Server-side pagination/filtering.
- Audit Logs.
- Export CSV عند الحاجة.

### المخرجات

- إدارة الموردين.
- اعتماد المنتجات.
- إدارة الطلبات.
- إدارة حالات Vanex الفاشلة.
- إعداد العمولة العامة.
- إمكانية تخصيص العمولة لمورد.
- Dashboard أساسي:
  - عدد الطلبات.
  - الطلبات الفاشلة.
  - إجمالي المبيعات.
  - عمولة المنصة.
  - مستحقات الموردين.

### معيار القبول

- كل عمليات Admin مسجلة.
- لا توجد شاشات إدارة تستدعي بيانات ضخمة بلا Pagination.
- لا يستطيع Vendor الوصول إلى Admin APIs.

---

## Sprint 9 — الإشعارات والتقييمات والدعم

**المدة:** أسبوع ونصف

### الهدف

تحسين التشغيل والثقة بعد اكتمال الطلب.

### التقنيات

- Notification abstraction.
- Email/SMS provider حسب المتاح.
- Background Jobs.
- Moderation rules.

### المخرجات

- إشعار إنشاء الطلب.
- إشعار تأكيد المورد.
- إشعار خروج الطلب للتوصيل.
- إشعار التسليم.
- تقييم المنتج والمورد بعد `Completed` فقط.
- فتح شكوى مرتبطة بالطلب.
- حالات الشكوى:

```text
Opened
UnderReview
Resolved
Closed
```

### معيار القبول

- الإشعارات لا تعطل إنشاء الطلب.
- لا يمكن إرسال تقييم بدون طلب مكتمل.
- لا يرسل النظام الإشعار نفسه عدة مرات لنفس الحدث.

---

## Sprint 10 — الجودة والأمان والاستعداد للإنتاج

**المدة:** أسبوع ونصف

### الهدف

تحويل النظام من Prototype إلى Release قابل للاستخدام الحقيقي.

### التقنيات

- Unit Tests.
- Integration Tests.
- Testcontainers.
- Playwright أو اختبارات E2E مكافئة.
- OpenTelemetry.
- Dependency scanning.
- Rate limiting.
- Security Headers.
- Backup/Restore.

### الاختبارات الإلزامية

- حساب السعر.
- منع شراء كمية غير متاحة.
- منع مورد من الوصول لبيانات مورد آخر.
- انتقالات الطلب.
- إلغاء الطلب.
- Retry مع Vanex.
- عدم تكرار الشحنة.
- Webhook مكرر.
- Token غير صالح.
- فشل قاعدة البيانات أو Vanex.

### معيار القبول

- لا توجد أخطاء حرجة مفتوحة.
- CI يشغل Build وTests وLint.
- يوجد Staging مستقل.
- يوجد Backup وتجربة Restore.
- الأسرار خارج Git.
- Logs وMetrics وTraces قابلة للمراجعة.

---

## Sprint 11 — الإطلاق التجريبي ثم الإنتاج

**المدة:** أسبوع ونصف

### الهدف

إطلاق محدود مع موردين حقيقيين قبل التوسع.

### المخرجات

- بيئة Production.
- Domain وHTTPS.
- إعداد النسخ الاحتياطية.
- مراقبة الأخطاء.
- Runbook للتشغيل.
- تدريب 3–5 موردين.
- اختبار 20–50 طلبًا تجريبيًا.
- مراجعة مشاكل Vanex.
- خطة Rollback.

### معيار القبول

- 3 موردين حقيقيين يستطيعون ربط Vanex وإضافة المنتجات.
- الزبون يستطيع تنفيذ طلب كامل.
- الطلب يصل إلى Vanex دون تدخل يدوي.
- يمكن تتبع الطلب حتى التسليم.
- يمكن التعامل مع فشل Vanex يدويًا من لوحة الإدارة.
- لا يتم التوسع قبل نجاح الـ Pilot.

---

## ترتيب الإصدارات

### Internal Demo

بعد Sprint 4:

- مستخدمون.
- موردون.
- منتجات.
- بحث.
- واجهة المتجر.

### MVP Pilot

بعد Sprint 7:

- Checkout.
- طلبات.
- دفع عند الاستلام.
- Vanex.
- تتبع أساسي.

### Production v1

بعد Sprint 11:

- Admin.
- عمولات.
- إشعارات.
- تقييمات.
- اختبارات.
- مراقبة.
- Backup.

## ما يؤجل بعد الإطلاق

- التأجير.
- السلة متعددة الموردين.
- الدفع الإلكتروني.
- التطبيق Native للهاتف.
- الدردشة الفورية.
- Microservices.
- نظام توصيات متقدم.
- مخازن متعددة للمورد الواحد.

## تعريف اكتمال أي Sprint

لا يعتبر الـ Sprint منتهيًا بمجرد كتابة الكود. يجب أن يحتوي على:

1. Migration أو Contract عند الحاجة.
2. Unit/Integration Tests للمنطق الجديد.
3. صلاحيات واختبارات سلبية.
4. واجهة مستخدم لحالات النجاح والفشل والتحميل.
5. Logging بدون بيانات حساسة.
6. توثيق API أو القرار التجاري.
7. مراجعة Code Review.
8. Demo داخلي.
9. تحديث README أو Runbook.
