import Link from "next/link";
import { Check, MessageCircle, PackageCheck } from "lucide-react";
import { Button } from "@/components/ui/button";

export default async function SuccessPage({ searchParams }: { searchParams: Promise<{ order?: string }> }) {
  const { order } = await searchParams;
  return <main className="mx-auto max-w-2xl px-5 py-24 text-center"><div className="mx-auto flex h-20 w-20 items-center justify-center rounded-full bg-primary text-white"><Check className="h-9 w-9" /></div><p className="mt-8 text-sm text-primary">تم استلام طلبك بنجاح</p><h1 className="mt-3 font-display text-5xl text-primary">شكراً لاختيارك إيليجانزا</h1><p className="mx-auto mt-5 max-w-md leading-8 text-muted-foreground">بنراجع طلبك ونتواصل معكِ قريباً لتأكيد التفاصيل وموعد التوصيل.</p>{order && <div className="mx-auto mt-8 max-w-xs rounded-2xl bg-muted px-5 py-4"><p className="text-xs text-muted-foreground">رقم الطلب</p><p className="mt-1 font-medium tracking-wider text-primary">{order}</p></div>}<div className="mt-10 flex flex-wrap justify-center gap-3"><Button asChild><Link href="/shop">مواصلة التسوق</Link></Button><Button variant="outline"><MessageCircle className="ml-2 h-4 w-4" /> واتساب خدمة العملاء</Button></div><div className="mt-16 flex justify-center gap-8 text-xs text-muted-foreground"><span className="flex items-center gap-2"><PackageCheck className="h-4 w-4" /> تجهيز سريع</span><span className="flex items-center gap-2"><Check className="h-4 w-4" /> دفع عند الاستلام</span></div></main>;
}
