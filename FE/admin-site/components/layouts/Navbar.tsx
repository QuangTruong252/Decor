"use client";

import { useSession, signOut } from "next-auth/react";
import { useRouter } from "next/navigation";
import { Button } from "../ui/button";
import { Menu } from "lucide-react";
import { useSidebar } from "./SidebarProvider";

export const Navbar = () => {
  const { data: session, status } = useSession();
  const router = useRouter();
  const { toggle } = useSidebar();

  const handleLogout = async () => {
    await signOut({ redirect: true, callbackUrl: "/login" });
  };

  return (
    <nav className="flex h-16 items-center justify-between border-b px-6">
      <div className="flex items-center justify-between w-full">
        {/* Mobile sidebar toggle */}
        <div className="flex items-center">
          <Button
            variant="ghost"
            size="sm"
            onClick={toggle}
            className="h-8 w-8 p-0 md:hidden"
            aria-label="Toggle sidebar"
          >
            <Menu className="h-4 w-4" />
          </Button>
        </div>

        <div className="flex items-center space-x-4">
          {status === "loading" ? (
            <span className="text-sm text-muted-foreground">Loading...</span>
          ) : session?.user ? (
            <>
              <span className="text-sm text-muted-foreground">
                Welcome, {session.user.name || session.user.email}
              </span>
              <Button variant="outline" size="sm" onClick={handleLogout} aria-label="Logout">
                Logout
              </Button>
            </>
          ) : (
            <Button variant="outline" size="sm" onClick={() => router.push("/login")} aria-label="Login">
              Login
            </Button>
          )}
        </div>
      </div>
    </nav>
  );
};