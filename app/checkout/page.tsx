"use client";

import Link from "next/link";
import { ArrowRight, Check, ShieldCheck } from "lucide-react";
import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useCart } from "@/components/store/cart-context";
import { formatCurrency } from "@/lib/utils";
import { createOrder, isApiBackedProduct } from "@/lib/storefront-api";

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
  const shipping = subtotal > 500 || subtotal === 0 ? 0 : 25;

  const submitOrder = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSubmitting(true);
    const orderNumber = `ELG-${Math.floor(100000 + Math.random() * 900000)}`;
    const formData = new FormData(event.currentTarget);
    const payload: DemoOrderPayload = {
      orderNumber,
      customerName: String(formData.get("name") ?? ""),
      phone: String(formData.get("phone") ?? ""),
      address: String(formData.get("address") ?? ""),
      city: String(formData.get("city") ?? ""),
      notes: String(formData.get("notes") ?? "") || undefined,
      subtotal,
      shipping,
      total: subtotal + shipping,
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
          customerName: payload.customerName,
          customerPhone: payload.phone,
          city: payload.city,
          address: payload.address,
          notes: payload.notes,
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

      // Demo mode is used only when NEXT_PUBLIC_API_URL is absent. It keeps
      // the visual prototype usable without introducing a second backend.
      window.localStorage.setItem("eleganza-last-order", JSON.stringify(payload));
      clearCart();
      router.push(`/success?order=${orderNumber}`);
    } catch {
      setSubmitting(false);
      window.alert("تعذر إنشاء الطلب. تأكدي من بيانات المنتج والـ API ثم حاولي مرة ثانية.");
    }
  };

  if (items.length === 0) {
    return <main className="mx-auto max-w-2xl px-5 py-24 text-center"><h1 className="font-display text-4xl text-primary">سلتك فاضية</h1><p className="mt-3 text-muted-foreground">أضيفي فستاناً قبل إتمام الطلب.</p><Button className="mt-7" asChild><Link href="/shop">العودة للمتجر</Link></Button></main>;
  }

  return (
    <main className="mx-auto max-w-7xl px-5 py-14 md:py-20"><Link href="/cart" className="mb-8 inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-primary"><ArrowRight className="h-4 w-4" /> العودة للسلة</Link><div className="mb-12"><p className="text-sm text-primary">الخطوة الأخيرة</p><h1 className="mt-3 font-display text-5xl text-primary">إتمام الطلب</h1></div><div className="grid gap-8 lg:grid-cols-[1fr_380px] lg:items-start"><Card className="p-6 md:p-8"><form onSubmit={submitOrder} className="space-y-7"><div><h2 className="font-display text-2xl text-primary">بيانات التوصيل</h2><p className="mt-1 text-sm text-muted-foreground">بنستخدم بياناتك لتأكيد الطلب والتوصيل فقط.</p></div><div className="grid gap-5 sm:grid-cols-2"><div className="space-y-2"><Label htmlFor="name">الاسم الكامل</Label><Input id="name" name="name" placeholder="مثال: سارة محمد" required /></div><div className="space-y-2"><Label htmlFor="phone">رقم الهاتف</Label><Input id="phone" name="phone" type="tel" placeholder="091 000 0000" required /></div><div className="space-y-2 sm:col-span-2"><Label htmlFor="address">العنوان بالتفصيل</Label><Input id="address" name="address" placeholder="المدينة، المنطقة، الشارع" required /></div><div className="space-y-2"><Label htmlFor="city">المدينة</Label><select id="city" name="city" required className="flex h-11 w-full rounded-2xl border bg-background px-4 text-sm outline-none focus:border-primary"><option value="">اختاري المدينة</option><option>طرابلس</option><option>بنغازي</option><option>مصراتة</option><option>الزاوية</option><option>سبها</option></select></div><div className="space-y-2"><Label htmlFor="notes">ملاحظات (اختياري)</Label><Input id="notes" name="notes" placeholder="وقت مناسب للتوصيل..." /></div></div><div><h2 className="font-display text-2xl text-primary">طريقة الدفع</h2><div className="mt-4 rounded-2xl border border-primary bg-primary/5 p-4"><div className="flex items-start gap-3"><span className="mt-0.5 flex h-5 w-5 items-center justify-center rounded-full bg-primary text-white"><Check className="h-3 w-3" /></span><div><p className="font-medium">الدفع عند الاستلام</p><p className="mt-1 text-xs text-muted-foreground">ادفعي نقداً عند وصول طلبك.</p></div></div></div></div><Button type="submit" size="lg" className="w-full" disabled={submitting}>{submitting ? "جاري تأكيد الطلب..." : "تأكيد الطلب"}</Button><p className="flex items-center justify-center gap-2 text-xs text-muted-foreground"><ShieldCheck className="h-4 w-4" /> بياناتك محفوظة معنا</p></form></Card><Card className="sticky top-28 p-6"><h2 className="font-display text-2xl text-primary">ملخص الطلب</h2><div className="mt-6 space-y-4">{items.map((item) => <div key={`${item.product.id}-${item.size}-${item.color}`} className="flex gap-3"><div className="h-16 w-12 shrink-0 overflow-hidden rounded-xl" style={{ backgroundColor: item.product.accent }}><img src={item.product.image} alt={item.product.name} className="h-full w-full object-cover" /></div><div className="min-w-0 flex-1"><p className="truncate text-sm font-medium">{item.product.name}</p><p className="mt-1 text-xs text-muted-foreground">{item.quantity} × {formatCurrency(item.product.price)}</p></div><p className="text-sm font-medium">{formatCurrency(item.product.price * item.quantity)}</p></div>)}<div className="border-t pt-4 text-sm"><div className="flex justify-between"><span className="text-muted-foreground">المجموع</span><span>{formatCurrency(subtotal)}</span></div><div className="mt-3 flex justify-between"><span className="text-muted-foreground">التوصيل</span><span>{shipping === 0 ? "مجاني" : formatCurrency(shipping)}</span></div><div className="mt-4 flex justify-between text-base font-medium"><span>الإجمالي</span><span className="text-primary">{formatCurrency(subtotal + shipping)}</span></div></div></div></Card></div></main>
  );
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
