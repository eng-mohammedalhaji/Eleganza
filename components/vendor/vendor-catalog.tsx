"use client";

import { FormEvent, useEffect, useState } from "react";
import { ApiClientError } from "@/lib/api-client";
import {
  addProductMedia,
  addProductVariant,
  createVendorProduct,
  getVendorCategories,
  getVendorProducts,
  submitVendorProduct,
  type VendorCategory,
  type VendorProduct,
} from "@/lib/vendor-api";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

const productStatus: Record<string, string> = {
  Draft: "مسودة",
  PendingApproval: "بانتظار المراجعة",
  Published: "منشور",
  Rejected: "مرفوض",
  Paused: "متوقف",
  Archived: "مؤرشف",
};

export function VendorCatalog() {
  const [categories, setCategories] = useState<VendorCategory[]>([]);
  const [products, setProducts] = useState<VendorProduct[]>([]);
  const [form, setForm] = useState({ categoryId: "", name: "", slug: "", description: "", size: "", color: "", sku: "", price: "", stock: "", imageUrl: "" });
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const load = async () => {
    try {
      const [availableCategories, ownProducts] = await Promise.all([getVendorCategories(), getVendorProducts()]);
      setCategories(availableCategories);
      setProducts(ownProducts);
      setError(null);
    } catch (caught) {
      setError(caught instanceof ApiClientError || caught instanceof Error ? caught.message : "تعذر تحميل كتالوج المتجر.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void load();
  }, []);

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);
    setSuccess(null);
    try {
      const product = await createVendorProduct({
        categoryId: form.categoryId,
        name: form.name,
        slug: form.slug,
        description: form.description,
      });
      await addProductVariant(product.id, {
        size: form.size,
        color: form.color,
        sku: form.sku,
        price: Number(form.price),
        stock: Number(form.stock),
      });
      if (form.imageUrl.trim()) {
        await addProductMedia(product.id, { storageKey: form.imageUrl.trim(), altText: form.name, sortOrder: 0 });
      }
      await submitVendorProduct(product.id);
      setForm({ categoryId: "", name: "", slug: "", description: "", size: "", color: "", sku: "", price: "", stock: "", imageUrl: "" });
      setSuccess("تم إرسال المنتج للمراجعة.");
      setLoading(true);
      await load();
    } catch (caught) {
      setError(caught instanceof ApiClientError || caught instanceof Error ? caught.message : "تعذر حفظ المنتج.");
    } finally {
      setSubmitting(false);
    }
  };

  return <section className="mt-8 grid gap-7 lg:grid-cols-[1fr_1.1fr]">
    <Card className="p-6 md:p-8"><h2 className="font-display text-2xl text-primary">إضافة منتج</h2><p className="mt-2 text-sm leading-7 text-muted-foreground">المنتج يمر بالمراجعة قبل ظهوره للزبائن.</p>{error && <p className="mt-5 rounded-2xl bg-red-50 p-4 text-sm text-red-700">{error}</p>}{success && <p className="mt-5 rounded-2xl bg-emerald-50 p-4 text-sm text-emerald-700">{success}</p>}<form onSubmit={submit} className="mt-6 space-y-4"><div className="space-y-2"><Label htmlFor="productCategory">التصنيف</Label><select id="productCategory" value={form.categoryId} onChange={(event) => setForm({ ...form, categoryId: event.target.value })} required className="flex h-11 w-full rounded-2xl border bg-background px-4 text-sm"><option value="">اختاري التصنيف</option>{categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}</select></div><div className="grid gap-4 sm:grid-cols-2"><div className="space-y-2"><Label htmlFor="productName">اسم المنتج</Label><Input id="productName" value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} required /></div><div className="space-y-2"><Label htmlFor="productSlug">الرابط المختصر</Label><Input id="productSlug" value={form.slug} onChange={(event) => setForm({ ...form, slug: event.target.value })} required /></div></div><div className="space-y-2"><Label htmlFor="productDescription">الوصف</Label><textarea id="productDescription" value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} required minLength={2} className="min-h-24 w-full rounded-2xl border bg-background px-4 py-3 text-sm" /></div><div className="grid gap-4 sm:grid-cols-2"><div className="space-y-2"><Label htmlFor="productSize">المقاس</Label><Input id="productSize" value={form.size} onChange={(event) => setForm({ ...form, size: event.target.value })} required /></div><div className="space-y-2"><Label htmlFor="productColor">اللون</Label><Input id="productColor" value={form.color} onChange={(event) => setForm({ ...form, color: event.target.value })} required /></div><div className="space-y-2"><Label htmlFor="productSku">SKU</Label><Input id="productSku" value={form.sku} onChange={(event) => setForm({ ...form, sku: event.target.value })} required /></div><div className="space-y-2"><Label htmlFor="productPrice">السعر</Label><Input id="productPrice" type="number" min="0.01" step="0.01" value={form.price} onChange={(event) => setForm({ ...form, price: event.target.value })} required /></div><div className="space-y-2"><Label htmlFor="productStock">المخزون</Label><Input id="productStock" type="number" min="0" value={form.stock} onChange={(event) => setForm({ ...form, stock: event.target.value })} required /></div><div className="space-y-2"><Label htmlFor="productImage">رابط الصورة</Label><Input id="productImage" type="url" value={form.imageUrl} onChange={(event) => setForm({ ...form, imageUrl: event.target.value })} /></div></div><Button type="submit" disabled={submitting || categories.length === 0}>{submitting ? "جاري الحفظ..." : "حفظ وإرسال للمراجعة"}</Button></form></Card>
    <Card className="p-6 md:p-8"><div className="flex items-center justify-between"><div><h2 className="font-display text-2xl text-primary">منتجات المتجر</h2><p className="mt-2 text-sm text-muted-foreground">{loading ? "جاري التحميل..." : `${products.length} منتج`}</p></div><Button type="button" variant="outline" onClick={() => { setLoading(true); void load(); }}>تحديث</Button></div><div className="mt-6 space-y-3">{!loading && products.length === 0 && <p className="rounded-2xl bg-muted p-5 text-sm text-muted-foreground">لا توجد منتجات بعد.</p>}{products.map((product) => <div key={product.id} className="rounded-2xl border p-4"><div className="flex items-start justify-between gap-4"><div><p className="font-medium text-primary">{product.name}</p><p className="mt-1 text-xs text-muted-foreground">{product.slug}</p></div><span className="rounded-full bg-muted px-3 py-1 text-xs">{productStatus[String(product.status)] ?? String(product.status)}</span></div>{product.reviewNote && <p className="mt-3 text-sm text-red-700">{product.reviewNote}</p>}<p className="mt-3 text-xs text-muted-foreground">{product.variants.length} متغير · {product.media.length} صورة</p></div>)}</div></Card>
  </section>;
}
