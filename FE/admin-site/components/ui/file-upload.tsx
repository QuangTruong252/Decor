/**
 * File Upload Component with Drag & Drop Support
 */

import React, { useCallback, useState } from 'react';
import { useDropzone } from 'react-dropzone';
import { Upload, File, X, AlertCircle, CheckCircle } from 'lucide-react';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { excelUtils } from '@/services/excel';

interface FileUploadProps {
  onFileSelect: (file: File | null) => void;
  accept?: string;
  maxSize?: number; // in MB
  disabled?: boolean;
  error?: string;
  progress?: number;
  isUploading?: boolean;
  className?: string;
}

export function FileUpload({
  onFileSelect,
  accept = '.xlsx,.xls,.csv',
  maxSize = 10,
  disabled = false,
  error,
  progress,
  isUploading = false,
  className
}: FileUploadProps) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [dragError, setDragError] = useState<string>('');

  const onDrop = useCallback((acceptedFiles: File[], rejectedFiles: any[]) => {
    setDragError('');

    if (rejectedFiles.length > 0) {
      const rejection = rejectedFiles[0];
      if (rejection.errors.some((e: any) => e.code === 'file-too-large')) {
        setDragError(`File size must be less than ${maxSize}MB`);
      } else if (rejection.errors.some((e: any) => e.code === 'file-invalid-type')) {
        setDragError('Please select a valid Excel file (.xlsx, .xls, .csv)');
      } else {
        setDragError('Invalid file selected');
      }
      return;
    }

    if (acceptedFiles.length > 0) {
      const file = acceptedFiles[0];
      
      // Additional validation
      if (!excelUtils.isValidExcelFile(file)) {
        setDragError('Please select a valid Excel file (.xlsx, .xls, .csv)');
        return;
      }

      if (!excelUtils.isValidFileSize(file, maxSize)) {
        setDragError(`File size must be less than ${maxSize}MB`);
        return;
      }

      setSelectedFile(file);
      onFileSelect(file);
    }
  }, [maxSize, onFileSelect]);

  const { getRootProps, getInputProps, isDragActive, isDragReject } = useDropzone({
    onDrop,
    accept: {
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': ['.xlsx'],
      'application/vnd.ms-excel': ['.xls'],
      'text/csv': ['.csv']
    },
    maxSize: maxSize * 1024 * 1024,
    multiple: false,
    disabled: disabled || isUploading
  });

  const removeFile = () => {
    setSelectedFile(null);
    setDragError('');
    onFileSelect(null);
  };

  const displayError = error || dragError;

  return (
    <div className={cn('w-full', className)}>
      {!selectedFile ? (
        <div
          {...getRootProps()}
          className={cn(
            'border-2 border-dashed rounded-lg p-6 text-center cursor-pointer transition-colors',
            'hover:border-primary/50 hover:bg-muted/50',
            isDragActive && !isDragReject && 'border-primary bg-primary/5',
            isDragReject && 'border-destructive bg-destructive/5',
            disabled && 'cursor-not-allowed opacity-50',
            displayError && 'border-destructive'
          )}
        >
          <input {...getInputProps()} />
          <div className="flex flex-col items-center gap-2">
            <Upload className={cn(
              'h-8 w-8',
              isDragActive && !isDragReject && 'text-primary',
              isDragReject && 'text-destructive',
              displayError && 'text-destructive'
            )} />
            <div className="text-sm">
              {isDragActive ? (
                isDragReject ? (
                  <span className="text-destructive">Invalid file type</span>
                ) : (
                  <span className="text-primary">Drop the file here</span>
                )
              ) : (
                <>
                  <span className="font-medium">Click to upload</span> or drag and drop
                </>
              )}
            </div>
            <div className="text-xs text-muted-foreground">
              Excel files (.xlsx, .xls, .csv) up to {maxSize}MB
            </div>
          </div>
        </div>
      ) : (
        <div className="border rounded-lg p-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <File className="h-8 w-8 text-blue-500" />
              <div>
                <div className="font-medium text-sm">{selectedFile.name}</div>
                <div className="text-xs text-muted-foreground">
                  {excelUtils.formatFileSize(selectedFile.size)}
                </div>
              </div>
            </div>
            {!isUploading && (
              <Button
                variant="ghost"
                size="sm"
                onClick={removeFile}
                className="h-8 w-8 p-0"
              >
                <X className="h-4 w-4" />
              </Button>
            )}
          </div>
          
          {isUploading && typeof progress === 'number' && (
            <div className="mt-3">
              <div className="flex items-center justify-between text-xs mb-1">
                <span>Uploading...</span>
                <span>{progress}%</span>
              </div>
              <Progress value={progress} className="h-2" />
            </div>
          )}
        </div>
      )}

      {displayError && (
        <Alert variant="destructive" className="mt-3">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>{displayError}</AlertDescription>
        </Alert>
      )}
    </div>
  );
}
