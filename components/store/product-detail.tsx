"use client";

import Link from "next/link";
import { ArrowRight, Check, Heart, Minus, Plus, ShoppingBag } from "lucide-react";
import { useState } from "react";
import type { Product } from "@/lib/products";
import { formatCurrency } from "@/lib/utils";
import { useCart } from "@/components/store/cart-context";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";

export function ProductDetail({ product }: { product: Product }) {
  const { addItem } = useCart();
  const [size, setSize] = useState(product.sizes[0]);
  const [color, setColor] = useState(product.colors[0]);
  const [quantity, setQuantity] = useState(1);
  const [added, setAdded] = useState(false);

  const addToCart = () => {
    for (let index = 0; index < quantity; index += 1) addItem(product, { size, color });
    setAdded(true);
    window.setTimeout(() => setAdded(false), 1800);
  };

  return (
    <main className="mx-auto max-w-7xl px-5 py-10 md:py-16">
      <Link href="/shop" className="mb-8 inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-primary"><ArrowRight className="h-4 w-4" /> العودة للمتجر</Link>
      <div className="grid gap-12 lg:grid-cols-2 lg:gap-20">
        <div className="relative overflow-hidden rounded-[2rem] bg-muted" style={{ backgroundColor: product.accent }}><img src={product.image} alt={product.name} className="aspect-[4/5] h-full w-full object-cover" />{product.badge && <Badge variant="soft" className="absolute right-5 top-5 bg-white/85">{product.badge}</Badge>}</div>
        <div className="flex flex-col justify-center"><p className="text-sm text-muted-foreground">{product.category === "bridal" ? "مجموعة العرائس" : product.category === "evening" ? "فساتين سهرة" : product.category === "modest" ? "إطلالة محتشمة" : "إطلالة يومية"}</p><h1 className="mt-3 font-display text-5xl text-primary">{product.name}</h1><div className="mt-5 flex items-center gap-3"><span className="text-2xl font-medium text-primary">{formatCurrency(product.price)}</span>{product.oldPrice && <span className="text-sm text-muted-foreground line-through">{formatCurrency(product.oldPrice)}</span>}</div><p className="mt-6 max-w-xl leading-8 text-muted-foreground">{product.description}</p><Separator className="my-8" />
          <div><div className="mb-3 flex items-center justify-between"><span className="text-sm font-medium">المقاس</span><button className="text-xs text-muted-foreground underline">دليل المقاسات</button></div><div className="flex flex-wrap gap-2">{product.sizes.map((item) => <button key={item} onClick={() => setSize(item)} className={`flex h-11 min-w-11 items-center justify-center rounded-full border px-4 text-sm transition-colors ${size === item ? "border-primary bg-primary text-white" : "hover:border-primary"}`}>{item}</button>)}</div></div>
          <div className="mt-6"><p className="mb-3 text-sm font-medium">اللون: <span className="font-normal text-muted-foreground">{color}</span></p><div className="flex flex-wrap gap-2">{product.colors.map((item) => <button key={item} onClick={() => setColor(item)} className={`rounded-full border px-4 py-2 text-xs transition-colors ${color === item ? "border-primary bg-primary text-white" : "hover:border-primary"}`}>{item}</button>)}</div></div>
          <div className="mt-8 flex flex-col gap-3 sm:flex-row"><div className="flex h-12 items-center justify-between rounded-full border px-2 sm:w-36"><button onClick={() => setQuantity((value) => Math.max(1, value - 1))} className="flex h-8 w-8 items-center justify-center rounded-full hover:bg-muted"><Minus className="h-4 w-4" /></button><span className="text-sm">{quantity}</span><button onClick={() => setQuantity((value) => Math.min(product.stock, value + 1))} className="flex h-8 w-8 items-center justify-center rounded-full hover:bg-muted"><Plus className="h-4 w-4" /></button></div><Button size="lg" className="flex-1" onClick={addToCart}>{added ? <><Check className="ml-2 h-4 w-4" /> تمت الإضافة</> : <><ShoppingBag className="ml-2 h-4 w-4" /> أضيفي للسلة</>}</Button><Button variant="outline" size="icon" className="h-12 w-12 shrink-0" aria-label="إضافة للمفضلة"><Heart className="h-5 w-5" /></Button></div>
          <div className="mt-8 grid gap-3 text-sm text-muted-foreground sm:grid-cols-2"><p className="flex items-center gap-2"><Check className="h-4 w-4 text-primary" /> متوفر حالياً ({product.stock})</p><p className="flex items-center gap-2"><Check className="h-4 w-4 text-primary" /> شحن آمن داخل ليبيا</p></div>
        </div>
      </div>
    </main>
  );
}
