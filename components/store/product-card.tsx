"use client";

import Link from "next/link";
import { ArrowUpLeft, Heart, ShoppingBag } from "lucide-react";
import { useState } from "react";
import type { Product } from "@/lib/products";
import { formatCurrency } from "@/lib/utils";
import { useCart } from "@/components/store/cart-context";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

export function ProductCard({ product }: { product: Product }) {
  const { addItem } = useCart();
  const [liked, setLiked] = useState(false);

  return (
    <article className="group">
      <div className="relative aspect-[4/5] overflow-hidden rounded-[1.5rem] bg-muted" style={{ backgroundColor: product.accent }}>
        <Link href={`/shop/${product.slug}`} className="block h-full">
          <img src={product.image} alt={product.name} className="h-full w-full object-cover transition duration-700 group-hover:scale-105" />
        </Link>
        <div className="absolute right-4 top-4 flex flex-col gap-2">
          {product.badge && <Badge variant="soft" className="bg-white/85 backdrop-blur">{product.badge}</Badge>}
        </div>
        <Button
          variant="outline"
          size="icon"
          className="absolute left-4 top-4 border-white/70 bg-white/80 backdrop-blur hover:bg-white"
          onClick={() => setLiked((value) => !value)}
          aria-label="إضافة للمفضلة"
        >
          <Heart className={`h-4 w-4 ${liked ? "fill-primary text-primary" : ""}`} />
        </Button>
        <div className="absolute inset-x-4 bottom-4 translate-y-14 opacity-0 transition duration-300 group-hover:translate-y-0 group-hover:opacity-100">
          <Button className="w-full bg-white text-primary hover:bg-white/90" onClick={() => addItem(product)}>
            <ShoppingBag className="ml-2 h-4 w-4" />
            أضف للسلة
          </Button>
        </div>
      </div>
      <div className="flex items-start justify-between gap-3 px-1 pt-4">
        <div>
          <Link href={`/shop/${product.slug}`} className="font-display text-xl transition-colors hover:text-primary">{product.name}</Link>
          <p className="mt-1 text-sm text-muted-foreground">{product.category === "bridal" ? "مجموعة العرائس" : product.category === "evening" ? "فساتين سهرة" : product.category === "modest" ? "إطلالة محتشمة" : "إطلالة يومية"}</p>
        </div>
        <div className="text-left">
          <p className="font-medium text-primary">{formatCurrency(product.price)}</p>
          {product.oldPrice && <p className="text-xs text-muted-foreground line-through">{formatCurrency(product.oldPrice)}</p>}
        </div>
      </div>
      <Link href={`/shop/${product.slug}`} className="mt-3 inline-flex items-center gap-1 px-1 text-xs text-muted-foreground transition-colors hover:text-primary">
        اكتشفي التفاصيل <ArrowUpLeft className="h-3.5 w-3.5" />
      </Link>
    </article>
  );
}
