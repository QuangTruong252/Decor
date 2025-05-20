import React from "react";
import { BannerDTO } from "@/services/banners";
import { Button } from "@/components/ui/button";
import { Pencil, Trash2 } from "lucide-react";
import { getImageUrl } from "@/lib/utils";

interface BannerTableProps {
  banners: BannerDTO[];
  onEdit: (banner: BannerDTO) => void;
  onDelete: (banner: BannerDTO) => void;
}

export const BannerTable: React.FC<BannerTableProps> = ({ banners, onEdit, onDelete }) => {
  return (
    <div className="overflow-x-auto rounded-lg border">
      <table className="min-w-full text-sm">
        <thead className="bg-muted">
          <tr>
            <th className="px-4 py-2 text-left">Image</th>
            <th className="px-4 py-2 text-left">Title</th>
            <th className="px-4 py-2 text-left">Link</th>
            <th className="px-4 py-2 text-left">Active</th>
            <th className="px-4 py-2 text-left">Order</th>
            <th className="px-4 py-2 text-left">Created At</th>
            <th className="px-4 py-2 text-left">Actions</th>
          </tr>
        </thead>
        <tbody>
          {banners.length === 0 ? (
            <tr>
              <td colSpan={7} className="px-4 py-6 text-center text-muted-foreground">
                No banners found.
              </td>
            </tr>
          ) : (
            banners.map((banner) => (
              <tr key={banner.id} className="border-t">
                <td className="px-4 py-2">
                  {banner.imageUrl ? (
                    <img src={getImageUrl(banner.imageUrl)} alt={banner.title || "Banner"} className="h-12 w-24 object-cover rounded" />
                  ) : (
                    <span className="text-muted-foreground">No image</span>
                  )}
                </td>
                <td className="px-4 py-2">{banner.title}</td>
                <td className="px-4 py-2">
                  {banner.link ? (
                    <a href={banner.link} target="_blank" rel="noopener noreferrer" className="text-blue-600 underline">{banner.link}</a>
                  ) : (
                    <span className="text-muted-foreground">-</span>
                  )}
                </td>
                <td className="px-4 py-2">
                  {banner.isActive ? (
                    <span className="text-green-600 font-semibold">Yes</span>
                  ) : (
                    <span className="text-red-500 font-semibold">No</span>
                  )}
                </td>
                <td className="px-4 py-2">{banner.displayOrder ?? 0}</td>
                <td className="px-4 py-2">{new Date(banner.createdAt).toLocaleString()}</td>
                <td className="px-4 py-2 flex gap-2">
                  <Button variant="outline" size="icon" onClick={() => onEdit(banner)} aria-label="Edit banner">
                    <Pencil className="h-4 w-4" />
                  </Button>
                  <Button variant="destructive" size="icon" onClick={() => onDelete(banner)} aria-label="Delete banner">
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
};