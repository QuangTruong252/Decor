'use client'

import React, { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { BannerDTO, getBanners, createBanner, updateBanner, deleteBanner } from "@/services/banners";
import { BannerTable } from "@/components/banners/BannerTable";
import { BannerDialog } from "@/components/banners/BannerDialog";
import { useConfirmationDialog } from "@/components/ui/confirmation-dialog";
import { Loader2 } from "lucide-react";

const BannersPage = () => {
    const [banners, setBanners] = useState<BannerDTO[]>([]);
    const [loading, setLoading] = useState(true);
    const [dialogOpen, setDialogOpen] = useState(false);
    const [editBanner, setEditBanner] = useState<BannerDTO | null>(null);
    const [dialogLoading, setDialogLoading] = useState(false);
    const { confirm } = useConfirmationDialog();
    const fetchBanners = async () => {
        setLoading(true);
        try {
            const data = await getBanners();
            setBanners(data);
        } catch (e) {
            // Handle error (could show toast)
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchBanners();
    }, []);

    const handleAdd = () => {
        setEditBanner(null);
        setDialogOpen(true);
    };

    const handleEdit = (banner: BannerDTO) => {
        setEditBanner(banner);
        setDialogOpen(true);
    };

    const handleDelete = async (banner: BannerDTO) => {
        const handleDeleteConfirmed = async (banner: BannerDTO) => {
            await deleteBanner(banner.id);
            await fetchBanners();
        }
        confirm({
            title: "Delete Banner", // TODO: i18n api
            message: "Are you sure you want to delete this banner?",
            confirmText: "Delete", // TODO: i18n api
            cancelText: "Cancel", // TODO: i18n api
            onConfirm: () => handleDeleteConfirmed(banner),
        })
    };

    const handleDialogSubmit = async (data: any) => {
        setDialogLoading(true);
        try {
            if (editBanner) {
                await updateBanner(editBanner.id, data);
            } else {
                await createBanner(data);
            }
            setDialogOpen(false);
            await fetchBanners();
        } catch (e) {
            // Handle error
        } finally {
            setDialogLoading(false);
        }
    };

    return (
        <div className="p-6">
            <div className="flex items-center justify-between mb-6">
                <h1 className="text-2xl font-bold">Banners</h1>
                <Button variant="default" onClick={handleAdd}>Add Banner</Button>
            </div>
            {loading ? (
                <div className="flex h-96 items-center justify-center">
                    <Loader2 className="h-8 w-8 animate-spin text-primary" />
                </div>
            ) : (
                <BannerTable banners={banners} onEdit={handleEdit} onDelete={handleDelete} />
            )}
            <BannerDialog
                open={dialogOpen}
                onClose={() => setDialogOpen(false)}
                onSubmit={handleDialogSubmit}
                initialData={editBanner || undefined}
                isEdit={!!editBanner}
            />
        </div>
    );
};

export default BannersPage;