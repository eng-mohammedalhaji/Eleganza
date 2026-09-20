import { type ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function formatCurrency(value: number) {
  return new Intl.NumberFormat("ar-LY", {
    style: "currency",
    currency: "LYD",
    maximumFractionDigits: 0,
  }).format(value);
}
