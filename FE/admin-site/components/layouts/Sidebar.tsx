"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";
import {
  LayoutDashboard,
  Package,
  ListTree,
  ShoppingCart,
  Users,
  Settings,
  ChevronLeft,
  ChevronRight,
  FolderOpen,
} from "lucide-react";
import { useSidebar } from "./SidebarProvider";
import { Button } from "@/components/ui/button";

const menuItems = [
  {
    title: "Dashboard",
    href: "/dashboard",
    icon: LayoutDashboard,
  },
  {
    title: "Products",
    href: "/products",
    icon: Package,
  },
  {
    title: "Categories",
    href: "/categories",
    icon: ListTree,
  },
  {
    title: "Orders",
    href: "/orders",
    icon: ShoppingCart,
  },
  {
    title: "Customers",
    href: "/customers",
    icon: Users,
  },
  {
    title: "Files",
    href: "/files",
    icon: FolderOpen,
  },
  {
    title: "Banners",
    href: "/banners",
    icon: Package,
  },
  {
    title: "Settings",
    href: "/settings",
    icon: Settings,
  },
];

export function Sidebar() {
  const pathname = usePathname();
  const { isCollapsed, toggle } = useSidebar();

  return (
    <aside
      className={cn(
        "flex h-full flex-col border-r bg-sidebar transition-all duration-300 ease-in-out",
        isCollapsed ? "w-16" : "w-64"
      )}
    >
      {/* Header with toggle button */}
      <div className="flex h-16 items-center border-b px-3">
        {!isCollapsed && (
          <Link href="/dashboard" className="text-xl font-bold text-sidebar-foreground flex-1">
            DecorStore Admin
          </Link>
        )}
        <Button
          variant="ghost"
          size="sm"
          onClick={toggle}
          className="h-8 w-8 p-0 text-sidebar-foreground hover:bg-sidebar-accent"
          aria-label={isCollapsed ? "Expand sidebar" : "Collapse sidebar"}
        >
          {isCollapsed ? (
            <ChevronRight className="h-4 w-4" />
          ) : (
            <ChevronLeft className="h-4 w-4" />
          )}
        </Button>
      </div>

      {/* Navigation */}
      <nav className="flex-1 space-y-1 p-2">
        {menuItems.map((item) => {
          const isActive = pathname.startsWith(item.href);
          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                isActive
                  ? "bg-sidebar-accent text-sidebar-accent-foreground"
                  : "text-sidebar-foreground/60 hover:bg-sidebar-accent/50 hover:text-sidebar-foreground",
                isCollapsed && "justify-center px-2"
              )}
              title={isCollapsed ? item.title : undefined}
            >
              <item.icon className="h-5 w-5 flex-shrink-0" />
              {!isCollapsed && (
                <span className="truncate">{item.title}</span>
              )}
            </Link>
          );
        })}
      </nav>
    </aside>
  );
}
