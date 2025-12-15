"use client";

import { cn } from "@/lib/utils";
import { LoadingSpinner } from "./loading-spinner";

interface PageLoaderProps {
  className?: string;
  message?: string;
}

export function PageLoader({ className, message = "Loading..." }: PageLoaderProps) {
  return (
    <div
      className={cn(
        "flex flex-col items-center justify-center min-h-[400px] gap-4",
        className
      )}
    >
      <LoadingSpinner size="lg" />
      <p className="text-sm text-muted-foreground">{message}</p>
    </div>
  );
}
