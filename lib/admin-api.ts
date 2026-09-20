import { apiRequest } from "@/lib/api-client";

export type AdminOrder = {
  orderNumber: string;
  customerName: string;
  total: number;
  status: number | string;
  createdAt: string;
};

export function getAdminOrders(take = 50) {
  return apiRequest<AdminOrder[]>(`/api/admin/orders?take=${take}`);
}
