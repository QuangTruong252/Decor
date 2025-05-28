/**
 * Export Dialog Component for Excel Export functionality
 */

import React, { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Checkbox } from '@/components/ui/checkbox';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Progress } from '@/components/ui/progress';
import {
  Download,
  FileSpreadsheet,
  Filter,
  CheckCircle
} from 'lucide-react';
import { toast } from 'sonner';
import { excelUtils } from '@/services/excel';
import type { ExportOptions } from '@/types/excel';

interface ExportDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  onExportData: (filters: any, format: string) => Promise<Blob>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  currentFilters?: any;
  hasActiveFilters?: boolean;
  exportType: 'categories' | 'products' | 'customers';
}

export function ExportDialog({
  open,
  onOpenChange,
  title,
  onExportData,
  currentFilters = {},
  hasActiveFilters = false,
  exportType
}: ExportDialogProps) {
  const [exportOptions, setExportOptions] = useState<ExportOptions>({
    format: 'xlsx',
    includeFilters: true
  });
  const [isExporting, setIsExporting] = useState(false);
  const [progress, setProgress] = useState(0);

  // Export data mutation
  const exportDataMutation = useMutation({
    mutationFn: async () => {
      const filters = exportOptions.includeFilters ? currentFilters : {};
      return onExportData(filters, exportOptions.format);
    },
    onSuccess: (blob) => {
      const filename = excelUtils.generateFilename(
        `${exportType}_export`,
        exportOptions.format
      );
      excelUtils.downloadBlob(blob, filename);
      toast.success('Export completed successfully!');
      setIsExporting(false);
      onOpenChange(false);
    },
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    onError: (error: any) => {
      toast.error('Export failed: ' + (error.message || 'Unknown error'));
      setIsExporting(false);
    }
  });

  const handleExport = () => {
    setIsExporting(true);
    setProgress(0);

    // Simulate progress for better UX
    const progressInterval = setInterval(() => {
      setProgress(prev => {
        if (prev >= 90) {
          clearInterval(progressInterval);
          return prev;
        }
        return prev + 10;
      });
    }, 200);

    exportDataMutation.mutate();
    
    // Clear progress after completion
    setTimeout(() => {
      setProgress(100);
      clearInterval(progressInterval);
    }, 2000);
  };

  const handleClose = () => {
    if (!isExporting) {
      setExportOptions({
        format: 'xlsx',
        includeFilters: true
      });
      setProgress(0);
      onOpenChange(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Download className="h-5 w-5" />
            {title}
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-6 py-4">

          {/* Format Selection */}
          <div className="space-y-3">
            <Label className="text-sm font-medium">File Format</Label>
            <RadioGroup
              value={exportOptions.format}
              onValueChange={(value: 'xlsx' | 'csv') =>
                setExportOptions(prev => ({ ...prev, format: value }))
              }
              disabled={isExporting}
            >
              <div className="flex items-center space-x-2">
                <RadioGroupItem value="xlsx" id="xlsx" />
                <Label htmlFor="xlsx" className="text-sm flex items-center gap-2">
                  <FileSpreadsheet className="h-4 w-4" />
                  Excel (.xlsx)
                </Label>
              </div>
              <div className="flex items-center space-x-2">
                <RadioGroupItem value="csv" id="csv" />
                <Label htmlFor="csv" className="text-sm flex items-center gap-2">
                  <FileSpreadsheet className="h-4 w-4" />
                  CSV (.csv)
                </Label>
              </div>
            </RadioGroup>
          </div>

          {/* Data Export Options */}
          <div className="space-y-3">
            <Label className="text-sm font-medium">Export Options</Label>
            <div className="space-y-2">
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="includeFilters"
                  checked={exportOptions.includeFilters}
                  onCheckedChange={(checked) =>
                    setExportOptions(prev => ({ ...prev, includeFilters: !!checked }))
                  }
                  disabled={isExporting}
                />
                <Label htmlFor="includeFilters" className="text-sm flex items-center gap-2">
                  <Filter className="h-4 w-4" />
                  Apply current filters
                </Label>
              </div>
              {hasActiveFilters && exportOptions.includeFilters && (
                <Alert>
                  <CheckCircle className="h-4 w-4" />
                  <AlertDescription className="text-xs">
                    Current filters will be applied to the export
                  </AlertDescription>
                </Alert>
              )}
            </div>
          </div>

          {/* Progress Bar */}
          {isExporting && (
            <div className="space-y-2">
              <div className="flex items-center justify-between text-sm">
                <span>Exporting...</span>
                <span>{progress}%</span>
              </div>
              <Progress value={progress} className="h-2" />
            </div>
          )}
        </div>

        <DialogFooter>
          <Button
            variant="outline"
            onClick={handleClose}
            disabled={isExporting}
          >
            Cancel
          </Button>
          <Button
            onClick={handleExport}
            disabled={isExporting}
          >
            <Download className="h-4 w-4 mr-2" />
            Export Data
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
