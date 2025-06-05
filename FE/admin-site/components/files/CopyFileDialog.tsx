import { useFileManager } from "@/hooks/useFileManager";
import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogTitle, DialogDescription, DialogHeader, DialogFooter } from "../ui/dialog";
import { Button } from "../ui/button";
import { DestinationSelect } from "./DestinationSelect";

interface CopyFileDialogProps {
    selectedItemPath?: string;
    isCopySingle?: boolean;
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export const CopyFileDialog = ({
    selectedItemPath,
    isCopySingle,
    open,
    onOpenChange,
}: CopyFileDialogProps) => {
    const [destinationPath, setDestinationPath] = useState("");
    const { rootFolderStructure, selectedCount, copySelectedItems, copyItem, isCopying } = useFileManager();
    const folderOptions = rootFolderStructure?.subfolders;

    // Reset destination path when dialog opens/closes
    useEffect(() => {
        if (!open) {
            setDestinationPath('');
        }
    }, [open]);

    const handleCopy = async () => {
        if (destinationPath !== undefined) {
            try {
                if (selectedItemPath && isCopySingle) {
                    await copyItem(selectedItemPath, destinationPath);
                } else {
                    await copySelectedItems(destinationPath);
                }
                onOpenChange(false);
            } catch (error) {
                // Error handling is done in the hook
                console.error('Copy operation failed:', error);
            }
        }
    };

    const handleCancel = () => {
        setDestinationPath('');
        onOpenChange(false);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Copy Items</DialogTitle>
                    <DialogDescription>
                        {(selectedItemPath && isCopySingle) ? 
                        `Select the destination folder to copy file.` : 
                        `Select the destination folder to copy ${selectedCount} selected item${selectedCount !== 1 ? "s" : ""}.`}
                    </DialogDescription>
                </DialogHeader>

                <div className="space-y-4">
                    <DestinationSelect 
                        options={folderOptions} 
                        onChange={setDestinationPath} 
                        value={destinationPath} 
                    />
                </div>

                <DialogFooter>
                    <Button variant="outline" onClick={handleCancel} disabled={isCopying}>
                        Cancel
                    </Button>
                    <Button 
                        onClick={handleCopy} 
                        disabled={destinationPath === undefined || isCopying}
                    >
                        {isCopying ? "Copying..." : "Copy Items"}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};
