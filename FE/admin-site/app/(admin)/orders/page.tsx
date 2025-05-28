"use client";

import { OrdersDataTable } from "@/components/orders/OrdersDataTable";

export default function OrdersPage() {
  return <div className="p-6 overflow-y-auto">
    <OrdersDataTable/>
  </div>;
}