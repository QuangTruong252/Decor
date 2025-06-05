import { FolderOpen } from "lucide-react";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "../ui/select";
import { FolderStructure } from "@/types/fileManager";

interface DestinationSelectProps {
    options: FolderStructure[] | undefined;
    onChange: (path: string) => void;
    value: string;
}

// Helper function to flatten folder structure for select options
const flattenFolders = (folders: FolderStructure[], level = 0): Array<{ folder: FolderStructure; level: number }> => {
    const result: Array<{ folder: FolderStructure; level: number }> = [];
    
    folders.forEach(folder => {
        result.push({ folder, level });
        
        if (folder.subfolders && folder.subfolders.length > 0) {
            result.push(...flattenFolders(folder.subfolders, level + 1));
        }
    });
    
    return result;
};

export const DestinationSelect = ({
    options,
    onChange,
    value,
}: DestinationSelectProps) => {
    if (!options) return null;

    const flatFolders = flattenFolders(options);
    console.log("Flattened folders:", flatFolders);
    
    // Find selected folder name for display
    const selectedFolder = flatFolders.find(item => item.folder.relativePath === value);
    
    return (
        <div>
            <label className="text-sm font-medium">Destination Folder</label>
            <Select value={value} onValueChange={onChange}>
                <SelectTrigger className="mt-1 w-full">
                    <SelectValue placeholder="Select a folder">
                        {selectedFolder && (
                            <div className="flex items-center gap-2">
                                <FolderOpen className="h-4 w-4" />
                                <span style={{ paddingLeft: `${selectedFolder.level * 16}px` }}>
                                    {selectedFolder.folder.name}
                                </span>
                            </div>
                        )}
                    </SelectValue>
                </SelectTrigger>
                <SelectContent className="max-h-[300px] overflow-y-auto">
                    {/* Add root option */}
                    <SelectItem value="/">
                        <div className="flex items-center gap-2">
                            <FolderOpen className="h-4 w-4" />
                            <span>Root Folder</span>
                        </div>
                    </SelectItem>
                    
                    {flatFolders.map(({ folder, level }) => (
                        <SelectItem key={folder.relativePath} value={folder.relativePath}>
                            <div className="flex items-center gap-2" style={{ paddingLeft: `${level * 16}px` }}>
                                <FolderOpen className="h-4 w-4" />
                                <span className="capitalize truncate" title={folder.name}>
                                    {folder.name}
                                </span>
                            </div>
                        </SelectItem>
                    ))}
                </SelectContent>
            </Select>
        </div>
    );
};
