import { useFileManager } from "@/hooks/useFileManager";
import { useState } from "react";
import { Dialog, DialogContent, DialogTitle, DialogDescription, DialogHeader, DialogFooter } from "../ui/dialog";
import { Button } from "../ui/button";
import { FolderStructure } from "@/types/fileManager";
import { SelectItem, Select, SelectTrigger, SelectValue, SelectContent } from "../ui/select";
import { FolderOpen } from "lucide-react";
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
    const { rootFolderStructure, selectedCount, copySelectedItems, copyItem } = useFileManager();
    const folderOptions = rootFolderStructure?.subfolders;
    const handleCopy = () => {
        if (destinationPath) {
            (selectedItemPath && isCopySingle) ? copyItem(selectedItemPath, destinationPath) : copySelectedItems(destinationPath);
            onOpenChange(false);
            setDestinationPath("");
        }
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
                    <DestinationSelect options={folderOptions} onChange={setDestinationPath} value={destinationPath} />
                </div>

                <DialogFooter>
                    <Button variant="outline" onClick={() => onOpenChange(false)}>
                        Cancel
                    </Button>
                    <Button onClick={handleCopy} disabled={!destinationPath}>
                        Copy Items
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    )
}