"use client";

import { ReactNode } from "react";
import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { Sidebar } from "./Sidebar";
import { Navbar } from "./Navbar";
import { SidebarProvider, useSidebar } from "./SidebarProvider";
import { cn } from "@/lib/utils";

interface AdminLayoutProps {
  children: ReactNode;
}

function AdminLayoutContent({ children }: AdminLayoutProps) {
  const { isCollapsed, collapse } = useSidebar();
  const { data: session, status } = useSession();
  const router = useRouter();

  useEffect(() => {
    console.log("[AdminLayout] Session status:", status);
    console.log("[AdminLayout] Session data:", session);
    
    if (status === "loading") return; // Still loading
    
    if (status === "unauthenticated") {
      console.log("[AdminLayout] User not authenticated, redirecting to login");
      router.push("/login");
      return;
    }
  }, [status, session, router]);

  // Show loading while checking authentication
  if (status === "loading") {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="text-center">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent"></div>
          <p className="mt-2 text-sm text-muted-foreground">Loading...</p>
        </div>
      </div>
    );
  }

  // Don't render anything if not authenticated (will redirect)
  if (status === "unauthenticated") {
    return null;
  }

  return (
    <div className="flex h-screen bg-background relative">
      {/* Mobile overlay */}
      {!isCollapsed && (
        <div
          className="fixed inset-0 bg-black/50 z-40 md:hidden"
          onClick={collapse}
          aria-hidden="true"
        />
      )}

      {/* Sidebar */}
      <div className={cn(
        "fixed md:relative z-50 md:z-auto",
        !isCollapsed ? "translate-x-0" : "-translate-x-full md:translate-x-0"
      )}>
        <Sidebar />
      </div>

      {/* Main content */}
      <div className="flex flex-1 flex-col overflow-hidden">
        <Navbar />
        <main className="flex-1 overflow-y-auto">{children}</main>
      </div>
    </div>
  );
}

export function AdminLayout({ children }: AdminLayoutProps) {
  return (
    <SidebarProvider>
      <AdminLayoutContent>{children}</AdminLayoutContent>
    </SidebarProvider>
  );
}
