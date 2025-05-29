/**
 * Import Dialog Component for Excel Import functionality
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
import { Alert, AlertDescription } from '@/components/ui/alert';
import { FileUpload } from '@/components/ui/file-upload';
import {
  CheckCircle,
  AlertCircle,
  XCircle,
  Upload,
  FileText,
  BarChart3
} from 'lucide-react';
import { toast } from 'sonner';
import type {
  ExcelValidationResultDTO,
  ExcelImportResultDTO,
  ImportProcessState,
  CategoryImportStatisticsDTO,
  ProductImportStatisticsDTO,
  CustomerImportStatisticsDTO
} from '@/types/excel';

interface ImportDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  onValidate: (file: File) => Promise<ExcelValidationResultDTO>;
  onImport: (file: File) => Promise<ExcelImportResultDTO<any>>;
  onGetStatistics?: (file: File) => Promise<CategoryImportStatisticsDTO | ProductImportStatisticsDTO | CustomerImportStatisticsDTO>;
  onSuccess?: () => void;
}

export function ImportDialog({
  open,
  onOpenChange,
  title,
  onValidate,
  onImport,
  onGetStatistics,
  onSuccess
}: ImportDialogProps) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [processState, setProcessState] = useState<ImportProcessState>({
    step: 'upload',
    isProcessing: false
  });

  // Validation mutation
  const validateMutation = useMutation({
    mutationFn: onValidate,
    onSuccess: (result) => {
      setProcessState(prev => ({
        ...prev,
        step: 'preview',
        isProcessing: false,
        validationResult: result
      }));
    },
    onError: (error: any) => {
      setProcessState(prev => ({
        ...prev,
        isProcessing: false,
        error: error.message || 'Validation failed'
      }));
      toast.error('Validation failed');
    }
  });

  // Import mutation
  const importMutation = useMutation({
    mutationFn: onImport,
    onSuccess: (result) => {
      setProcessState(prev => ({
        ...prev,
        step: 'complete',
        isProcessing: false,
        importResult: result
      }));

      if (result.isSuccess) {
        toast.success(`Import completed successfully! ${result.successfulRows} rows imported.`);
        onSuccess?.();
      } else {
        toast.error(`Import completed with errors. ${result.errorRows} rows failed.`);
      }
    },
    onError: (error: any) => {
      setProcessState(prev => ({
        ...prev,
        isProcessing: false,
        error: error.message || 'Import failed'
      }));
      toast.error('Import failed');
      handleClose()
    }
  });

  // Statistics mutation
  const statisticsMutation = useMutation({
    mutationFn: onGetStatistics!,
    onSuccess: (result) => {
      setProcessState(prev => ({
        ...prev,
        statistics: result
      }));
    }
  });

  const handleFileSelect = (file: File | null) => {
    setSelectedFile(file);
    if (file && onGetStatistics) {
      statisticsMutation.mutate(file);
    }
  };

  const handleValidate = () => {
    if (!selectedFile) return;

    setProcessState(prev => ({
      ...prev,
      step: 'validate',
      isProcessing: true,
      error: undefined
    }));

    validateMutation.mutate(selectedFile);
  };

  const handleImport = () => {
    if (!selectedFile) return;

    setProcessState(prev => ({
      ...prev,
      step: 'import',
      isProcessing: true,
      error: undefined
    }));

    importMutation.mutate(selectedFile);
  };

  const handleClose = () => {
    setSelectedFile(null);
    setProcessState({
      step: 'upload',
      isProcessing: false
    });
    onOpenChange(false);
  };

  const renderStepContent = () => {
    switch (processState.step) {
      case 'upload':
        return (
          <div className="space-y-4">
            <FileUpload
              onFileSelect={handleFileSelect}
              error={processState.error}
              disabled={processState.isProcessing}
            />

            {processState.statistics && (
              <div className="bg-muted/50 rounded-lg p-4">
                <div className="flex items-center gap-2 mb-2">
                  <BarChart3 className="h-4 w-4" />
                  <span className="font-medium text-sm">File Statistics</span>
                </div>
                <div className="grid grid-cols-2 gap-2 text-xs">
                  <div>Total Rows: {processState.statistics.totalRows}</div>
                  <div>File Size: {(processState.statistics.fileSizeBytes / 1024 / 1024).toFixed(2)} MB</div>
                  {'newCategories' in processState.statistics && (
                    <>
                      <div>New Categories: {processState.statistics.newCategories}</div>
                      <div>Updated Categories: {processState.statistics.updatedCategories}</div>
                    </>
                  )}
                  {'newProducts' in processState.statistics && (
                    <>
                      <div>New Products: {processState.statistics.newProducts}</div>
                      <div>Updated Products: {processState.statistics.updatedProducts}</div>
                    </>
                  )}
                </div>
              </div>
            )}
          </div>
        );

      case 'validate':
        return (
          <div className="space-y-4">
            <div className="text-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto mb-2"></div>
              <p className="text-sm text-muted-foreground">Validating file...</p>
            </div>
          </div>
        );

      case 'preview':
        const validation = processState.validationResult!;
        return (
          <div className="space-y-4">
            <div className="flex items-center gap-2">
              {validation.isValid ? (
                <CheckCircle className="h-5 w-5 text-green-500" />
              ) : (
                <XCircle className="h-5 w-5 text-red-500" />
              )}
              <span className="font-medium">
                {validation.isValid ? 'File is valid' : 'File has validation errors'}
              </span>
            </div>

            {validation.fileInfo && (
              <div className="bg-muted/50 rounded-lg p-3">
                <div className="text-sm space-y-1">
                  <div>Rows: {validation.fileInfo.rowCount}</div>
                  <div>Columns: {validation.fileInfo.columnCount}</div>
                  <div>Worksheets: {validation.fileInfo.worksheetCount}</div>
                </div>
              </div>
            )}

            {validation.errors && validation.errors.length > 0 && (
              <Alert variant="destructive">
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>
                  <div className="space-y-1">
                    {validation.errors.slice(0, 3).map((error, index) => (
                      <div key={index} className="text-xs">{error}</div>
                    ))}
                    {validation.errors.length > 3 && (
                      <div className="text-xs">... and {validation.errors.length - 3} more errors</div>
                    )}
                  </div>
                </AlertDescription>
              </Alert>
            )}

            {validation.warnings && validation.warnings.length > 0 && (
              <Alert>
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>
                  <div className="space-y-1">
                    {validation.warnings.slice(0, 3).map((warning, index) => (
                      <div key={index} className="text-xs">{warning}</div>
                    ))}
                    {validation.warnings.length > 3 && (
                      <div className="text-xs">... and {validation.warnings.length - 3} more warnings</div>
                    )}
                  </div>
                </AlertDescription>
              </Alert>
            )}
          </div>
        );

      case 'import':
        return (
          <div className="space-y-4">
            <div className="text-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto mb-2"></div>
              <p className="text-sm text-muted-foreground">Importing data...</p>
            </div>
          </div>
        );

      case 'complete':
        const result = processState.importResult!;
        return (
          <div className="space-y-4">
            <div className="flex items-center gap-2">
              {result.isSuccess ? (
                <CheckCircle className="h-5 w-5 text-green-500" />
              ) : (
                <AlertCircle className="h-5 w-5 text-yellow-500" />
              )}
              <span className="font-medium">Import Complete</span>
            </div>

            <div className="grid grid-cols-3 gap-4 text-center">
              <div className="bg-green-50 rounded-lg p-3">
                <div className="text-lg font-semibold text-green-600">{result.successfulRows}</div>
                <div className="text-xs text-green-600">Successful</div>
              </div>
              <div className="bg-red-50 rounded-lg p-3">
                <div className="text-lg font-semibold text-red-600">{result.errorRows}</div>
                <div className="text-xs text-red-600">Errors</div>
              </div>
              <div className="bg-blue-50 rounded-lg p-3">
                <div className="text-lg font-semibold text-blue-600">{result.totalRows}</div>
                <div className="text-xs text-blue-600">Total</div>
              </div>
            </div>

            {result.summary && (
              <Alert>
                <FileText className="h-4 w-4" />
                <AlertDescription>{result.summary}</AlertDescription>
              </Alert>
            )}
          </div>
        );

      default:
        return null;
    }
  };

  const renderFooter = () => {
    switch (processState.step) {
      case 'upload':
        return (
          <>
            <Button variant="outline" onClick={handleClose}>
              Cancel
            </Button>
            <Button
              onClick={handleValidate}
              disabled={!selectedFile || processState.isProcessing}
            >
              <Upload className="h-4 w-4 mr-2" />
              Validate File
            </Button>
          </>
        );

      case 'preview':
        const canImport = processState.validationResult?.isValid;
        return (
          <>
            <Button variant="outline" onClick={() => setProcessState(prev => ({ ...prev, step: 'upload' }))}>
              Back
            </Button>
            <Button
              onClick={handleImport}
              disabled={!canImport || processState.isProcessing}
            >
              <Upload className="h-4 w-4 mr-2" />
              Import Data
            </Button>
          </>
        );

      case 'complete':
        return (
          <Button onClick={handleClose}>
            Close
          </Button>
        );

      default:
        return null;
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>

        <div className="py-4">
          {renderStepContent()}
        </div>

        {processState.error && (
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>{processState.error}</AlertDescription>
          </Alert>
        )}

        <DialogFooter>
          {renderFooter()}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
