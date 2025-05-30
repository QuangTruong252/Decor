import { Dialog, DialogContent, DialogTitle, DialogDescription, DialogHeader, DialogFooter } from "../ui/dialog";
import { Button } from "../ui/button";
import { DestinationSelect } from "./DestinationSelect";
import { FolderStructure } from "@/types/fileManager";
import { useFileManager } from "@/hooks/useFileManager";
import { useState } from "react";

interface MoveFileDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}
export const MoveFileDialog = ({
    open,
    onOpenChange,
}: MoveFileDialogProps) => {
    const { selectedCount, rootFolderStructure, moveSelectedItems } = useFileManager();
    const [destinationPath, setDestinationPath] = useState<string>('');

    const folderOptions: FolderStructure[] = rootFolderStructure?.subfolders || [];
    const handleMove = () => {
        if (destinationPath) {
            moveSelectedItems(destinationPath);
            onOpenChange(false);
            setDestinationPath("");
        }
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
                    <DestinationSelect options={folderOptions} onChange={setDestinationPath} value={destinationPath} />
                </div>

                <DialogFooter>
                    <Button variant="outline" onClick={() => onOpenChange(false)}>
                        Cancel
                    </Button>
                    <Button onClick={handleMove} disabled={!destinationPath}>
                        Move Items
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    )
}