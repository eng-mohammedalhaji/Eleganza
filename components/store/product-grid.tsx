"use client";

import { Search, SlidersHorizontal } from "lucide-react";
import { useMemo, useState } from "react";
import type { Product } from "@/lib/products";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { ProductCard } from "@/components/store/product-card";

const categoryLabels: Record<string, string> = {
  all: "الكل",
  evening: "سهرة",
  bridal: "عرائس",
  daily: "يومي",
  modest: "محتشم",
};

export function ProductGrid({ initialProducts, initialCategory = "all" }: { initialProducts: Product[]; initialCategory?: string }) {
  const [category, setCategory] = useState(initialCategory);
  const [query, setQuery] = useState("");
  const [sort, setSort] = useState("featured");

  const filtered = useMemo(() => {
    const result = initialProducts.filter((product) => {
      const matchesCategory = category === "all" || product.category === category;
      const matchesQuery = `${product.name} ${product.description}`.toLowerCase().includes(query.toLowerCase());
      return matchesCategory && matchesQuery;
    });
    if (sort === "low") return [...result].sort((a, b) => a.price - b.price);
    if (sort === "high") return [...result].sort((a, b) => b.price - a.price);
    return [...result].sort((a, b) => Number(Boolean(b.featured)) - Number(Boolean(a.featured)));
  }, [category, initialProducts, query, sort]);

  return (
    <div>
      <div className="mb-10 flex flex-col gap-4 rounded-3xl border bg-white/60 p-4 md:flex-row md:items-center md:justify-between">
        <div className="relative w-full md:max-w-xs">
          <Search className="absolute right-4 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="ابحثي عن فستان..." className="pr-11" />
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <SlidersHorizontal className="ml-1 h-4 w-4 text-muted-foreground" />
          {Object.entries(categoryLabels).map(([key, label]) => (
            <Button key={key} variant={category === key ? "default" : "outline"} size="sm" onClick={() => setCategory(key)}>{label}</Button>
          ))}
          <select value={sort} onChange={(event) => setSort(event.target.value)} className="h-9 rounded-full border bg-transparent px-3 text-xs outline-none">
            <option value="featured">الأبرز</option>
            <option value="low">السعر: الأقل</option>
            <option value="high">السعر: الأعلى</option>
          </select>
        </div>
      </div>
      <div className="mb-5 flex items-center justify-between text-sm text-muted-foreground"><span>{filtered.length} فساتين</span><span>مختارات إيليجانزا</span></div>
      {filtered.length > 0 ? (
        <div className="grid gap-x-5 gap-y-12 sm:grid-cols-2 lg:grid-cols-4">{filtered.map((product) => <ProductCard key={product.id} product={product} />)}</div>
      ) : (
        <div className="rounded-3xl border border-dashed py-20 text-center text-muted-foreground">لم نجد نتائج مطابقة. جرّبي كلمة أخرى.</div>
      )}
    </div>
  );
}
