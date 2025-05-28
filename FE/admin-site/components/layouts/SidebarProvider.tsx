"use client";

import { createContext, useContext, useEffect, useState, ReactNode } from "react";

interface SidebarContextType {
  isCollapsed: boolean;
  toggle: () => void;
  collapse: () => void;
  expand: () => void;
}

const SidebarContext = createContext<SidebarContextType | undefined>(undefined);

export function useSidebar() {
  const context = useContext(SidebarContext);
  if (context === undefined) {
    throw new Error("useSidebar must be used within a SidebarProvider");
  }
  return context;
}

interface SidebarProviderProps {
  children: ReactNode;
}

export function SidebarProvider({ children }: SidebarProviderProps) {
  const [isCollapsed, setIsCollapsed] = useState(false);

  // Load saved state from localStorage on mount
  useEffect(() => {
    const saved = localStorage.getItem("sidebar-collapsed");
    const isMobile = window.innerWidth < 768;

    if (isMobile) {
      // Always start collapsed on mobile
      setIsCollapsed(true);
    } else if (saved !== null) {
      // Use saved state on desktop
      setIsCollapsed(JSON.parse(saved));
    }

    // Handle resize events
    const handleResize = () => {
      const isMobileNow = window.innerWidth < 768;
      if (isMobileNow) {
        setIsCollapsed(true);
      }
    };

    window.addEventListener("resize", handleResize);

    return () => window.removeEventListener("resize", handleResize);
  }, []);

  // Save state to localStorage when it changes (desktop only)
  useEffect(() => {
    const isMobile = window.innerWidth < 768;
    if (!isMobile) {
      localStorage.setItem("sidebar-collapsed", JSON.stringify(isCollapsed));
    }
  }, [isCollapsed]);

  const toggle = () => setIsCollapsed(!isCollapsed);
  const collapse = () => setIsCollapsed(true);
  const expand = () => setIsCollapsed(false);

  const value = {
    isCollapsed,
    toggle,
    collapse,
    expand,
  };

  return (
    <SidebarContext.Provider value={value}>
      {children}
    </SidebarContext.Provider>
  );
}
