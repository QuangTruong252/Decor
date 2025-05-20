import React, { useState } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { BannerDTO, CreateBannerPayload, UpdateBannerPayload } from "@/services/banners";

type BannerDialogProps = {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateBannerPayload | UpdateBannerPayload) => void;
  initialData?: BannerDTO;
  isEdit?: boolean;
};

export const BannerDialog: React.FC<BannerDialogProps> = ({ open, onClose, onSubmit, initialData, isEdit }) => {
  const [title, setTitle] = useState(initialData?.title || "");
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [link, setLink] = useState(initialData?.link || "");
  const [isActive, setIsActive] = useState(initialData?.isActive ?? true);
  const [displayOrder, setDisplayOrder] = useState(initialData?.displayOrder || 0);
  const [error, setError] = useState("");

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setImageFile(e.target.files[0]);
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) {
      setError("Title is required");
      return;
    }
    if (!isEdit && !imageFile) {
      setError("Image is required");
      return;
    }
    setError("");
    const payload: any = {
      title,
      link,
      isActive,
      displayOrder,
    };
    if (imageFile) payload.imageFile = imageFile;
    onSubmit(payload);
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>{isEdit ? "Edit Banner" : "Add Banner"}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="space-y-2">
            <Label htmlFor="title">Title</Label>
            <Input
              id="title"
              value={title}
              onChange={e => setTitle(e.target.value)}
              required
              maxLength={100}
              placeholder="Enter banner title"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="image">Image</Label>
            <Input
              id="image"
              type="file"
              accept="image/*"
              onChange={handleFileChange}
              required={!isEdit}
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="link">Link</Label>
            <Input
              id="link"
              value={link}
              onChange={e => setLink(e.target.value)}
              maxLength={255}
              placeholder="Enter URL (optional)"
            />
          </div>
          <div className="flex items-center gap-2">
            <Checkbox id="isActive" checked={isActive} onCheckedChange={(checked) => setIsActive(checked === true)} />
            <Label htmlFor="isActive">Active</Label>
          </div>
          <div className="space-y-2">
            <Label htmlFor="displayOrder">Display Order</Label>
            <Input
              id="displayOrder"
              type="number"
              value={displayOrder}
              onChange={e => setDisplayOrder(Number(e.target.value))}
              min={0}
              placeholder="Enter display order"
            />
          </div>
          {error && <div className="text-red-500 text-sm">{error}</div>}
          <DialogFooter>
            <Button type="submit" variant="default">{isEdit ? "Save" : "Add"}</Button>
            <Button type="button" variant="outline" onClick={onClose}>Cancel</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
};