"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { ArrowLeft, Boxes, MoreHorizontal, Plus, ShoppingBag, TrendingUp, Users } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { products } from "@/lib/products";
import { formatCurrency } from "@/lib/utils";
import { ApiClientError, getApiBaseUrl } from "@/lib/api-client";
import { getAdminOrders } from "@/lib/admin-api";

type OrderRow = {
  id: string;
  name: string;
  date: string;
  total: number;
  status: string;
};

const demoOrders: OrderRow[] = [
  { id: "ELG-284103", name: "سارة محمد", date: "اليوم، 10:45", total: 735, status: "جديد" },
  { id: "ELG-283992", name: "نور علي", date: "أمس، 18:20", total: 620, status: "قيد التجهيز" },
  { id: "ELG-283817", name: "ريم سالم", date: "أمس، 12:10", total: 1450, status: "تم الشحن" },
  { id: "ELG-283506", name: "جود فرحات", date: "17 سبتمبر", total: 390, status: "مكتمل" },
];

export function AdminDashboard() {
  const apiConfigured = Boolean(getApiBaseUrl());
  const [orders, setOrders] = useState<OrderRow[]>(apiConfigured ? [] : demoOrders);
  const [totalSales, setTotalSales] = useState(apiConfigured ? 0 : 28450);
  const [loading, setLoading] = useState(apiConfigured);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!apiConfigured) return;
    getAdminOrders(50)
      .then((items) => {
        const mapped = items.map((order) => ({
          id: order.orderNumber,
          name: order.customerName,
          date: new Intl.DateTimeFormat("ar-LY", { day: "numeric", month: "short", hour: "2-digit", minute: "2-digit" }).format(new Date(order.createdAt)),
          total: Number(order.total),
          status: statusLabel(order.status),
        }));
        setOrders(mapped);
        setTotalSales(mapped.reduce((sum, order) => sum + order.total, 0));
      })
      .catch((caught) => setError(caught instanceof ApiClientError ? caught.status === 401 || caught.status === 403 ? "تسجيل دخول Admin مطلوب لعرض بيانات اللوحة." : caught.message : "تعذر تحميل بيانات الإدارة."))
      .finally(() => setLoading(false));
  }, [apiConfigured]);

  return <DashboardView orders={orders} totalSales={totalSales} loading={loading} error={error} />;
}

function statusLabel(status: number | string) {
  return { "0": "جديد", "1": "قيد التجهيز", "2": "مرفوض", "3": "ملغي", "4": "مكتمل", Pending: "جديد", Confirmed: "قيد التجهيز", Rejected: "مرفوض", Cancelled: "ملغي", Completed: "مكتمل" }[String(status)] ?? String(status);
}

function DashboardView({ orders, totalSales, loading = false, error }: { orders: OrderRow[]; totalSales: number; loading?: boolean; error?: string | null }) {
  const liveLabel = error ? "تحتاج صلاحية Admin" : loading ? "جاري التحميل" : "من API";
  return <main className="min-h-screen bg-muted/40"><div className="mx-auto max-w-7xl px-5 py-10"><div className="mb-10 flex flex-col justify-between gap-5 sm:flex-row sm:items-center"><div><p className="text-sm text-primary">لوحة التحكم</p><h1 className="mt-2 font-display text-4xl text-primary">صباح الخير، فريق إيليجانزا</h1></div><Button asChild><Link href="/shop"><ArrowLeft className="ml-2 h-4 w-4" /> عرض المتجر</Link></Button></div><div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4"><StatCard icon={<ShoppingBag />} label="إجمالي الطلبات" value={String(orders.length)} change={liveLabel} /><StatCard icon={<TrendingUp />} label="إجمالي المبيعات" value={formatCurrency(totalSales)} change={liveLabel} /><StatCard icon={<Users />} label="عميلات جديدات" value={apiConfiguredValue(error, "64")} change={error ? "غير متاح" : "+8.4%"} /><StatCard icon={<Boxes />} label="منتجات نشطة" value={apiConfiguredValue(error, String(products.length))} change={error ? "غير متاح" : "بيانات الكتالوج"} /></div><div className="mt-8 grid gap-8 lg:grid-cols-[1.25fr_0.75fr]"><Card><CardHeader className="flex-row items-center justify-between"><div><CardTitle>آخر الطلبات</CardTitle><p className="mt-1 text-sm text-muted-foreground">{error ?? "تُقرأ من ASP.NET Core API"}</p></div><Button variant="outline" size="sm">كل الطلبات</Button></CardHeader><CardContent className="p-0">{loading ? <div className="px-6 py-12 text-center text-sm text-muted-foreground">جاري الاتصال بالـ API...</div> : error ? <div className="px-6 py-12 text-center text-sm text-red-700">{error}</div> : orders.length === 0 ? <div className="px-6 py-12 text-center text-sm text-muted-foreground">لا توجد طلبات محفوظة حالياً.</div> : <div className="divide-y">{orders.map((order) => <div key={order.id} className="flex flex-wrap items-center gap-4 px-6 py-4"><div className="min-w-32 flex-1"><p className="text-sm font-medium text-primary">{order.id}</p><p className="mt-1 text-xs text-muted-foreground">{order.name} · {order.date}</p></div><p className="text-sm font-medium">{formatCurrency(order.total)}</p><Badge variant={order.status === "جديد" ? "default" : "muted"}>{order.status}</Badge><Button variant="ghost" size="icon" aria-label="خيارات"><MoreHorizontal className="h-4 w-4" /></Button></div>)}</div>}</CardContent></Card><Card><CardHeader className="flex-row items-center justify-between"><div><CardTitle>المنتجات</CardTitle><p className="mt-1 text-sm text-muted-foreground">آخر المنتجات المضافة</p></div><Button variant="soft" size="sm"><Plus className="ml-1 h-4 w-4" /> إضافة</Button></CardHeader><CardContent className="space-y-4">{products.slice(0, 4).map((product) => <div key={product.id} className="flex items-center gap-3"><div className="h-12 w-10 overflow-hidden rounded-xl" style={{ backgroundColor: product.accent }}><img src={product.image} alt={product.name} className="h-full w-full object-cover" /></div><div className="min-w-0 flex-1"><p className="truncate text-sm font-medium">{product.name}</p><p className="mt-1 text-xs text-muted-foreground">{product.stock} قطع متوفرة</p></div><p className="text-sm text-primary">{formatCurrency(product.price)}</p></div>)}</CardContent></Card></div></div></main>;
}

function apiConfiguredValue(error: string | null | undefined, fallback: string) {
  return error ? "—" : fallback;
}

function StatCard({ icon, label, value, change }: { icon: React.ReactNode; label: string; value: string; change: string }) {
  return <Card className="shadow-none"><CardContent className="p-5"><div className="flex items-start justify-between"><span className="flex h-10 w-10 items-center justify-center rounded-2xl bg-primary/10 text-primary">{icon}</span><span className="text-xs text-emerald-600">{change}</span></div><p className="mt-6 text-sm text-muted-foreground">{label}</p><p className="mt-1 font-display text-2xl text-primary">{value}</p></CardContent></Card>;
}
