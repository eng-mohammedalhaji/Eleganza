# إيليجانزا

منصة سوق عربية للفساتين مبنية بـ Next.js وASP.NET Core وPostgreSQL مع واجهة shadcn/ui.

## خارطة التنفيذ

الخطة التنفيذية، مدد الـ Sprints، ومعايير القبول موجودة في
[docs/SPRINTS.md](docs/SPRINTS.md).

الـ Backend الأساسي هو .NET 10 داخل solution مستقل:

```text
Eleganza.slnx
src/Eleganza.Api
src/Eleganza.Application
src/Eleganza.Domain
src/Eleganza.Infrastructure
src/Eleganza.Contracts
```

## التشغيل

يتطلب المشروع Node.js 18.18 أو أحدث.

```bash
pnpm install
```

في طرفية ثانية:

```bash
pnpm dev
```

ثم افتح `http://localhost:3000`.

## تشغيل Backend .NET محليًا

يتطلب .NET 10 وPostgreSQL. إذا كان Docker متاحًا:

```bash
docker compose up -d postgres
dotnet tool restore
dotnet ef database update \
  --project src/Eleganza.Infrastructure \
  --startup-project src/Eleganza.Api
dotnet run --project src/Eleganza.Api
```

يعمل فحص الصحة على `/health`.

الكتالوج والـ Checkout يتصلان بـ ASP.NET Core عبر `NEXT_PUBLIC_API_URL`. عند تشغيل البيئة التطويرية، يطبّق الـ API الـ migrations ويزرع كتالوجًا تجريبيًا تلقائيًا. تستخدم الواجهة `lib/products.ts` كـ fallback بصري فقط عندما لا يكون API URL مضبوطًا أو أثناء تطوير الواجهة دون Backend.

## المسارات

- `/` الصفحة الرئيسية
- `/shop` المتجر والفلترة والبحث
- `/shop/[slug]` تفاصيل المنتج
- `/cart` السلة
- `/checkout` إتمام الطلب
- `/admin` لوحة التحكم التجريبية

## قبل الإنتاج

1. ربط المصادقة والكتالوج والطلبات بـ ASP.NET Core API.
2. استبدال بيانات العرض ببيانات PostgreSQL عبر API وإنشاء seed للمنتجات.
3. إضافة بوابة الدفع أو تفعيل الدفع عند الاستلام حسب سياسة المتجر.
4. ربط زر واتساب ورقم خدمة العملاء الحقيقي.
5. إضافة حماية فعلية لمسار `/admin`.
