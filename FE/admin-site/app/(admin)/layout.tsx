import { AdminLayout } from "@/components/layouts/AdminLayout";
import { ReactNode } from "react";
import { QueryProvider } from "@/providers/QueryProvider";

export default function Layout({ children }: { children: ReactNode }) {
  return (
    <QueryProvider>
      <AdminLayout>{children}</AdminLayout>
    </QueryProvider>
  );
}
