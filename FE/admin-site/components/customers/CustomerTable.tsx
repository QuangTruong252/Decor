"use client";

import { CustomerDTO } from "@/services/customers";
import { format } from "date-fns";
import { CustomerDetailDialog } from "./CustomerDetailDialog";
import { EditCustomerDialog, DeleteCustomerButton } from "./CustomerDialog";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

interface CustomerTableProps {
  customers: CustomerDTO[];
  onDelete: (id: number) => Promise<void>;
  isDeleting?: number | null;
}

export function CustomerTable({ customers, onDelete, isDeleting }: CustomerTableProps) {
  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>ID</TableHead>
          <TableHead>Name</TableHead>
          <TableHead>Email</TableHead>
          <TableHead>Location</TableHead>
          <TableHead>Phone</TableHead>
          <TableHead>Joined</TableHead>
          <TableHead className="text-right">Actions</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {customers.length > 0 ? (
          customers.map((customer) => (
            <TableRow key={customer.id}>
              <TableCell className="font-medium">#{customer.id}</TableCell>
              <TableCell>
                {`${customer.firstName || ""} ${customer.lastName || ""}`.trim() || "N/A"}
              </TableCell>
              <TableCell className="text-muted-foreground">
                {customer.email || "N/A"}
              </TableCell>
              <TableCell className="text-muted-foreground">
                {customer.city && customer.state ? `${customer.city}, ${customer.state}` :
                 customer.city || customer.state || "N/A"}
              </TableCell>
              <TableCell className="text-muted-foreground">
                {customer.phone || "N/A"}
              </TableCell>
              <TableCell className="text-muted-foreground">
                {customer.createdAt ? format(new Date(customer.createdAt), "MMM dd, yyyy") : "N/A"}
              </TableCell>
              <TableCell className="text-right">
                <div className="flex justify-end gap-2">
                  <CustomerDetailDialog customerId={customer.id} />
                  <EditCustomerDialog customer={customer} onSuccess={() => {}} />
                  <DeleteCustomerButton
                    customerId={customer.id}
                    onDelete={onDelete}
                    isDeleting={isDeleting === customer.id}
                  />
                </div>
              </TableCell>
            </TableRow>
          ))
        ) : (
          <TableRow>
            <TableCell colSpan={7} className="h-24 text-center text-muted-foreground">
              No customers found.
            </TableCell>
          </TableRow>
        )}
      </TableBody>
    </Table>
  );
}
