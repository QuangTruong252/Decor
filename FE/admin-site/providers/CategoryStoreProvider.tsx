"use client";

import { useEffect } from "react";
import { useSession } from "next-auth/react";
import { useCategoryStore } from "@/stores/categoryStore";

/**
 * Category Store Provider Component
 * 
 * This component initializes the category store when the user is authenticated
 * and resets it when the user logs out.
 */
export function CategoryStoreProvider({ children }: { children: React.ReactNode }) {
  const { data: session, status } = useSession();
  const { initializeCategories, reset, isInitialized } = useCategoryStore();

  useEffect(() => {
    // Initialize categories when user is authenticated
    if (status === "authenticated" && session?.user && !isInitialized) {
      console.log("Initializing category store for authenticated user");
      initializeCategories();
    }
    
    // Reset store when user logs out
    if (status === "unauthenticated") {
      console.log("Resetting category store for unauthenticated user");
      reset();
    }
  }, [status, session, isInitialized, initializeCategories, reset]);

  return <>{children}</>;
}
