/**
 * Import/Export Toolbar Component
 */

import React, { useState } from 'react';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger
} from '@/components/ui/dropdown-menu';
import {
  Upload,
  Download,
  FileSpreadsheet,
  ChevronDown
} from 'lucide-react';
import { ImportDialog } from './import-dialog';
import { ExportDialog } from './export-dialog';
import type {
  ExcelValidationResultDTO,
  ExcelImportResultDTO,
  CategoryImportStatisticsDTO,
  ProductImportStatisticsDTO,
  CustomerImportStatisticsDTO
} from '@/types/excel';

interface ImportExportToolbarProps {
  // Export functions
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  onExportData: (filters: any, format: string) => Promise<Blob>;
  onExportTemplate: (includeExample: boolean) => Promise<Blob>;

  // Import functions
  onValidateImport: (file: File) => Promise<ExcelValidationResultDTO>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  onImportData: (file: File) => Promise<ExcelImportResultDTO<any>>;
  onGetImportStatistics?: (file: File) => Promise<CategoryImportStatisticsDTO | ProductImportStatisticsDTO | CustomerImportStatisticsDTO>;

  // Current state
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  currentFilters?: any;
  hasActiveFilters?: boolean;

  // Configuration
  exportType: 'categories' | 'products' | 'customers';
  disabled?: boolean;

  // Callbacks
  onImportSuccess?: () => void;
}

export function ImportExportToolbar({
  onExportData,
  onExportTemplate,
  onValidateImport,
  onImportData,
  onGetImportStatistics,
  currentFilters = {},
  hasActiveFilters = false,
  exportType,
  disabled = false,
  onImportSuccess
}: ImportExportToolbarProps) {
  const [showImportDialog, setShowImportDialog] = useState(false);
  const [showExportDialog, setShowExportDialog] = useState(false);

  const entityName = exportType === 'categories' ? 'Categories' :
                     exportType === 'products' ? 'Products' : 'Customers';

  return (
    <>
      <div className="flex items-center gap-2">
        {/* Import Dropdown */}
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button
              variant="outline"
              size="sm"
              disabled={disabled}
              className="h-8"
            >
              <Upload className="h-4 w-4 mr-2" />
              Import
              <ChevronDown className="h-4 w-4 ml-1" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="start" className="w-48">
            <DropdownMenuItem onClick={() => setShowImportDialog(true)}>
              <FileSpreadsheet className="h-4 w-4 mr-2" />
              Import from Excel
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>

        {/* Export Dropdown */}
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button
              variant="outline"
              size="sm"
              disabled={disabled}
              className="h-8"
            >
              <Download className="h-4 w-4 mr-2" />
              Export
              <ChevronDown className="h-4 w-4 ml-1" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="start" className="w-48">
            <DropdownMenuItem onClick={() => setShowExportDialog(true)}>
              <FileSpreadsheet className="h-4 w-4 mr-2" />
              Export to Excel
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem
              onClick={async () => {
                try {
                  const blob = await onExportTemplate(true);
                  const link = document.createElement('a');
                  link.href = window.URL.createObjectURL(blob);
                  link.download = `${exportType}_template.xlsx`;
                  link.click();
                  window.URL.revokeObjectURL(link.href);
                } catch (error) {
                  console.error('Template download failed:', error);
                }
              }}
            >
              <Download className="h-4 w-4 mr-2" />
              Download Template
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>

      {/* Import Dialog */}
      <ImportDialog
        open={showImportDialog}
        onOpenChange={setShowImportDialog}
        title={`Import ${entityName}`}
        onValidate={onValidateImport}
        onImport={onImportData}
        onGetStatistics={onGetImportStatistics}
        onSuccess={onImportSuccess}
      />

      {/* Export Dialog */}
      <ExportDialog
        open={showExportDialog}
        onOpenChange={setShowExportDialog}
        title={`Export ${entityName}`}
        onExportData={onExportData}
        currentFilters={currentFilters}
        hasActiveFilters={hasActiveFilters}
        exportType={exportType}
      />
    </>
  );
}
