import Link from "next/link";
import { ArrowRight } from "lucide-react";
import { Button } from "@/components/ui/button";

export default function NotFound() {
  return <main className="mx-auto max-w-2xl px-5 py-28 text-center"><p className="text-sm text-primary">404</p><h1 className="mt-3 font-display text-5xl text-primary">القطعة غير موجودة</h1><p className="mt-4 text-muted-foreground">يمكن تكون خلصت من المجموعة، لكن أكيد في قطعة ثانية تستناكِ.</p><Button className="mt-8" asChild><Link href="/shop"><ArrowRight className="ml-2 h-4 w-4" /> العودة للمتجر</Link></Button></main>;
}
