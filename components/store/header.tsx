"use client";

import Link from "next/link";
import { Heart, Menu, Search, ShoppingBag, UserRound, X } from "lucide-react";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { useCart } from "@/components/store/cart-context";

export function Header() {
  const [open, setOpen] = useState(false);
  const { itemCount } = useCart();
  const links = [
    { href: "/shop", label: "المتجر" },
    { href: "/shop?category=evening", label: "فساتين السهرة" },
    { href: "/shop?category=bridal", label: "مجموعة العرائس" },
    { href: "/shop?category=daily", label: "إطلالات يومية" },
  ];

  return (
    <header className="sticky top-0 z-40 border-b border-border/70 bg-background/90 backdrop-blur-xl">
      <div className="mx-auto flex h-20 max-w-7xl items-center justify-between px-5">
        <Link href="/" className="group flex items-center gap-3" onClick={() => setOpen(false)}>
          <span className="flex h-10 w-10 items-center justify-center rounded-full bg-primary text-lg text-primary-foreground">إ</span>
          <span>
            <span className="block font-display text-2xl font-semibold leading-none text-primary">إيليجانزا</span>
            <span className="mt-1 block text-[10px] tracking-[0.25em] text-muted-foreground">ELEGANZA</span>
          </span>
        </Link>

        <nav className="hidden items-center gap-7 text-sm md:flex">
          {links.map((link) => (
            <Link key={link.href} href={link.href} className="transition-colors hover:text-primary">
              {link.label}
            </Link>
          ))}
        </nav>

        <div className="flex items-center gap-1">
          <Button variant="ghost" size="icon" asChild aria-label="البحث">
            <Link href="/shop"><Search className="h-5 w-5" /></Link>
          </Button>
          <Button variant="ghost" size="icon" asChild aria-label="الحساب" className="hidden sm:inline-flex">
            <Link href="/account"><UserRound className="h-5 w-5" /></Link>
          </Button>
          <Button variant="ghost" size="icon" className="hidden sm:inline-flex" aria-label="المفضلة">
            <Heart className="h-5 w-5" />
          </Button>
          <Button variant="ghost" size="icon" asChild aria-label="السلة" className="relative">
            <Link href="/cart">
              <ShoppingBag className="h-5 w-5" />
              {itemCount > 0 && <span className="absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-primary px-1 text-[10px] text-white">{itemCount}</span>}
            </Link>
          </Button>
          <Button variant="ghost" size="icon" className="md:hidden" onClick={() => setOpen((value) => !value)} aria-label="القائمة">
            {open ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
          </Button>
        </div>
      </div>
      {open && (
        <nav className="border-t bg-background px-5 py-4 md:hidden">
          <div className="mx-auto flex max-w-7xl flex-col gap-4 text-sm">
            {links.map((link) => (
              <Link key={link.href} href={link.href} onClick={() => setOpen(false)} className="py-1">
                {link.label}
              </Link>
            ))}
          </div>
        </nav>
      )}
    </header>
  );
}
