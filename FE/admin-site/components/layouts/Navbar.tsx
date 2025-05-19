"use client";

import { usePathname } from "next/navigation";
import { User2 } from "lucide-react";
import { useAuthStore } from "@/stores/auth";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { Button } from "@/components/ui/button";

const PAGE_TITLES: Record<string, string> = {
  "/dashboard": "Dashboard",
  "/products": "Products",
  "/categories": "Categories",
  "/orders": "Orders",
  "/customers": "Customers",
  "/settings": "Settings",
};

export function Navbar() {
  const pathname = usePathname();
  const user = useAuthStore((state) => state.user);
  const logout = useAuthStore((state) => state.logout);

  const pageTitle = PAGE_TITLES[pathname] || "Admin";

  return (
    <header className="flex h-16 items-center justify-between border-b px-6">
      <h1 className="text-lg font-semibold">{pageTitle}</h1>
      <Popover>
        <PopoverTrigger asChild>
          <Button
            variant="ghost"
            size="icon"
            aria-label="User menu"
            className="rounded-full focus:outline-none focus:ring-2 focus:ring-primary"
          >
            <User2 className="h-6 w-6" />
          </Button>
        </PopoverTrigger>
        <PopoverContent align="end" className="w-56 p-4">
          <div className="flex flex-col items-center gap-2">
            <div className="h-12 w-12 rounded-full bg-primary/10 flex items-center justify-center">
              <User2 className="h-8 w-8 text-primary" />
            </div>
            <div className="text-center">
              <p className="text-base font-medium">{user?.name || "Admin User"}</p>
              <p className="text-xs text-muted-foreground">{user?.email || "admin@decorstore.com"}</p>
            </div>
            <Button
              variant="destructive"
              className="mt-2 w-full"
              onClick={logout}
              aria-label="Logout"
            >
              Logout
            </Button>
          </div>
        </PopoverContent>
      </Popover>
    </header>
  );
}