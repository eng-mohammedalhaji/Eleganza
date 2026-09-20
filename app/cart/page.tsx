"use client";

import Link from "next/link";
import { ArrowLeft, Minus, Plus, ShoppingBag, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { useCart } from "@/components/store/cart-context";
import { formatCurrency } from "@/lib/utils";

export default function CartPage() {
  const { items, itemCount, subtotal, updateQuantity, removeItem } = useCart();
  const shipping = subtotal > 500 || subtotal === 0 ? 0 : 25;

  return (
    <main className="mx-auto max-w-7xl px-5 py-14 md:py-20"><div className="mb-12"><p className="text-sm text-primary">حقيبتك</p><h1 className="mt-3 font-display text-5xl text-primary">سلة المشتريات</h1></div>
      {items.length === 0 ? <div className="rounded-[2rem] border border-dashed py-24 text-center"><ShoppingBag className="mx-auto h-10 w-10 text-muted-foreground" /><h2 className="mt-5 font-display text-2xl text-primary">السلة فاضية حالياً</h2><p className="mt-2 text-muted-foreground">اختاري قطعة تحبيها وخلّيها في انتظارك.</p><Button className="mt-7" asChild><Link href="/shop">تسوقي الآن <ArrowLeft className="mr-2 h-4 w-4" /></Link></Button></div> : <div className="grid gap-8 lg:grid-cols-[1fr_380px] lg:items-start"><div className="space-y-4">{items.map((item) => <Card key={`${item.product.id}-${item.size}-${item.color}`} className="flex gap-4 p-4 shadow-none sm:gap-6"><div className="h-32 w-24 shrink-0 overflow-hidden rounded-2xl" style={{ backgroundColor: item.product.accent }}><img src={item.product.image} alt={item.product.name} className="h-full w-full object-cover" /></div><div className="flex min-w-0 flex-1 flex-col justify-between py-1"><div className="flex items-start justify-between gap-3"><div><h2 className="font-display text-xl text-primary">{item.product.name}</h2><p className="mt-1 text-xs text-muted-foreground">المقاس: {item.size} · اللون: {item.color}</p></div><button onClick={() => removeItem(item.product.id, item.size, item.color)} className="text-muted-foreground hover:text-primary" aria-label="حذف"><Trash2 className="h-4 w-4" /></button></div><div className="flex items-end justify-between gap-3"><div className="flex h-9 items-center rounded-full border px-1"><button onClick={() => updateQuantity(item.product.id, item.quantity - 1, item.size, item.color)} className="flex h-7 w-7 items-center justify-center rounded-full hover:bg-muted"><Minus className="h-3.5 w-3.5" /></button><span className="w-7 text-center text-sm">{item.quantity}</span><button onClick={() => updateQuantity(item.product.id, item.quantity + 1, item.size, item.color)} className="flex h-7 w-7 items-center justify-center rounded-full hover:bg-muted"><Plus className="h-3.5 w-3.5" /></button></div><p className="font-medium text-primary">{formatCurrency(item.product.price * item.quantity)}</p></div></div></Card>)}</div><Card className="sticky top-28 p-6"><h2 className="font-display text-2xl text-primary">ملخص الطلب</h2><div className="mt-6 space-y-4 text-sm"><div className="flex justify-between"><span className="text-muted-foreground">المنتجات ({itemCount})</span><span>{formatCurrency(subtotal)}</span></div><div className="flex justify-between"><span className="text-muted-foreground">التوصيل</span><span>{shipping === 0 ? "مجاني" : formatCurrency(shipping)}</span></div><div className="border-t pt-4"><div className="flex justify-between text-base font-medium"><span>الإجمالي</span><span className="text-primary">{formatCurrency(subtotal + shipping)}</span></div></div></div><Button className="mt-7 w-full" size="lg" asChild><Link href="/checkout">إتمام الطلب <ArrowLeft className="mr-2 h-4 w-4" /></Link></Button><p className="mt-4 text-center text-xs text-muted-foreground">التوصيل مجاني للطلبات فوق 500 د.ل</p></Card></div>}
    </main>
  );
}
