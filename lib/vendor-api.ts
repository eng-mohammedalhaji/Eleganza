import { apiRequest } from "@/lib/api-client";

export type Vendor = {
  id: string;
  businessName: string;
  slug: string;
  phone: string;
  city: string;
  status: number | string;
  reviewNote?: string | null;
};

export type ShippingAccount = {
  provider: string;
  status: number | string;
  merchantId?: string | null;
  baseUrl?: string | null;
  lastValidatedAt?: string | null;
  lastError?: string | null;
};

export type VendorApplication = {
  businessName: string;
  slug: string;
  phone: string;
  city: string;
};

export type VanexConnection = {
  accessToken: string;
  merchantId?: string;
};

export type VanexLocation = {
  id: string;
  name: string;
};

export type VendorCategory = {
  id: string;
  name: string;
  slug: string;
  isActive: boolean;
};

export type VendorProduct = {
  id: string;
  categoryId: string;
  name: string;
  slug: string;
  description: string;
  status: number | string;
  reviewNote?: string | null;
  variants: Array<{ id: string; size: string; color: string; sku: string; price: number; stock: number }>;
  media: Array<{ id: string; storageKey: string; altText?: string | null; sortOrder: number }>;
};

export async function getMyVendor() {
  try {
    return await apiRequest<Vendor>("/api/vendors/me");
  } catch (error) {
    if (error instanceof Error && "status" in error && (error as { status: number }).status === 404) {
      return null;
    }
    throw error;
  }
}

export function applyAsVendor(application: VendorApplication) {
  return apiRequest<Vendor>("/api/vendors", {
    method: "POST",
    body: JSON.stringify(application),
  });
}

export async function getVanexAccount() {
  try {
    return await apiRequest<ShippingAccount>("/api/vendors/me/shipping/vanex");
  } catch (error) {
    if (error instanceof Error && "status" in error && (error as { status: number }).status === 404) {
      return null;
    }
    throw error;
  }
}

export function connectVanex(connection: VanexConnection) {
  return apiRequest<ShippingAccount>("/api/vendors/me/shipping/vanex", {
    method: "PUT",
    body: JSON.stringify(connection),
  });
}

export function disconnectVanex() {
  return apiRequest<ShippingAccount>("/api/vendors/me/shipping/vanex", {
    method: "DELETE",
  });
}

export function getVendorVanexCities(vendorId: string) {
  return apiRequest<VanexLocation[]>(`/api/vendors/${vendorId}/shipping/vanex/cities`);
}

export function getVendorVanexSubCities(vendorId: string, cityId: string) {
  return apiRequest<VanexLocation[]>(
    `/api/vendors/${vendorId}/shipping/vanex/cities/${encodeURIComponent(cityId)}/subcities`,
  );
}

export function getVendorCategories() {
  return apiRequest<VendorCategory[]>("/api/categories");
}

export function getVendorProducts() {
  return apiRequest<VendorProduct[]>("/api/vendor/products");
}

export function createVendorProduct(payload: { categoryId: string; name: string; slug: string; description: string }) {
  return apiRequest<VendorProduct>("/api/products", { method: "POST", body: JSON.stringify(payload) });
}

export function addProductVariant(productId: string, payload: { size: string; color: string; sku: string; price: number; stock: number }) {
  return apiRequest<VendorProduct>(`/api/products/${productId}/variants`, { method: "POST", body: JSON.stringify(payload) });
}

export function addProductMedia(productId: string, payload: { storageKey: string; altText?: string; sortOrder: number }) {
  return apiRequest<VendorProduct>(`/api/products/${productId}/media`, { method: "POST", body: JSON.stringify(payload) });
}

export function submitVendorProduct(productId: string) {
  return apiRequest<VendorProduct>(`/api/products/${productId}/submit`, { method: "POST" });
}
