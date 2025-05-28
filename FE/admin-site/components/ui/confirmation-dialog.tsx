"use client"

import { createContext, useState, useCallback, useContext, type ReactNode } from "react"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { AlertCircle, CheckCircle, Loader2 } from "lucide-react"
import { useToast } from "@/hooks/use-toast"

// Types
type ConfirmationOptions = {
  title: string
  message: string
  confirmText?: string
  cancelText?: string
  variant?: "default" | "destructive"
  onConfirm?: () => Promise<void> | void
  onCancel?: () => void
}

type ConfirmationDialogContextType = {
  confirm: (options: ConfirmationOptions) => Promise<boolean>
  closeDialog: () => void
  options: ConfirmationOptions | null
  isOpen: boolean
  isLoading: boolean
  status: "idle" | "loading" | "success" | "error"
  error: string | null
}

// Context
const ConfirmationDialogContext = createContext<ConfirmationDialogContextType | undefined>(undefined)

// Provider Component
export function ConfirmationDialogProvider({ children }: { children: ReactNode }) {
  const [isOpen, setIsOpen] = useState(false)
  const [options, setOptions] = useState<ConfirmationOptions | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [status, setStatus] = useState<"idle" | "loading" | "success" | "error">("idle")
  const [error, setError] = useState<string | null>(null)
  const [resolveRef, setResolveRef] = useState<((value: boolean) => void) | null>(null)
  const { success, error: toastError } = useToast()

  const confirm = useCallback((options: ConfirmationOptions) => {
    setOptions(options)
    setIsOpen(true)
    setStatus("idle")
    setError(null)

    return new Promise<boolean>((resolve) => {
      setResolveRef(() => resolve)
    })
  }, [])

  const handleConfirm = useCallback(async () => {
    if (!options?.onConfirm) {
      if (resolveRef) resolveRef(true)
      setIsOpen(false)
      return
    }

    try {
      setIsLoading(true)
      setStatus("loading")
      await options.onConfirm()
      setStatus("success")
      success({
        title: "Success",
        description: "Operation completed successfully!"
      })
      if (resolveRef) resolveRef(true)
      setTimeout(() => {
        setIsOpen(false)
        setIsLoading(false)
      }, 1000)
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : "An error occurred"
      setStatus("error")
      setError(errorMessage)
      toastError({
        title: "Error",
        description: `Error: ${errorMessage}`
      })
      setIsLoading(false)
      if (resolveRef) resolveRef(false)
    }
  }, [options, resolveRef, success, toastError])

  const handleCancel = useCallback(() => {
    if (options?.onCancel) {
      options.onCancel()
    }
    if (resolveRef) resolveRef(false)
    setIsOpen(false)
  }, [options, resolveRef])

  const closeDialog = useCallback(() => {
    if (isLoading) return
    setIsOpen(false)
    if (resolveRef) resolveRef(false)
  }, [isLoading, resolveRef])

  // Dialog Component
  const ConfirmationDialogComponent = useCallback(() => {
    if (!options) return null

    return (
      <Dialog open={isOpen} onOpenChange={(open) => !open && closeDialog()}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>{options.title}</DialogTitle>
            <DialogDescription>{options.message}</DialogDescription>
          </DialogHeader>

          {status === "error" && (
            <div className="flex items-center gap-2 p-3 text-sm border rounded-md bg-destructive/10 text-destructive border-destructive/20">
              <AlertCircle className="w-4 h-4" />
              <span>{error || "An error occurred"}</span>
            </div>
          )}

          {status === "success" && (
            <div className="flex items-center gap-2 p-3 text-sm border rounded-md bg-green-50 text-green-700 border-green-200 dark:bg-green-900/20 dark:text-green-400 dark:border-green-900/30">
              <CheckCircle className="w-4 h-4" />
              <span>Operation completed successfully</span>
            </div>
          )}

          <DialogFooter className="gap-2">
            <Button variant="outline" onClick={handleCancel} disabled={isLoading}>
              {options.cancelText || "Cancel"}
            </Button>
            <Button
              variant={options.variant || "default"}
              onClick={handleConfirm}
              disabled={isLoading || status === "success"}
            >
              {isLoading ? (
                <>
                  <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                  Processing...
                </>
              ) : (
                options.confirmText || "Confirm"
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    )
  }, [isOpen, options, status, error, isLoading, closeDialog, handleCancel, handleConfirm])

  return (
    <ConfirmationDialogContext.Provider
      value={{
        confirm,
        closeDialog,
        options,
        isOpen,
        isLoading,
        status,
        error,
      }}
    >
      {children}
      <ConfirmationDialogComponent />
    </ConfirmationDialogContext.Provider>
  )
}

// Hook
export function useConfirmationDialog() {
  const context = useContext(ConfirmationDialogContext)

  if (context === undefined) {
    throw new Error("useConfirmationDialog must be used within a ConfirmationDialogProvider")
  }

  return context
}
