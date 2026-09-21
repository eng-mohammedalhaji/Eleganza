"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { ArrowRight, CheckCircle2, Store } from "lucide-react";
import { ApiClientError, getApiBaseUrl } from "@/lib/api-client";
import { applyAsVendor, connectVanex, getMyVendor, getVanexAccount, type ShippingAccount, type Vendor } from "@/lib/vendor-api";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { VendorCatalog } from "@/components/vendor/vendor-catalog";

const vendorStatus: Record<string, string> = { "0": "قيد الانتظار", "1": "قيد المراجعة", "2": "معتمد", "3": "مرفوض", "4": "موقوف", Pending: "قيد الانتظار", UnderReview: "قيد المراجعة", Approved: "معتمد", Rejected: "مرفوض", Suspended: "موقوف" };
const shippingStatus: Record<string, string> = { "0": "غير موصول", "1": "قيد التحقق", "2": "متصل", "3": "غير صالح", Disconnected: "غير موصول", PendingValidation: "قيد التحقق", Connected: "متصل", Invalid: "غير صالح" };

export default function VendorPage() {
  const [vendor, setVendor] = useState<Vendor | null>(null);
  const [shipping, setShipping] = useState<ShippingAccount | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [application, setApplication] = useState({ businessName: "", slug: "", phone: "", city: "" });
  const [vanex, setVanex] = useState({ accessToken: "", merchantId: "" });

  useEffect(() => {
    if (!getApiBaseUrl()) { setLoading(false); return; }
    Promise.all([getMyVendor(), getVanexAccount()]).then(([currentVendor, account]) => { setVendor(currentVendor); setShipping(account); }).catch((caught) => setError(caught instanceof Error ? caught.message : "تعذر تحميل بيانات المورد.")).finally(() => setLoading(false));
  }, []);

  const submitApplication = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault(); setSubmitting(true); setError(null);
    try { setVendor(await applyAsVendor(application)); } catch (caught) { setError(caught instanceof ApiClientError || caught instanceof Error ? caught.message : "تعذر إرسال الطلب."); } finally { setSubmitting(false); }
  };

  const submitVanex = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault(); setSubmitting(true); setError(null);
    try { setShipping(await connectVanex(vanex)); setVanex({ accessToken: "", merchantId: "" }); } catch (caught) { setError(caught instanceof ApiClientError || caught instanceof Error ? caught.message : "تعذر ربط الحساب."); } finally { setSubmitting(false); }
  };

  if (!getApiBaseUrl()) return <main className="mx-auto max-w-3xl px-5 py-24 text-center"><h1 className="font-display text-4xl text-primary">شغّلي الـ API أولاً</h1><p className="mt-4 text-muted-foreground">اضبطي `NEXT_PUBLIC_API_URL` حتى تتمكني من فتح متجر وربط الشحن.</p></main>;
  if (loading) return <main className="mx-auto max-w-3xl px-5 py-24 text-center text-muted-foreground">جاري تحميل حساب المورد...</main>;

  return <main className="mx-auto max-w-5xl px-5 py-14 md:py-20"><Link href="/" className="mb-8 inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-primary"><ArrowRight className="h-4 w-4" /> العودة للرئيسية</Link><div className="mb-10"><p className="text-sm text-primary">بوابة الموردين</p><h1 className="mt-3 font-display text-5xl text-primary">{vendor ? vendor.businessName : "افتحي متجركِ"}</h1><p className="mt-4 max-w-2xl leading-8 text-muted-foreground">أضيفي منتجاتكِ، وبعد الاعتماد اربطي حساب Vanex الخاص بمتجرك لإرسال الطلبات مباشرة.</p></div>{error && <p className="mb-6 rounded-2xl bg-red-50 p-4 text-sm text-red-700">{error}</p>}{!vendor ? <Card className="max-w-2xl p-6 md:p-8"><div className="mb-7 flex items-center gap-3"><span className="flex h-11 w-11 items-center justify-center rounded-2xl bg-primary/10 text-primary"><Store className="h-5 w-5" /></span><div><h2 className="font-display text-2xl text-primary">طلب فتح متجر</h2><p className="text-sm text-muted-foreground">سيتم مراجعته من فريق إيليجانزا.</p></div></div><form onSubmit={submitApplication} className="grid gap-5 sm:grid-cols-2"><div className="space-y-2 sm:col-span-2"><Label htmlFor="businessName">اسم المحل</Label><Input id="businessName" value={application.businessName} onChange={(event) => setApplication({ ...application, businessName: event.target.value })} required minLength={2} /></div><div className="space-y-2"><Label htmlFor="slug">الرابط المختصر</Label><Input id="slug" placeholder="al-shaqabi" value={application.slug} onChange={(event) => setApplication({ ...application, slug: event.target.value })} required /></div><div className="space-y-2"><Label htmlFor="phone">رقم الهاتف</Label><Input id="phone" type="tel" value={application.phone} onChange={(event) => setApplication({ ...application, phone: event.target.value })} required /></div><div className="space-y-2 sm:col-span-2"><Label htmlFor="city">المدينة</Label><Input id="city" value={application.city} onChange={(event) => setApplication({ ...application, city: event.target.value })} required /></div><Button type="submit" size="lg" className="sm:col-span-2" disabled={submitting}>{submitting ? "جاري الإرسال..." : "إرسال طلب الاعتماد"}</Button></form></Card> : <><div className="grid gap-7 lg:grid-cols-[0.8fr_1.2fr]"><Card className="p-6"><p className="text-sm text-muted-foreground">حالة المتجر</p><p className="mt-3 font-display text-3xl text-primary">{vendorStatus[String(vendor.status)] ?? "غير معروفة"}</p>{vendor.reviewNote && <p className="mt-4 rounded-2xl bg-muted p-4 text-sm">{vendor.reviewNote}</p>}<div className="mt-6 space-y-3 text-sm text-muted-foreground"><p>المدينة: {vendor.city}</p><p>الهاتف: {vendor.phone}</p><p className="flex items-center gap-2"><CheckCircle2 className="h-4 w-4 text-primary" /> لا يظهر المتجر للزبائن قبل الاعتماد</p></div></Card>{String(vendor.status) === "2" || vendor.status === "Approved" ? <Card className="p-6 md:p-8"><h2 className="font-display text-2xl text-primary">ربط Vanex</h2><p className="mt-2 text-sm leading-7 text-muted-foreground">الصقّي الـ Access Token الذي حصلتِ عليه من Vanex. سيتم تشفيره في السيرفر ولن نعيد عرضه.</p>{shipping && <p className="mt-5 rounded-2xl bg-muted p-4 text-sm">الحالة: <strong>{shippingStatus[String(shipping.status)] ?? "غير معروفة"}</strong>{shipping.lastError && <span className="mt-2 block text-red-700">{shipping.lastError}</span>}</p>}<form onSubmit={submitVanex} className="mt-6 space-y-5"><div className="space-y-2"><Label htmlFor="accessToken">Access Token</Label><Input id="accessToken" type="password" value={vanex.accessToken} onChange={(event) => setVanex({ ...vanex, accessToken: event.target.value })} required autoComplete="off" /></div><div className="space-y-2"><Label htmlFor="merchantId">Merchant ID (اختياري)</Label><Input id="merchantId" value={vanex.merchantId} onChange={(event) => setVanex({ ...vanex, merchantId: event.target.value })} /></div><Button type="submit" disabled={submitting}>{submitting ? "جاري التحقق..." : "حفظ وربط الحساب"}</Button></form></Card> : <Card className="p-6"><h2 className="font-display text-2xl text-primary">الخطوة التالية</h2><p className="mt-3 leading-8 text-muted-foreground">سيظهر نموذج ربط Vanex بعد اعتماد المتجر من الإدارة.</p></Card>}</div>{(String(vendor.status) === "2" || vendor.status === "Approved") && <VendorCatalog />}</>}</main>;
}
