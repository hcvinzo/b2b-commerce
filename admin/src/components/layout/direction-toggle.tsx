"use client";

import { Languages } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useDirection } from "@/hooks/use-direction";

export function DirectionToggle() {
  const { direction, setDirection } = useDirection();

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" className="h-9 w-9">
          <Languages className="h-4 w-4" />
          <span className="sr-only">Toggle direction</span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem
          onClick={() => setDirection("ltr")}
          className={direction === "ltr" ? "bg-accent" : ""}
        >
          <span className="mr-2">EN</span>
          Left to Right (LTR)
        </DropdownMenuItem>
        <DropdownMenuItem
          onClick={() => setDirection("rtl")}
          className={direction === "rtl" ? "bg-accent" : ""}
        >
          <span className="mr-2">AR</span>
          Right to Left (RTL)
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
