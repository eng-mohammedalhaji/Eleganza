import Link from "next/link";
import { ArrowLeft, ArrowUpLeft, Sparkles } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ProductCard } from "@/components/store/product-card";
import { categories, products } from "@/lib/products";

export default function HomePage() {
  const featured = products.filter((product) => product.featured).slice(0, 4);

  return (
    <main>
      <section className="relative overflow-hidden border-b">
        <div className="noise absolute inset-0 opacity-20" />
        <div className="mx-auto grid max-w-7xl items-center gap-12 px-5 py-16 md:grid-cols-[0.9fr_1.1fr] md:py-24 lg:py-28">
          <div className="relative z-10 max-w-xl">
            <Badge variant="soft" className="mb-6 w-fit"><Sparkles className="ml-2 h-3.5 w-3.5" /> مجموعة صيف 2026</Badge>
            <h1 className="font-display text-5xl leading-[1.08] text-primary sm:text-6xl lg:text-7xl">أناقتكِ،<br /><span className="text-accent-foreground">بلمسة تُحكى.</span></h1>
            <p className="mt-6 max-w-md text-base leading-8 text-muted-foreground">فساتين منتقاة بعناية للمرأة التي تعرف أن التفاصيل الصغيرة تصنع الإطلالة الكبيرة.</p>
            <div className="mt-8 flex flex-wrap gap-3">
              <Button size="lg" asChild><Link href="/shop">اكتشفي المجموعة <ArrowLeft className="mr-2 h-4 w-4" /></Link></Button>
              <Button size="lg" variant="outline" asChild><Link href="/shop?category=bridal">مجموعة العرائس</Link></Button>
            </div>
            <div className="mt-12 flex gap-8 border-t pt-6 text-sm">
              <div><p className="font-display text-2xl text-primary">+120</p><p className="mt-1 text-muted-foreground">تصميماً مختاراً</p></div>
              <div><p className="font-display text-2xl text-primary">4.9/5</p><p className="mt-1 text-muted-foreground">رضا عميلاتنا</p></div>
              <div><p className="font-display text-2xl text-primary">48h</p><p className="mt-1 text-muted-foreground">تجهيز الطلب</p></div>
            </div>
          </div>
          <div className="relative mx-auto w-full max-w-lg">
            <div className="absolute -inset-6 rounded-[3rem] bg-accent/20 blur-3xl" />
            <div className="relative aspect-[4/5] overflow-hidden rounded-[2.5rem] bg-[#dcb4ae] shadow-soft">
              <img src="https://images.unsplash.com/photo-1591369822096-ffd140ec948f?auto=format&fit=crop&w=1200&q=90" alt="إطلالة من مجموعة إيليجانزا" className="h-full w-full object-cover" />
              <div className="absolute bottom-5 right-5 rounded-2xl bg-white/85 px-5 py-4 backdrop-blur"><p className="text-xs text-muted-foreground">اختيار الأسبوع</p><p className="mt-1 font-display text-xl text-primary">Rose Satin</p></div>
            </div>
            <div className="absolute -bottom-5 -left-5 hidden rounded-2xl border bg-white p-4 shadow-soft sm:block"><p className="text-xs text-muted-foreground">تفصيل ناعم</p><p className="mt-1 font-medium text-primary">صُنع ليليق بكِ</p></div>
          </div>
        </div>
      </section>

      <section className="mx-auto max-w-7xl px-5 py-20">
        <div className="mb-10 flex items-end justify-between gap-4"><div><p className="text-sm text-primary">تسوّقي حسب المناسبة</p><h2 className="mt-2 font-display text-4xl text-primary">اختاري مزاجكِ</h2></div><Link href="/shop" className="hidden items-center gap-2 text-sm text-muted-foreground hover:text-primary sm:flex">كل التصنيفات <ArrowUpLeft className="h-4 w-4" /></Link></div>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {categories.map((category, index) => (
            <Link href={`/shop?category=${category.slug}`} key={category.slug} className={`group relative overflow-hidden rounded-3xl p-6 ${index % 2 === 0 ? "bg-[#ead7cf]" : "bg-[#e3e1d1]"}`}>
              <div className="relative z-10"><p className="text-xs text-primary/70">0{index + 1}</p><h3 className="mt-10 font-display text-2xl text-primary">{category.name}</h3><p className="mt-2 text-sm text-primary/70">{category.count} تصميماً</p></div>
              <div className="absolute -bottom-10 -left-10 h-32 w-32 rounded-full border-[18px] border-white/25 transition-transform duration-500 group-hover:scale-125" />
              <ArrowUpLeft className="absolute left-6 top-6 h-5 w-5 text-primary/50 transition-transform group-hover:-translate-x-1 group-hover:-translate-y-1" />
            </Link>
          ))}
        </div>
      </section>

      <section className="bg-white/60 py-20">
        <div className="mx-auto max-w-7xl px-5"><div className="mb-10 flex items-end justify-between gap-4"><div><p className="text-sm text-primary">مختاراتنا لكِ</p><h2 className="mt-2 font-display text-4xl text-primary">الأكثر طلباً</h2></div><Button variant="outline" asChild><Link href="/shop">شاهدي الكل <ArrowLeft className="mr-2 h-4 w-4" /></Link></Button></div><div className="grid gap-x-5 gap-y-12 sm:grid-cols-2 lg:grid-cols-4">{featured.map((product) => <ProductCard key={product.id} product={product} />)}</div></div>
      </section>

      <section className="mx-auto max-w-7xl px-5 py-20"><div className="overflow-hidden rounded-[2rem] bg-primary px-8 py-12 text-primary-foreground md:px-16 md:py-16"><div className="grid items-center gap-8 md:grid-cols-[1fr_auto]"><div><p className="text-sm text-accent">تجربة إيليجانزا</p><h2 className="mt-3 max-w-xl font-display text-4xl leading-tight md:text-5xl">فستانك المثالي يبدأ من هنا.</h2><p className="mt-4 max-w-lg leading-7 text-primary-foreground/70">تواصلي معنا لنساعدكِ في اختيار القصة والمقاس المناسبين لمناسبتك القادمة.</p></div><Button variant="soft" size="lg" className="w-fit bg-accent text-primary hover:bg-accent/90">تواصلي معنا <ArrowLeft className="mr-2 h-4 w-4" /></Button></div></div></section>
    </main>
  );
}
