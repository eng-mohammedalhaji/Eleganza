"use client";

import { useEffect } from "react";
import { Button } from "@/components/ui/button";

export default function ErrorPage({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <main className="mx-auto max-w-2xl px-5 py-24 text-center">
      <p className="text-sm text-primary">تعذر إكمال الصفحة</p>
      <h1 className="mt-3 font-display text-4xl text-primary">الخدمة غير متاحة مؤقتاً</h1>
      <p className="mt-4 text-muted-foreground">{error.message || "حدث خطأ غير متوقع. حاولي مرة أخرى."}</p>
      <Button className="mt-7" onClick={() => reset()}>إعادة المحاولة</Button>
    </main>
  );
}
