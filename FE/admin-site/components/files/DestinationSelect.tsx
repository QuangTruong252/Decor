import { FolderOpen } from "lucide-react";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "../ui/select";
import { FolderStructure } from "@/types/fileManager";

interface DestinationSelectProps {
    options: FolderStructure[] | undefined;
    onChange: (path: string) => void;
    value: string;
}

interface DestinationNodeProps {
    folder: FolderStructure;
    value: string;
    onChange: (path: string) => void;
}

export const DestinationNode = ({
    folder,
    value,
    onChange,
}: DestinationNodeProps) => {
    const hasChildren = folder.subfolders && folder.subfolders.length > 0;

    return (
        <>
            <SelectItem key={folder.relativePath} value={folder.relativePath}>
                <div className="flex items-center gap-2 capitalize">
                    <FolderOpen className="h-4 w-4" />
                    {folder.name}
                </div>
            </SelectItem>
            {hasChildren && (
                <div className="pl-4">
                    {folder.subfolders.map((subfolder) => (
                        <DestinationNode
                            key={subfolder.relativePath}
                            folder={subfolder}
                            value={value}
                            onChange={onChange}
                        />
                    ))}
                </div>
            )}
        </>
    )
}

export const DestinationSelect = ({
    options,
    onChange,
    value,
}: DestinationSelectProps) => {
    if (!options) return null;
    return (
        <div>
            <label className="text-sm font-medium">Destination Folder</label>
            <Select value={value} onValueChange={onChange}>
                <SelectTrigger className="mt-1 w-full">
                    <SelectValue placeholder="Select a folder" />
                </SelectTrigger>
                <SelectContent>
                    {options.map((option) => (
                        <DestinationNode
                            key={option.relativePath}
                            folder={option}
                            value={value}
                            onChange={onChange}
                        />
                    ))}
                </SelectContent>
            </Select>
        </div>
    )
}