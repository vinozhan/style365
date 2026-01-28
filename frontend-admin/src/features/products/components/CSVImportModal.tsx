import { useState, useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import { Upload, FileText, CheckCircle, XCircle, AlertCircle, Download, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Checkbox } from '@/components/ui/checkbox';
import { Label } from '@/components/ui/label';
import { cn } from '@/lib/utils';
import type { BulkImportResult } from '../services/productService';

interface CSVImportModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onImport: (file: File, validateOnly: boolean, skipDuplicates: boolean) => Promise<BulkImportResult>;
  templateUrl: string;
}

type ImportState = 'idle' | 'selected' | 'validating' | 'validated' | 'importing' | 'completed';

export function CSVImportModal({ open, onOpenChange, onImport, templateUrl }: CSVImportModalProps) {
  const [file, setFile] = useState<File | null>(null);
  const [state, setState] = useState<ImportState>('idle');
  const [skipDuplicates, setSkipDuplicates] = useState(true);
  const [result, setResult] = useState<BulkImportResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  const onDrop = useCallback((acceptedFiles: File[]) => {
    if (acceptedFiles.length > 0) {
      setFile(acceptedFiles[0]);
      setState('selected');
      setResult(null);
      setError(null);
    }
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      'text/csv': ['.csv'],
      'application/vnd.ms-excel': ['.csv'],
    },
    maxFiles: 1,
    maxSize: 10 * 1024 * 1024, // 10MB
    disabled: state === 'validating' || state === 'importing',
  });

  const handleValidate = async () => {
    if (!file) return;

    setState('validating');
    setError(null);

    try {
      const importResult = await onImport(file, true, skipDuplicates);
      setResult(importResult);
      setState('validated');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Validation failed');
      setState('selected');
    }
  };

  const handleImport = async () => {
    if (!file) return;

    setState('importing');
    setError(null);

    try {
      const importResult = await onImport(file, false, skipDuplicates);
      setResult(importResult);
      setState('completed');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Import failed');
      setState('validated');
    }
  };

  const handleReset = () => {
    setFile(null);
    setState('idle');
    setResult(null);
    setError(null);
  };

  const handleClose = () => {
    handleReset();
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle>Import Products from CSV</DialogTitle>
          <DialogDescription>
            Upload a CSV file to bulk import products. Download the template for the correct format.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          {/* Template Download */}
          <div className="flex items-center justify-between rounded-lg bg-slate-50 p-3">
            <div className="flex items-center gap-2">
              <FileText className="h-5 w-5 text-slate-500" />
              <span className="text-sm text-slate-700">
                Download the CSV template with sample data
              </span>
            </div>
            <Button variant="outline" size="sm" asChild>
              <a href={templateUrl} download>
                <Download className="mr-2 h-4 w-4" />
                Template
              </a>
            </Button>
          </div>

          {/* File Upload */}
          {state === 'idle' || state === 'selected' ? (
            <>
              <div
                {...getRootProps()}
                className={cn(
                  'flex cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed p-8 transition-colors',
                  isDragActive
                    ? 'border-blue-500 bg-blue-50'
                    : file
                      ? 'border-green-500 bg-green-50'
                      : 'border-slate-300 hover:border-slate-400'
                )}
              >
                <input {...getInputProps()} />

                {file ? (
                  <div className="flex items-center gap-3">
                    <CheckCircle className="h-8 w-8 text-green-500" />
                    <div>
                      <p className="font-medium text-slate-700">{file.name}</p>
                      <p className="text-sm text-slate-500">
                        {(file.size / 1024).toFixed(1)} KB
                      </p>
                    </div>
                  </div>
                ) : (
                  <>
                    <Upload className="mb-2 h-8 w-8 text-slate-400" />
                    <p className="text-sm font-medium text-slate-700">
                      {isDragActive ? 'Drop CSV file here' : 'Drag & drop CSV file, or click to select'}
                    </p>
                    <p className="mt-1 text-xs text-slate-500">CSV files up to 10MB</p>
                  </>
                )}
              </div>

              {/* Options */}
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="skipDuplicates"
                  checked={skipDuplicates}
                  onCheckedChange={(checked) => setSkipDuplicates(!!checked)}
                />
                <Label htmlFor="skipDuplicates" className="text-sm font-normal">
                  Skip rows with existing SKUs (instead of reporting errors)
                </Label>
              </div>
            </>
          ) : null}

          {/* Error Message */}
          {error && (
            <div className="flex items-center gap-2 rounded-lg bg-red-50 p-3 text-red-700">
              <XCircle className="h-5 w-5" />
              <span className="text-sm">{error}</span>
            </div>
          )}

          {/* Validation/Import Results */}
          {result && (state === 'validated' || state === 'completed') && (
            <div className="space-y-3 rounded-lg border p-4">
              <div className="flex items-center gap-2">
                {result.errorCount === 0 ? (
                  <CheckCircle className="h-5 w-5 text-green-500" />
                ) : (
                  <AlertCircle className="h-5 w-5 text-yellow-500" />
                )}
                <span className="font-medium">
                  {state === 'completed' ? 'Import Complete' : 'Validation Complete'}
                </span>
              </div>

              {/* Summary Stats */}
              <div className="grid grid-cols-4 gap-4 text-center">
                <div className="rounded bg-slate-100 p-2">
                  <p className="text-2xl font-bold text-slate-700">{result.totalRows}</p>
                  <p className="text-xs text-slate-500">Total Rows</p>
                </div>
                <div className="rounded bg-green-100 p-2">
                  <p className="text-2xl font-bold text-green-700">{result.successCount}</p>
                  <p className="text-xs text-green-600">
                    {state === 'completed' ? 'Imported' : 'Valid'}
                  </p>
                </div>
                <div className="rounded bg-yellow-100 p-2">
                  <p className="text-2xl font-bold text-yellow-700">{result.skippedCount}</p>
                  <p className="text-xs text-yellow-600">Skipped</p>
                </div>
                <div className="rounded bg-red-100 p-2">
                  <p className="text-2xl font-bold text-red-700">{result.errorCount}</p>
                  <p className="text-xs text-red-600">Errors</p>
                </div>
              </div>

              {/* Error Details */}
              {result.errors.length > 0 && (
                <div className="max-h-40 space-y-2 overflow-y-auto">
                  <p className="text-sm font-medium text-red-600">Errors:</p>
                  {result.errors.slice(0, 10).map((err) => (
                    <div key={err.rowNumber} className="text-xs text-slate-600">
                      <span className="font-medium">Row {err.rowNumber}</span>{' '}
                      ({err.sku || 'no SKU'}): {err.errors.join(', ')}
                    </div>
                  ))}
                  {result.errors.length > 10 && (
                    <p className="text-xs text-slate-500">
                      ... and {result.errors.length - 10} more errors
                    </p>
                  )}
                </div>
              )}
            </div>
          )}

          {/* Loading State */}
          {(state === 'validating' || state === 'importing') && (
            <div className="flex flex-col items-center justify-center py-8">
              <Loader2 className="mb-3 h-8 w-8 animate-spin text-blue-500" />
              <p className="text-sm text-slate-600">
                {state === 'validating' ? 'Validating CSV...' : 'Importing products...'}
              </p>
            </div>
          )}
        </div>

        <DialogFooter>
          {state === 'completed' ? (
            <Button onClick={handleClose}>Done</Button>
          ) : (
            <>
              <Button variant="outline" onClick={handleClose}>
                Cancel
              </Button>

              {state === 'selected' && (
                <Button onClick={handleValidate}>
                  Validate
                </Button>
              )}

              {state === 'validated' && result && result.errorCount === 0 && (
                <>
                  <Button variant="outline" onClick={handleReset}>
                    Choose Different File
                  </Button>
                  <Button onClick={handleImport}>
                    Import {result.successCount} Products
                  </Button>
                </>
              )}

              {state === 'validated' && result && result.errorCount > 0 && (
                <>
                  <Button variant="outline" onClick={handleReset}>
                    Choose Different File
                  </Button>
                  {result.successCount > 0 && (
                    <Button variant="secondary" onClick={handleImport}>
                      Import {result.successCount} Valid Products
                    </Button>
                  )}
                </>
              )}
            </>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
