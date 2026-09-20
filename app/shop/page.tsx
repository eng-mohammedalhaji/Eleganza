import { ProductGrid } from "@/components/store/product-grid";
import { getStorefrontProducts } from "@/lib/storefront-api";

export default async function ShopPage({ searchParams }: { searchParams: Promise<{ category?: string }> }) {
  const params = await searchParams;
  const products = await getStorefrontProducts();
  return (
    <main className="mx-auto max-w-7xl px-5 py-14 md:py-20">
      <div className="mb-14 max-w-2xl"><p className="text-sm text-primary">المتجر</p><h1 className="mt-3 font-display text-5xl text-primary">قطع تشبهكِ</h1><p className="mt-5 leading-8 text-muted-foreground">تشكيلة منتقاة تجمع بين القصّات العصرية، الخامات الجميلة، والتفاصيل التي تجعل كل فستان مميزاً.</p></div>
      <ProductGrid initialProducts={products} initialCategory={params.category ?? "all"} />
    </main>
  );
}
