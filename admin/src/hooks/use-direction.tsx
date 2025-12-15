"use client";

import React, { createContext, useContext, useState, useEffect } from "react";

type Direction = "ltr" | "rtl";

interface DirectionContextType {
  direction: Direction;
  setDirection: (dir: Direction) => void;
  toggleDirection: () => void;
}

const DirectionContext = createContext<DirectionContextType | undefined>(
  undefined
);

export function DirectionProvider({ children }: { children: React.ReactNode }) {
  const [direction, setDirectionState] = useState<Direction>("ltr");

  useEffect(() => {
    const saved = localStorage.getItem("app-direction") as Direction;
    if (saved) {
      setDirectionState(saved);
      document.documentElement.dir = saved;
    }
  }, []);

  const setDirection = (dir: Direction) => {
    setDirectionState(dir);
    document.documentElement.dir = dir;
    localStorage.setItem("app-direction", dir);
  };

  const toggleDirection = () => {
    const newDir = direction === "ltr" ? "rtl" : "ltr";
    setDirection(newDir);
  };

  return (
    <DirectionContext.Provider
      value={{ direction, setDirection, toggleDirection }}
    >
      {children}
    </DirectionContext.Provider>
  );
}

export function useDirection() {
  const context = useContext(DirectionContext);
  if (context === undefined) {
    throw new Error("useDirection must be used within a DirectionProvider");
  }
  return context;
}
