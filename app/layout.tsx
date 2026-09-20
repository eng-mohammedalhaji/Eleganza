import type { Metadata } from "next";
import "./globals.css";
import { CartProvider } from "@/components/store/cart-context";
import { Header } from "@/components/store/header";

export const metadata: Metadata = {
  title: "إيليجانزا | أناقة تُحكى",
  description: "متجر إيليجانزا للفساتين والإطلالات الراقية.",
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="ar" dir="rtl">
      <body>
        <CartProvider>
          <Header />
          {children}
          <footer className="border-t bg-white/70">
            <div className="mx-auto flex max-w-7xl flex-col gap-5 px-5 py-10 text-sm text-muted-foreground md:flex-row md:items-center md:justify-between">
              <div>
                <p className="font-display text-2xl font-semibold text-primary">إيليجانزا</p>
                <p className="mt-1">لأن كل مناسبة تستحق فستاناً يليق بها.</p>
              </div>
              <div className="flex gap-5">
                <span>الشحن داخل ليبيا</span>
                <span>دعم عبر واتساب</span>
                <span>سياسة الاستبدال</span>
              </div>
            </div>
          </footer>
        </CartProvider>
      </body>
    </html>
  );
}
