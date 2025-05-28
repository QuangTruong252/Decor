"use client";

import { CustomersDataTable } from "@/components/customers/CustomersDataTable";

export default function CustomersPage() {
  return <div className="p-6 overflow-y-auto">
    <CustomersDataTable/>
  </div>;
}