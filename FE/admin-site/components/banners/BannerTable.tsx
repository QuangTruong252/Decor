"use client";

import React from "react";
import { BannerDTO } from "@/services/banners";
import { Button } from "@/components/ui/button";
import { Pencil, Trash2 } from "lucide-react";
import { getImageUrl } from "@/lib/utils";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

interface BannerTableProps {
  banners: BannerDTO[];
  onEdit: (banner: BannerDTO) => void;
  onDelete: (banner: BannerDTO) => void;
}

export function BannerTable({ banners, onEdit, onDelete }: BannerTableProps) {
  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Image</TableHead>
          <TableHead>Title</TableHead>
          <TableHead>Link</TableHead>
          <TableHead>Active</TableHead>
          <TableHead>Order</TableHead>
          <TableHead>Created At</TableHead>
          <TableHead className="text-right">Actions</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {banners.length === 0 ? (
          <TableRow>
            <TableCell colSpan={7} className="h-24 text-center text-muted-foreground">
              No banners found.
            </TableCell>
          </TableRow>
        ) : (
          banners.map((banner) => (
            <TableRow key={banner.id}>
              <TableCell>
                {banner.imageUrl ? (
                  <img src={getImageUrl(banner.imageUrl)} alt={banner.title || "Banner"} className="h-12 w-24 object-cover rounded" />
                ) : (
                  <span className="text-muted-foreground">No image</span>
                )}
              </TableCell>
              <TableCell>{banner.title}</TableCell>
              <TableCell>
                {banner.link ? (
                  <a href={banner.link} target="_blank" rel="noopener noreferrer" className="text-primary hover:underline">{banner.link}</a>
                ) : (
                  <span className="text-muted-foreground">-</span>
                )}
              </TableCell>
              <TableCell>
                {banner.isActive ? (
                  <span className="text-green-600 font-semibold">Yes</span>
                ) : (
                  <span className="text-red-500 font-semibold">No</span>
                )}
              </TableCell>
              <TableCell>{banner.displayOrder ?? 0}</TableCell>
              <TableCell>{new Date(banner.createdAt).toLocaleString()}</TableCell>
              <TableCell className="text-right">
                <div className="flex justify-end gap-2">
                  <Button variant="outline" size="icon" onClick={() => onEdit(banner)} aria-label="Edit banner">
                    <Pencil className="h-4 w-4" />
                  </Button>
                  <Button variant="destructive" size="icon" onClick={() => onDelete(banner)} aria-label="Delete banner">
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </TableCell>
            </TableRow>
          ))
        )}
      </TableBody>
    </Table>
  );
}