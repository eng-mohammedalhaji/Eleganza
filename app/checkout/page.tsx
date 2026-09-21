"use client";

import Link from "next/link";
import { ArrowRight, Check, ShieldCheck } from "lucide-react";
import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useCart } from "@/components/store/cart-context";
import { formatCurrency } from "@/lib/utils";
import { ApiClientError } from "@/lib/api-client";
import { createOrder, isApiBackedProduct } from "@/lib/storefront-api";
import {
  getVendorVanexCities,
  getVendorVanexSubCities,
  type VanexLocation,
} from "@/lib/vendor-api";

type DemoOrderPayload = {
  orderNumber: string;
  customerName: string;
  phone: string;
  address: string;
  city: string;
  notes?: string;
  subtotal: number;
  shipping: number;
  total: number;
  items: Array<{
    productName: string;
    price: number;
    quantity: number;
    size: string;
    color: string;
  }>;
};

function isApiConfigured() {
  return Boolean(process.env.NEXT_PUBLIC_API_URL);
}

export default function CheckoutPage() {
  return <CheckoutContent />;
}

function CheckoutContent() {
  const router = useRouter();
  const { items, subtotal, clearCart } = useCart();
  const [submitting, setSubmitting] = useState(false);
  const [cities, setCities] = useState<VanexLocation[]>([]);
  const [subCities, setSubCities] = useState<VanexLocation[]>([]);
  const [cityId, setCityId] = useState("");
  const [subCityId, setSubCityId] = useState("");
  const [locationsError, setLocationsError] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const vendorId = items[0]?.product.vendorId;
  const shipping = isApiConfigured() ? null : 0;

  useEffect(() => {
    if (!isApiConfigured() || !vendorId) return;

    let cancelled = false;
    getVendorVanexCities(vendorId)
      .then((result) => {
        if (cancelled) return;
        setCities(result);
        setLocationsError(null);
      })
      .catch(() => {
        if (!cancelled) setLocationsError("تعذر تحميل مدن التوصيل من Vanex.");
      });

    return () => {
      cancelled = true;
    };
  }, [vendorId]);

  useEffect(() => {
    if (!vendorId || !cityId || cities.length === 0) {
      setSubCities([]);
      setSubCityId("");
      return;
    }

    let cancelled = false;
    setSubCityId("");
    getVendorVanexSubCities(vendorId, cityId)
      .then((result) => {
        if (!cancelled) setSubCities(result);
      })
      .catch(() => {
        if (!cancelled) setLocationsError("تعذر تحميل المناطق التابعة للمدينة المختارة.");
      });

    return () => {
      cancelled = true;
    };
  }, [cities.length, cityId, vendorId]);

  const submitOrder = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSubmitting(true);
    setSubmitError(null);
    const orderNumber = `ELG-${Math.floor(100000 + Math.random() * 900000)}`;
    const formData = new FormData(event.currentTarget);
    const customerName = String(formData.get("name") ?? "");
    const phone = String(formData.get("phone") ?? "");
    const address = String(formData.get("address") ?? "");
    const city = String(formData.get("city") ?? "");
    const notes = String(formData.get("notes") ?? "") || undefined;
    const deliveryCityId = toPositiveInteger(formData.get("deliveryCityId"));
    const deliverySubCityId = toPositiveInteger(formData.get("deliverySubCityId"));
    const payload: DemoOrderPayload = {
      orderNumber,
      customerName,
      phone,
      address,
      city,
      notes,
      subtotal,
      shipping: shipping ?? 0,
      total: subtotal + (shipping ?? 0),
      items: items.map((item) => ({
        productName: item.product.name,
        price: item.product.price,
        quantity: item.quantity,
        size: item.size,
        color: item.color,
      })),
    };

    try {
      if (isApiConfigured()) {
        if (!items.every((item) => isApiBackedProduct(item.product, item.variantId))) {
          throw new Error("السلة تحتوي على منتج غير مرتبط بكتالوج المنصة.");
        }

        const created = await createOrder({
          customerName,
          customerPhone: phone,
          city,
          deliveryCityId,
          deliverySubCityId,
          address,
          notes,
          items: items.map((item) => ({
            productId: item.product.apiId!,
            variantId: item.variantId!,
            quantity: item.quantity,
          })),
        }, getIdempotencyKey());
        clearCart();
        window.sessionStorage.removeItem("eleganza-checkout-idempotency-key");
        router.push(`/success?order=${created.orderNumber}`);
        return;
      }

      window.localStorage.setItem("eleganza-last-order", JSON.stringify(payload));
      clearCart();
      router.push(`/success?order=${orderNumber}`);
    } catch (caught) {
      setSubmitting(false);
      setSubmitError(caught instanceof ApiClientError || caught instanceof Error
        ? caught.message
        : "تعذر إنشاء الطلب. راجعي البيانات وحاولي مرة ثانية.");
    }
  };

  if (items.length === 0) {
    return <main className="mx-auto max-w-2xl px-5 py-24 text-center"><h1 className="font-display text-4xl text-primary">سلتك فاضية</h1><p className="mt-3 text-muted-foreground">أضيفي فستاناً قبل إتمام الطلب.</p><Button className="mt-7" asChild><Link href="/shop">العودة للمتجر</Link></Button></main>;
  }

  const selectedCity = cities.find((city) => city.id === cityId);

  return (
    <main className="mx-auto max-w-7xl px-5 py-14 md:py-20">
      <Link href="/cart" className="mb-8 inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-primary"><ArrowRight className="h-4 w-4" /> العودة للسلة</Link>
      <div className="mb-12"><p className="text-sm text-primary">الخطوة الأخيرة</p><h1 className="mt-3 font-display text-5xl text-primary">إتمام الطلب</h1></div>
      <div className="grid gap-8 lg:grid-cols-[1fr_380px] lg:items-start">
        <Card className="p-6 md:p-8">
          <form onSubmit={submitOrder} className="space-y-7">
            <div><h2 className="font-display text-2xl text-primary">بيانات التوصيل</h2><p className="mt-1 text-sm text-muted-foreground">بنستخدم بياناتك لتأكيد الطلب والتوصيل فقط.</p></div>
            {locationsError && <p className="rounded-2xl bg-amber-50 p-4 text-sm text-amber-800">{locationsError} يجب ربط Vanex بحساب المورد قبل الإرسال.</p>}
            {submitError && <p className="rounded-2xl bg-red-50 p-4 text-sm text-red-700">{submitError}</p>}
            <div className="grid gap-5 sm:grid-cols-2">
              <div className="space-y-2"><Label htmlFor="name">الاسم الكامل</Label><Input id="name" name="name" placeholder="الاسم الكامل" required /></div>
              <div className="space-y-2"><Label htmlFor="phone">رقم الهاتف</Label><Input id="phone" name="phone" type="tel" placeholder="رقم الهاتف" required /></div>
              <div className="space-y-2 sm:col-span-2"><Label htmlFor="address">العنوان بالتفصيل</Label><Input id="address" name="address" placeholder="المنطقة، الشارع، أقرب علامة" required /></div>
              {cities.length > 0 ? <>
                <div className="space-y-2"><Label htmlFor="city">المدينة</Label><select id="city" value={cityId} onChange={(event) => setCityId(event.target.value)} required className="flex h-11 w-full rounded-2xl border bg-background px-4 text-sm outline-none focus:border-primary"><option value="">اختاري المدينة</option>{cities.map((city) => <option key={city.id} value={city.id}>{city.name}</option>)}</select><input type="hidden" name="city" value={selectedCity?.name ?? ""} /><input type="hidden" name="deliveryCityId" value={cityId} /></div>
                <div className="space-y-2"><Label htmlFor="subCity">المنطقة</Label><select id="subCity" value={subCityId} onChange={(event) => setSubCityId(event.target.value)} required className="flex h-11 w-full rounded-2xl border bg-background px-4 text-sm outline-none focus:border-primary"><option value="">اختاري المنطقة</option>{subCities.map((subCity) => <option key={subCity.id} value={subCity.id}>{subCity.name}</option>)}</select><input type="hidden" name="deliverySubCityId" value={subCityId} /></div>
              </> : <div className="space-y-2 sm:col-span-2"><Label htmlFor="city">المدينة</Label><Input id="city" name="city" placeholder="المدينة" required /><p className="text-xs text-muted-foreground">سيتم التحقق من معرفات Vanex عند ربط حساب المورد.</p></div>}
              <div className="space-y-2 sm:col-span-2"><Label htmlFor="notes">ملاحظات (اختياري)</Label><Input id="notes" name="notes" placeholder="وقت مناسب للتوصيل أو تفاصيل إضافية..." /></div>
            </div>
            <div><h2 className="font-display text-2xl text-primary">طريقة الدفع</h2><div className="mt-4 rounded-2xl border border-primary bg-primary/5 p-4"><div className="flex items-start gap-3"><span className="mt-0.5 flex h-5 w-5 items-center justify-center rounded-full bg-primary text-white"><Check className="h-3 w-3" /></span><div><p className="font-medium">الدفع عند الاستلام</p><p className="mt-1 text-xs text-muted-foreground">ادفعي نقداً عند وصول طلبك.</p></div></div></div></div>
            <Button type="submit" size="lg" className="w-full" disabled={submitting}>{submitting ? "جاري تأكيد الطلب..." : "تأكيد الطلب"}</Button><p className="flex items-center justify-center gap-2 text-xs text-muted-foreground"><ShieldCheck className="h-4 w-4" /> بياناتك محفوظة معنا</p>
          </form>
        </Card>
        <Card className="sticky top-28 p-6"><h2 className="font-display text-2xl text-primary">ملخص الطلب</h2><div className="mt-6 space-y-4">{items.map((item) => <div key={`${item.product.id}-${item.size}-${item.color}`} className="flex gap-3"><div className="h-16 w-12 shrink-0 overflow-hidden rounded-xl" style={{ backgroundColor: item.product.accent }}><img src={item.product.image} alt={item.product.name} className="h-full w-full object-cover" /></div><div className="min-w-0 flex-1"><p className="truncate text-sm font-medium">{item.product.name}</p><p className="mt-1 text-xs text-muted-foreground">{item.quantity} × {formatCurrency(item.product.price)}</p></div><p className="text-sm font-medium">{formatCurrency(item.product.price * item.quantity)}</p></div>)}<div className="border-t pt-4 text-sm"><div className="flex justify-between"><span className="text-muted-foreground">المجموع</span><span>{formatCurrency(subtotal)}</span></div><div className="mt-3 flex justify-between"><span className="text-muted-foreground">التوصيل</span><span>{shipping === null ? "يحدد من النظام" : "مجاني"}</span></div><div className="mt-4 flex justify-between text-base font-medium"><span>الإجمالي</span><span className="text-primary">{shipping === null ? "يحدد بعد التأكيد" : formatCurrency(subtotal + shipping)}</span></div></div></div></Card>
      </div>
    </main>
  );
}

function toPositiveInteger(value: FormDataEntryValue | null) {
  const parsed = Number(value);
  return Number.isInteger(parsed) && parsed > 0 ? parsed : undefined;
}

function getIdempotencyKey() {
  const storageKey = "eleganza-checkout-idempotency-key";
  const existing = window.sessionStorage.getItem(storageKey);
  if (existing) return existing;

  const key = typeof crypto.randomUUID === "function"
    ? crypto.randomUUID()
    : `checkout-${Date.now()}-${Math.random().toString(36).slice(2)}`;
  window.sessionStorage.setItem(storageKey, key);
  return key;
}
