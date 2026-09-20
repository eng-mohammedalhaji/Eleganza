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
  baseUrl?: string;
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
