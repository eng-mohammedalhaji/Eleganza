"use client";

import Link from "next/link";
import { FormEvent, useState } from "react";
import { ArrowRight, CheckCircle2, LogIn, UserPlus } from "lucide-react";
import { useRouter } from "next/navigation";
import { ApiClientError, getApiBaseUrl } from "@/lib/api-client";
import { login, register } from "@/lib/auth-api";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

export default function AccountPage() {
  const router = useRouter();
  const [mode, setMode] = useState<"login" | "register">("login");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [registered, setRegistered] = useState(false);

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      if (!getApiBaseUrl()) {
        throw new Error("شغّلي ASP.NET Core API أولاً ثم أعيدي المحاولة.");
      }

      if (mode === "register") {
        await register({ email, password });
        setRegistered(true);
        setMode("login");
      } else {
        await login({ email, password });
        router.push("/vendor");
        router.refresh();
      }
    } catch (caught) {
      setError(caught instanceof ApiClientError || caught instanceof Error ? caught.message : "تعذر تنفيذ العملية.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main className="mx-auto max-w-6xl px-5 py-14 md:py-20">
      <Link href="/" className="mb-8 inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-primary">
        <ArrowRight className="h-4 w-4" /> العودة للرئيسية
      </Link>
      <div className="mx-auto grid max-w-4xl gap-8 lg:grid-cols-[0.8fr_1.2fr]">
        <div className="rounded-[2rem] bg-primary p-8 text-primary-foreground md:p-10">
          <p className="text-sm text-accent">حسابك في إيليجانزا</p>
          <h1 className="mt-4 font-display text-4xl leading-tight md:text-5xl">تسوّقي، أو افتحي متجركِ.</h1>
          <p className="mt-5 leading-8 text-primary-foreground/70">حساب واحد يتيح لكِ متابعة الطلبات أو التقديم كمورد وربط حساب الشحن الخاص بمتجرك.</p>
          <div className="mt-8 space-y-4 text-sm text-primary-foreground/80">
            <p className="flex items-center gap-2"><CheckCircle2 className="h-4 w-4 text-accent" /> جلسة آمنة عبر Cookie</p>
            <p className="flex items-center gap-2"><CheckCircle2 className="h-4 w-4 text-accent" /> لا يتم حفظ Access Token في المتصفح</p>
          </div>
        </div>
        <Card className="p-6 md:p-10">
          <div className="mb-8 flex gap-2 rounded-full bg-muted p-1">
            <button className={`flex flex-1 items-center justify-center gap-2 rounded-full px-4 py-3 text-sm ${mode === "login" ? "bg-white text-primary shadow-sm" : "text-muted-foreground"}`} onClick={() => { setMode("login"); setRegistered(false); }}><LogIn className="h-4 w-4" /> تسجيل الدخول</button>
            <button className={`flex flex-1 items-center justify-center gap-2 rounded-full px-4 py-3 text-sm ${mode === "register" ? "bg-white text-primary shadow-sm" : "text-muted-foreground"}`} onClick={() => { setMode("register"); setRegistered(false); }}><UserPlus className="h-4 w-4" /> حساب جديد</button>
          </div>
          {registered && <p className="mb-5 rounded-2xl bg-emerald-50 p-4 text-sm text-emerald-700">تم إنشاء الحساب. سجّلي الدخول للمتابعة.</p>}
          {error && <p className="mb-5 rounded-2xl bg-red-50 p-4 text-sm text-red-700">{error}</p>}
          <form onSubmit={submit} className="space-y-5">
            <div className="space-y-2"><Label htmlFor="email">البريد الإلكتروني</Label><Input id="email" type="email" value={email} onChange={(event) => setEmail(event.target.value)} required autoComplete="email" /></div>
            <div className="space-y-2"><Label htmlFor="password">كلمة المرور</Label><Input id="password" type="password" value={password} onChange={(event) => setPassword(event.target.value)} required minLength={8} autoComplete={mode === "login" ? "current-password" : "new-password"} /><p className="text-xs text-muted-foreground">8 أحرف على الأقل وتحتوي على رقم.</p></div>
            <Button type="submit" size="lg" className="w-full" disabled={submitting}>{submitting ? "جاري التنفيذ..." : mode === "login" ? "دخول آمن" : "إنشاء الحساب"}</Button>
          </form>
        </Card>
      </div>
    </main>
  );
}
