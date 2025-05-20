"use client";

import { useSession, signOut } from "next-auth/react";
import { useRouter } from "next/navigation";
import { Button } from "../ui/button";

export const Navbar = () => {
  const { data: session, status } = useSession();
  const router = useRouter();

  const handleLogout = async () => {
    await signOut({ redirect: true, callbackUrl: "/login" });
  };

  return (
    <nav className="flex h-16 items-center justify-between border-b px-6">
      <div className="flex items-center justify-between w-full">
        {/* Placeholder for potential left-aligned items if sidebar is collapsible */}
        <div></div>
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