import { notFound } from "next/navigation";
import { ProductDetail } from "@/components/store/product-detail";
import { products } from "@/lib/products";
import { getStorefrontProduct } from "@/lib/storefront-api";

export function generateStaticParams() {
  return products.map((product) => ({ slug: product.slug }));
}

export default async function ProductPage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = await params;
  const product = await getStorefrontProduct(slug);
  if (!product) notFound();
  return <ProductDetail product={product} />;
}
