import {
  categories as demoCategories,
  getProduct as getDemoProduct,
  products as demoProducts,
  type Product,
  type StorefrontVariant,
} from "@/lib/products";
import { apiRequest } from "@/lib/api-client";

export type StorefrontCategory = {
  name: string;
  slug: string;
  count: number;
  description?: string | null;
};

type ApiCategory = {
  name: string;
  slug: string;
  description?: string | null;
  isActive: boolean;
};

type ApiProduct = {
  id: string;
  vendorId: string;
  categoryId: string;
  vendorName: string;
  categoryName: string;
  categorySlug: string;
  name: string;
  slug: string;
  description: string;
  isFeatured: boolean;
  variants: Array<{
    id: string;
    size: string;
    color: string;
    price: number;
    stock: number;
  }>;
  media: Array<{
    storageKey: string;
    altText?: string | null;
    sortOrder: number;
  }>;
};

export type CreateOrderPayload = {
  customerName: string;
  customerPhone: string;
  city: string;
  deliveryCityId?: number;
  deliverySubCityId?: number;
  address: string;
  mapUrl?: string;
  notes?: string;
  items: Array<{
    productId: string;
    variantId: string;
    quantity: number;
  }>;
};

export type CreatedOrder = {
  id: string;
  orderNumber: string;
  total: number;
};

function getApiBaseUrl() {
  return process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") || null;
}

function toProduct(item: ApiProduct): Product {
  const variants: StorefrontVariant[] = item.variants.map((variant) => ({
    id: variant.id,
    size: variant.size,
    color: variant.color,
    price: Number(variant.price),
    stock: variant.stock,
  }));
  const firstVariant = variants[0];
  const media = [...item.media].sort((a, b) => a.sortOrder - b.sortOrder);

  return {
    id: item.id,
    apiId: item.id,
    vendorId: item.vendorId,
    categoryId: item.categoryId,
    slug: item.slug,
    name: item.name,
    category: item.categorySlug || "all",
    description: item.description,
    price: firstVariant?.price ?? 0,
    sizes: [...new Set(variants.map((variant) => variant.size))],
    colors: [...new Set(variants.map((variant) => variant.color))],
    stock: variants.reduce((total, variant) => total + variant.stock, 0),
    badge: item.isFeatured ? "مختارات إيليجانزا" : undefined,
    featured: item.isFeatured,
    image: media[0]?.storageKey || "/placeholder-dress.svg",
    accent: "hsl(var(--muted))",
    variants,
  };
}

export async function getStorefrontProducts(): Promise<Product[]> {
  const baseUrl = getApiBaseUrl();
  if (!baseUrl) return demoProducts;

  try {
    const response = await fetch(`${baseUrl}/api/products`, {
      next: { revalidate: 60, tags: ["storefront-products"] },
    });
    if (!response.ok) throw new Error("تعذر تحميل المنتجات من واجهة API.");
    const data = (await response.json()) as ApiProduct[];
    return data.map(toProduct);
  } catch (error) {
    throw error instanceof Error ? error : new Error("تعذر تحميل المنتجات من واجهة API.");
  }
}

export async function getStorefrontProduct(slug: string): Promise<Product | undefined> {
  const baseUrl = getApiBaseUrl();
  if (!baseUrl) return getDemoProduct(slug);

  try {
    const response = await fetch(`${baseUrl}/api/products/${encodeURIComponent(slug)}`, {
      next: { revalidate: 60, tags: [`storefront-product:${slug}`] },
    });
    if (response.status === 404) return undefined;
    if (!response.ok) throw new Error("تعذر تحميل المنتج من واجهة API.");
    return toProduct((await response.json()) as ApiProduct);
  } catch (error) {
    throw error instanceof Error ? error : new Error("تعذر تحميل المنتج من واجهة API.");
  }
}

export async function getStorefrontCategories(
  storefrontProducts: Product[],
): Promise<StorefrontCategory[]> {
  const baseUrl = getApiBaseUrl();
  if (!baseUrl) return demoCategories;

  try {
    const response = await fetch(`${baseUrl}/api/categories`, {
      next: { revalidate: 300, tags: ["storefront-categories"] },
    });
    if (!response.ok) throw new Error("تعذر تحميل التصنيفات من واجهة API.");
    const data = (await response.json()) as ApiCategory[];
    return data.map((category) => ({
      name: category.name,
      slug: category.slug,
      description: category.description,
      count: storefrontProducts.filter((product) => product.category === category.slug).length,
    }));
  } catch (error) {
    throw error instanceof Error ? error : new Error("تعذر تحميل التصنيفات من واجهة API.");
  }
}

export function isApiBackedProduct(product: Product, variantId?: string) {
  return Boolean(product.apiId && variantId);
}

export async function createOrder(payload: CreateOrderPayload, idempotencyKey: string): Promise<CreatedOrder> {
  return apiRequest<CreatedOrder>("/api/orders", {
    method: "POST",
    headers: { "Idempotency-Key": idempotencyKey },
    body: JSON.stringify(payload),
  });
}
