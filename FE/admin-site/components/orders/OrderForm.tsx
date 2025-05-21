"use client";

import { useState, useEffect } from "react";
import { useForm, useFieldArray } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Loader2, Plus, Trash2 } from "lucide-react";
import { OrderDTO, CreateOrderPayload, UpdateOrderPayload } from "@/services/orders";
import { getProducts, Product } from "@/services/products";
import { getCustomers, CustomerDTO } from "@/services/customers";

// Order form validation schema
const orderFormSchema = z.object({
  userId: z.string().min(1, "Customer is required"),
  paymentMethod: z.string().min(1, "Payment method is required"),
  shippingAddress: z.string().min(1, "Shipping address is required"),
  notes: z.string().min(1, "Notes are required"),
  contactEmail: z.string().email("Invalid email address"),
  contactPhone: z.string().min(1, "Contact phone is required"),
  shippingCity: z.string().min(1, "Shipping city is required"),
  shippingState: z.string().min(1, "Shipping state is required"),
  shippingCountry: z.string().min(1, "Shipping country is required"),
  shippingPostalCode: z.string().min(1, "Shipping postal code is required"),
  orderItems: z.array(
    z.object({
      productId: z.string().min(1, "Product is required"),
      quantity: z.string().min(1, "Quantity is required").transform(val => parseInt(val, 10)),
    })
  ).min(1, "At least one product is required"),
});

// Update order form validation schema
const updateOrderFormSchema = z.object({
  paymentMethod: z.string().min(1, "Payment method is required"),
  shippingAddress: z.string().min(1, "Shipping address is required"),
  notes: z.string().min(1, "Notes are required"),
  contactEmail: z.string().email("Invalid email address"),
  contactPhone: z.string().min(1, "Contact phone is required"),
  shippingCity: z.string().min(1, "Shipping city is required"),
  shippingState: z.string().min(1, "Shipping state is required"),
  shippingCountry: z.string().min(1, "Shipping country is required"),
  shippingPostalCode: z.string().min(1, "Shipping postal code is required"),
});

type OrderFormValues = z.infer<typeof orderFormSchema>;
type UpdateOrderFormValues = z.infer<typeof updateOrderFormSchema>;

// Payment method options
const PAYMENT_METHODS = [
  "Credit Card",
  "PayPal",
  "Bank Transfer",
  "Cash on Delivery",
];

interface OrderFormProps {
  initialData?: OrderDTO;
  onSubmit: (data: CreateOrderPayload | UpdateOrderPayload) => Promise<void>;
  isSubmitting: boolean;
  isEdit?: boolean;
}

export function OrderForm({ initialData, onSubmit, isSubmitting, isEdit = false }: OrderFormProps) {
  const [products, setProducts] = useState<Product[]>([]);
  const [customers, setCustomers] = useState<CustomerDTO[]>([]);
  const [selectedCustomer, setSelectedCustomer] = useState<CustomerDTO | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const schema = isEdit ? updateOrderFormSchema : orderFormSchema;

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
    setValue,
  } = useForm<OrderFormValues | UpdateOrderFormValues>({
    resolver: zodResolver(schema),
    defaultValues: isEdit
      ? {
          paymentMethod: initialData?.paymentMethod || "",
          shippingAddress: initialData?.shippingAddress || "",
          notes: initialData?.notes || "",
          contactEmail: initialData?.contactEmail || "",
          contactPhone: initialData?.contactPhone || "",
          shippingCity: initialData?.shippingCity || "",
          shippingState: initialData?.shippingState || "",
          shippingCountry: initialData?.shippingCountry || "",
          shippingPostalCode: initialData?.shippingPostalCode || "",
        }
      : {
          userId: "",
          paymentMethod: "",
          shippingAddress: "",
          notes: "",
          contactEmail: "",
          contactPhone: "",
          shippingCity: "",
          shippingState: "",
          shippingCountry: "",
          shippingPostalCode: "",
          orderItems: [{ productId: "", quantity: "1" }],
        },
  });

  const { fields, append, remove } = useFieldArray({
    control,
    name: "orderItems",
  });

  // Fetch products and customers
  useEffect(() => {
    const fetchData = async () => {
      try {
        const [productsData, customersData] = await Promise.all([
          getProducts(),
          getCustomers(),
        ]);
        setProducts(productsData);
        setCustomers(customersData);
      } catch (error) {
        console.error("Error fetching data:", error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, []);

  // Handle customer selection to auto-populate contact and shipping information
  const handleCustomerChange = (customerId: string) => {
    setValue("userId", customerId);

    const selectedCustomer = customers.find(c => c.id.toString() === customerId);
    if (selectedCustomer) {
      // Auto-populate contact information
      setValue("contactEmail", selectedCustomer.email || "");
      setValue("contactPhone", selectedCustomer.phone || "");

      // Auto-populate shipping information
      setValue("shippingAddress", selectedCustomer.address || "");
      setValue("shippingCity", selectedCustomer.city || "");
      setValue("shippingState", selectedCustomer.state || "");
      setValue("shippingCountry", selectedCustomer.country || "");
      setValue("shippingPostalCode", selectedCustomer.postalCode || "");
    }
  };

  const onFormSubmit = async (data: OrderFormValues | UpdateOrderFormValues) => {
    try {
      if (isEdit) {
        // Handle update
        const updateData = data as UpdateOrderFormValues;
        await onSubmit({
          paymentMethod: updateData.paymentMethod,
          shippingAddress: updateData.shippingAddress,
          notes: updateData.notes,
          contactEmail: updateData.contactEmail,
          contactPhone: updateData.contactPhone,
          shippingCity: updateData.shippingCity,
          shippingState: updateData.shippingState,
          shippingCountry: updateData.shippingCountry,
          shippingPostalCode: updateData.shippingPostalCode,
        });
      } else {
        // Handle create
        const createData = data as OrderFormValues;
        await onSubmit({
          userId: parseInt(createData.userId, 10),
          paymentMethod: createData.paymentMethod,
          shippingAddress: createData.shippingAddress,
          notes: createData.notes,
          contactEmail: createData.contactEmail,
          contactPhone: createData.contactPhone,
          shippingCity: createData.shippingCity,
          shippingState: createData.shippingState,
          shippingCountry: createData.shippingCountry,
          shippingPostalCode: createData.shippingPostalCode,
          orderItems: createData.orderItems.map(item => ({
            productId: parseInt(item.productId, 10),
            quantity: item.quantity,
          })),
        });
      }
    } catch (error) {
      console.error("Form submission error:", error);
    }
  };

  if (isLoading) {
    return (
      <div className="flex h-40 items-center justify-center">
        <Loader2 className="h-6 w-6 animate-spin text-primary" />
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-6">
      {!isEdit && (
        <div className="space-y-2">
          <Label htmlFor="userId">Customer</Label>
          <p className="text-sm text-muted-foreground mb-2">
            Select a customer to automatically populate contact and shipping information.
          </p>
          <Select
            onValueChange={handleCustomerChange}
            defaultValue={(initialData?.userId || "").toString()}
          >
            <SelectTrigger id="userId" className={errors.userId ? "border-destructive" : ""}>
              <SelectValue placeholder="Select customer" />
            </SelectTrigger>
            <SelectContent>
              {customers.map((customer) => (
                <SelectItem key={customer.id} value={customer.id.toString()}>
                  {`${customer.firstName || ""} ${customer.lastName || ""}`.trim() || `Customer #${customer.id}`}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          {errors.userId && (
            <p className="text-xs text-destructive">{errors.userId.message}</p>
          )}
        </div>
      )}

      <div className="space-y-2">
        <Label htmlFor="paymentMethod">Payment Method</Label>
        <Select
          onValueChange={(value) => setValue("paymentMethod", value)}
          defaultValue={initialData?.paymentMethod || ""}
        >
          <SelectTrigger id="paymentMethod" className={errors.paymentMethod ? "border-destructive" : ""}>
            <SelectValue placeholder="Select payment method" />
          </SelectTrigger>
          <SelectContent>
            {PAYMENT_METHODS.map((method) => (
              <SelectItem key={method} value={method}>
                {method}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        {errors.paymentMethod && (
          <p className="text-xs text-destructive">{errors.paymentMethod.message}</p>
        )}
      </div>

      <div className="space-y-2">
        <div className="flex items-center justify-between">
          <Label htmlFor="shippingAddress">Shipping Address</Label>
          <span className="text-xs text-muted-foreground">Auto-populated from customer data</span>
        </div>
        <Input
          id="shippingAddress"
          {...register("shippingAddress")}
          defaultValue={initialData?.shippingAddress || ""}
          className={errors.shippingAddress ? "border-destructive" : ""}
          readOnly
        />
        {errors.shippingAddress && (
          <p className="text-xs text-destructive">{errors.shippingAddress.message}</p>
        )}
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <Label htmlFor="shippingCity">City</Label>
            <span className="text-xs text-muted-foreground">Auto-populated</span>
          </div>
          <Input
            id="shippingCity"
            {...register("shippingCity")}
            defaultValue={initialData?.shippingCity || ""}
            className={errors.shippingCity ? "border-destructive" : ""}
            readOnly
          />
          {errors.shippingCity && (
            <p className="text-xs text-destructive">{errors.shippingCity.message}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="shippingState">State/Province</Label>
          <Input
            id="shippingState"
            {...register("shippingState")}
            defaultValue={initialData?.shippingState || ""}
            className={errors.shippingState ? "border-destructive" : ""}
            readOnly
          />
          {errors.shippingState && (
            <p className="text-xs text-destructive">{errors.shippingState.message}</p>
          )}
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label htmlFor="shippingCountry">Country</Label>
          <Input
            id="shippingCountry"
            {...register("shippingCountry")}
            defaultValue={initialData?.shippingCountry || ""}
            className={errors.shippingCountry ? "border-destructive" : ""}
            readOnly
          />
          {errors.shippingCountry && (
            <p className="text-xs text-destructive">{errors.shippingCountry.message}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="shippingPostalCode">Postal Code</Label>
          <Input
            id="shippingPostalCode"
            {...register("shippingPostalCode")}
            defaultValue={initialData?.shippingPostalCode || ""}
            className={errors.shippingPostalCode ? "border-destructive" : ""}
            readOnly
          />
          {errors.shippingPostalCode && (
            <p className="text-xs text-destructive">{errors.shippingPostalCode.message}</p>
          )}
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <Label htmlFor="contactEmail">Contact Email</Label>
            <span className="text-xs text-muted-foreground">Auto-populated</span>
          </div>
          <Input
            id="contactEmail"
            type="email"
            {...register("contactEmail")}
            defaultValue={initialData?.contactEmail || ""}
            className={errors.contactEmail ? "border-destructive" : ""}
            readOnly
          />
          {errors.contactEmail && (
            <p className="text-xs text-destructive">{errors.contactEmail.message}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="contactPhone">Contact Phone</Label>
          <Input
            id="contactPhone"
            {...register("contactPhone")}
            defaultValue={initialData?.contactPhone || ""}
            className={errors.contactPhone ? "border-destructive" : ""}
            readOnly
          />
          {errors.contactPhone && (
            <p className="text-xs text-destructive">{errors.contactPhone.message}</p>
          )}
        </div>
      </div>

      <div className="space-y-2">
        <Label htmlFor="notes">Notes</Label>
        <Input
          id="notes"
          {...register("notes")}
          defaultValue={initialData?.notes || ""}
          className={errors.notes ? "border-destructive" : ""}
        />
        {errors.notes && (
          <p className="text-xs text-destructive">{errors.notes.message}</p>
        )}
      </div>

      {!isEdit && (
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <Label>Order Items</Label>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => append({ productId: "", quantity: "1" })}
              className="h-8 gap-1"
            >
              <Plus className="h-4 w-4" /> Add Item
            </Button>
          </div>

          {fields.map((field, index) => (
            <div key={field.id} className="grid grid-cols-12 gap-2 items-end">
              <div className="col-span-7 space-y-2">
                <Label htmlFor={`orderItems.${index}.productId`}>Product</Label>
                <Select
                  onValueChange={(value) => setValue(`orderItems.${index}.productId`, value)}
                  defaultValue=""
                >
                  <SelectTrigger
                    id={`orderItems.${index}.productId`}
                    className={errors.orderItems?.[index]?.productId ? "border-destructive" : ""}
                  >
                    <SelectValue placeholder="Select product" />
                  </SelectTrigger>
                  <SelectContent>
                    {products.map((product) => (
                      <SelectItem key={product.id} value={product.id.toString()}>
                        {product.name} (${product.price})
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {errors.orderItems?.[index]?.productId && (
                  <p className="text-xs text-destructive">{errors.orderItems[index]?.productId?.message}</p>
                )}
              </div>

              <div className="col-span-3 space-y-2">
                <Label htmlFor={`orderItems.${index}.quantity`}>Quantity</Label>
                <Input
                  id={`orderItems.${index}.quantity`}
                  type="number"
                  min="1"
                  {...register(`orderItems.${index}.quantity`)}
                  className={errors.orderItems?.[index]?.quantity ? "border-destructive" : ""}
                />
                {errors.orderItems?.[index]?.quantity && (
                  <p className="text-xs text-destructive">{errors.orderItems[index]?.quantity?.message}</p>
                )}
              </div>

              <div className="col-span-2 flex justify-end">
                <Button
                  type="button"
                  variant="outline"
                  size="icon"
                  onClick={() => remove(index)}
                  className="h-10 w-10 text-destructive hover:bg-destructive/10 hover:text-destructive"
                  disabled={fields.length === 1}
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              </div>
            </div>
          ))}
          {errors.orderItems && typeof errors.orderItems === 'object' && 'message' in errors.orderItems && (
            <p className="text-xs text-destructive">{errors.orderItems.message as string}</p>
          )}
        </div>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              {isEdit ? "Updating..." : "Creating..."}
            </>
          ) : (
            <>{isEdit ? "Update Order" : "Create Order"}</>
          )}
        </Button>
      </div>
    </form>
  );
}
