"use client";

import { Bell } from "lucide-react";
import { useAuthStore } from "@/stores/auth";

export function Navbar() {
  const user = useAuthStore((state) => state.user);

  return (
    <header className="flex h-16 items-center justify-between border-b px-6">
      <div className="flex items-center gap-4">
        <h1 className="text-lg font-semibold">Dashboard</h1>
      </div>
      <div className="flex items-center gap-4">
        <button className="rounded-full p-2 text-muted-foreground hover:bg-accent hover:text-accent-foreground">
          <Bell className="h-5 w-5" />
        </button>
        <div className="flex items-center gap-2">
          <div className="h-8 w-8 rounded-full bg-primary/10" />
          <div>
            <p className="text-sm font-medium">{user?.name || "Admin User"}</p>
            <p className="text-xs text-muted-foreground">{user?.email || "admin@decorstore.com"}</p>
          </div>
        </div>
      </div>
    </header>
  );
} 