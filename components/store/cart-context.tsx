"use client";

import { createContext, useContext, useEffect, useMemo, useState } from "react";
import type { Product } from "@/lib/products";

export type CartItem = {
  product: Product;
  quantity: number;
  size: string;
  color: string;
};

type CartContextValue = {
  items: CartItem[];
  addItem: (product: Product, options?: { size?: string; color?: string }) => void;
  updateQuantity: (productId: string, quantity: number, size: string, color: string) => void;
  removeItem: (productId: string, size: string, color: string) => void;
  clearCart: () => void;
  itemCount: number;
  subtotal: number;
};

const CartContext = createContext<CartContextValue | null>(null);

export function CartProvider({ children }: { children: React.ReactNode }) {
  const [items, setItems] = useState<CartItem[]>([]);

  useEffect(() => {
    const saved = window.localStorage.getItem("eleganza-cart");
    if (saved) {
      try {
        setItems(JSON.parse(saved) as CartItem[]);
      } catch {
        window.localStorage.removeItem("eleganza-cart");
      }
    }
  }, []);

  useEffect(() => {
    window.localStorage.setItem("eleganza-cart", JSON.stringify(items));
  }, [items]);

  const value = useMemo<CartContextValue>(() => {
    const addItem = (product: Product, options?: { size?: string; color?: string }) => {
      const size = options?.size ?? product.sizes[0];
      const color = options?.color ?? product.colors[0];
      setItems((current) => {
        const existing = current.find(
          (item) => item.product.id === product.id && item.size === size && item.color === color
        );
        if (existing) {
          return current.map((item) =>
            item.product.id === product.id && item.size === size && item.color === color
              ? { ...item, quantity: Math.min(item.quantity + 1, product.stock) }
              : item
          );
        }
        return [...current, { product, quantity: 1, size, color }];
      });
    };

    const updateQuantity = (productId: string, quantity: number, size: string, color: string) => {
      setItems((current) =>
        current
          .map((item) =>
            item.product.id === productId && item.size === size && item.color === color
              ? { ...item, quantity: Math.min(Math.max(quantity, 0), item.product.stock) }
              : item
          )
          .filter((item) => item.quantity > 0)
      );
    };

    const removeItem = (productId: string, size: string, color: string) => {
      setItems((current) => current.filter((item) => !(item.product.id === productId && item.size === size && item.color === color)));
    };

    return {
      items,
      addItem,
      updateQuantity,
      removeItem,
      clearCart: () => setItems([]),
      itemCount: items.reduce((sum, item) => sum + item.quantity, 0),
      subtotal: items.reduce((sum, item) => sum + item.product.price * item.quantity, 0),
    };
  }, [items]);

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}

export function useCart() {
  const context = useContext(CartContext);
  if (!context) throw new Error("useCart must be used inside CartProvider");
  return context;
}
