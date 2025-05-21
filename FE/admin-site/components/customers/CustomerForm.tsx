"use client";

import { useForm } from "react-hook-form";
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
import { Loader2 } from "lucide-react";
import { CustomerDTO, CreateCustomerPayload, UpdateCustomerPayload } from "@/services/customers";

// Customer form validation schema for create
const createCustomerSchema = z.object({
  email: z.string().email("Invalid email address"),
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  phone: z.string().optional(),
  address: z.string().optional(),
  city: z.string().min(1, "City is required"),
  state: z.string().min(1, "State is required"),
  country: z.string().min(1, "Country is required"),
  postalCode: z.string().min(1, "Postal code is required"),
});

// Customer form validation schema for update
const updateCustomerSchema = z.object({
  email: z.string().email("Invalid email address"),
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  phone: z.string().optional(),
  address: z.string().optional(),
  city: z.string().min(1, "City is required"),
  state: z.string().min(1, "State is required"),
  country: z.string().min(1, "Country is required"),
  postalCode: z.string().min(1, "Postal code is required"),
});

type CreateCustomerFormValues = z.infer<typeof createCustomerSchema>;
type UpdateCustomerFormValues = z.infer<typeof updateCustomerSchema>;

// Country options (simplified list for example)
const COUNTRIES = ["United States", "Canada", "United Kingdom", "Australia", "Vietnam", "Other"];

interface CustomerFormProps {
  initialData?: CustomerDTO;
  onSubmit: (data: CreateCustomerPayload | UpdateCustomerPayload) => Promise<void>;
  isSubmitting: boolean;
  isEdit?: boolean;
}

export function CustomerForm({ initialData, onSubmit, isSubmitting, isEdit = false }: CustomerFormProps) {
  const schema = isEdit ? updateCustomerSchema : createCustomerSchema;

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
  } = useForm<CreateCustomerFormValues | UpdateCustomerFormValues>({
    resolver: zodResolver(schema),
    defaultValues: isEdit
      ? {
          email: initialData?.email || "",
          firstName: initialData?.firstName || "",
          lastName: initialData?.lastName || "",
          phone: initialData?.phone || "",
          address: initialData?.address || "",
          city: initialData?.city || "",
          state: initialData?.state || "",
          country: initialData?.country || "",
          postalCode: initialData?.postalCode || "",
        }
      : {
          email: "",
          firstName: "",
          lastName: "",
          phone: "",
          address: "",
          city: "",
          state: "",
          country: "",
          postalCode: "",
        },
  });

  const onFormSubmit = async (data: CreateCustomerFormValues | UpdateCustomerFormValues) => {
    try {
      await onSubmit(data);
    } catch (error) {
      console.error("Form submission error:", error);
    }
  };

  return (
    <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-6">
      <div className="space-y-2">
        <Label htmlFor="email">Email</Label>
        <Input
          id="email"
          type="email"
          {...register("email")}
          defaultValue={initialData?.email || ""}
          className={errors.email ? "border-destructive" : ""}
        />
        {errors.email && (
          <p className="text-xs text-destructive">{errors.email.message}</p>
        )}
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label htmlFor="firstName">First Name</Label>
          <Input
            id="firstName"
            {...register("firstName")}
            defaultValue={initialData?.firstName || ""}
            className={errors.firstName ? "border-destructive" : ""}
          />
          {errors.firstName && (
            <p className="text-xs text-destructive">{errors.firstName.message}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="lastName">Last Name</Label>
          <Input
            id="lastName"
            {...register("lastName")}
            defaultValue={initialData?.lastName || ""}
            className={errors.lastName ? "border-destructive" : ""}
          />
          {errors.lastName && (
            <p className="text-xs text-destructive">{errors.lastName.message}</p>
          )}
        </div>
      </div>

      <div className="space-y-2">
        <Label htmlFor="phone">Phone</Label>
        <Input
          id="phone"
          {...register("phone")}
          defaultValue={initialData?.phone || ""}
        />
      </div>

      <div className="space-y-2">
        <Label htmlFor="address">Address</Label>
        <Input
          id="address"
          {...register("address")}
          defaultValue={initialData?.address || ""}
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label htmlFor="city">City</Label>
          <Input
            id="city"
            {...register("city")}
            defaultValue={initialData?.city || ""}
            className={errors.city ? "border-destructive" : ""}
          />
          {errors.city && (
            <p className="text-xs text-destructive">{errors.city.message}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="state">State/Province</Label>
          <Input
            id="state"
            {...register("state")}
            defaultValue={initialData?.state || ""}
            className={errors.state ? "border-destructive" : ""}
          />
          {errors.state && (
            <p className="text-xs text-destructive">{errors.state.message}</p>
          )}
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label htmlFor="country">Country</Label>
          <Select
            onValueChange={(value) => setValue("country", value)}
            defaultValue={initialData?.country || ""}
          >
            <SelectTrigger
              id="country"
              className={errors.country ? "border-destructive" : ""}
            >
              <SelectValue placeholder="Select country" />
            </SelectTrigger>
            <SelectContent>
              {COUNTRIES.map((country) => (
                <SelectItem key={country} value={country}>
                  {country}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          {errors.country && (
            <p className="text-xs text-destructive">{errors.country.message}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="postalCode">Postal Code</Label>
          <Input
            id="postalCode"
            {...register("postalCode")}
            defaultValue={initialData?.postalCode || ""}
            className={errors.postalCode ? "border-destructive" : ""}
          />
          {errors.postalCode && (
            <p className="text-xs text-destructive">{errors.postalCode.message}</p>
          )}
        </div>
      </div>

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              {isEdit ? "Updating..." : "Creating..."}
            </>
          ) : (
            <>{isEdit ? "Update Customer" : "Create Customer"}</>
          )}
        </Button>
      </div>
    </form>
  );
}
