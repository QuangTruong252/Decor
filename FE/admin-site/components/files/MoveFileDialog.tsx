import { Dialog, DialogContent, DialogTitle, DialogDescription, DialogHeader, DialogFooter } from "../ui/dialog";
import { Button } from "../ui/button";
import { DestinationSelect } from "./DestinationSelect";
import { FolderStructure } from "@/types/fileManager";
import { useFileManager } from "@/hooks/useFileManager";
import { useState, useEffect } from "react";

interface MoveFileDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export const MoveFileDialog = ({
    open,
    onOpenChange,
}: MoveFileDialogProps) => {
    const { selectedCount, rootFolderStructure, moveSelectedItems, isMoving } = useFileManager();
    const [destinationPath, setDestinationPath] = useState<string>('');

    const folderOptions: FolderStructure[] = rootFolderStructure?.subfolders || [];
    
    // Reset destination path when dialog opens/closes
    useEffect(() => {
        if (!open) {
            setDestinationPath('');
        }
    }, [open]);

    const handleMove = async () => {
        if (destinationPath !== undefined) {
            try {
                await moveSelectedItems(destinationPath);
                onOpenChange(false);
            } catch (error) {
                // Error handling is done in the hook
                console.error('Move operation failed:', error);
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
                    <DialogTitle>Move Items</DialogTitle>
                    <DialogDescription>
                        Select the destination folder for {selectedCount} selected item{selectedCount !== 1 ? "s" : ""}.
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
                    <Button variant="outline" onClick={handleCancel} disabled={isMoving}>
                        Cancel
                    </Button>
                    <Button 
                        onClick={handleMove} 
                        disabled={destinationPath === undefined || isMoving}
                    >
                        {isMoving ? "Moving..." : "Move Items"}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};
