"use client";

import Image from "next/image";
import { cn } from "@/lib/utils";
import { Card, CardContent } from "@/components/ui/card";

interface AuthCardProps {
  children: React.ReactNode;
  className?: string;
}

export function AuthCard({ children, className }: AuthCardProps) {
  return (
    <div className={cn("flex flex-col gap-6", className)}>
      <Card className="overflow-hidden p-0">
        <CardContent className="grid p-0 md:grid-cols-2">
          <div className="p-6 md:p-8">{children}</div>
          <div className="relative hidden md:block">
            <Image
              src="/images/login-hero.jpg"
              alt="Vesmarket"
              fill
              className="object-cover dark:brightness-[0.2] dark:grayscale"
              priority
            />
          </div>
        </CardContent>
      </Card>
      <p className="px-6 text-center text-xs text-muted-foreground">
        Copyright © 2025. Vesmark Elektronik Hizmetler ve Ticaret A.Ş
      </p>
    </div>
  );
}
