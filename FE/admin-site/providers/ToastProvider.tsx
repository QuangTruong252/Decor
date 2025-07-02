"use client"

import type React from "react"
import { createContext, useContext, useState, useCallback, useEffect } from "react"
import { X } from "lucide-react"
import { cn } from "@/lib/utils"

export type ToastType = "success" | "error" | "warning" | "info"

export interface ToastProps {
    id: string
    title?: string
    description?: string
    type?: ToastType
    duration?: number
    action?: React.ReactNode
}

interface ToastContextProps {
    toasts: ToastProps[]
    addToast: (toast: Omit<ToastProps, "id">) => string
    removeToast: (id: string) => void
    updateToast: (id: string, toast: Partial<ToastProps>) => void
}

const ToastContext = createContext<ToastContextProps | undefined>(undefined)

export const useToast = () => {
    const context = useContext(ToastContext)
    if (!context) {
        throw new Error("useToast must be used within a ToastProvider")
    }
    return context
}

export const ToastProvider = ({ children }: { children: React.ReactNode }) => {
    const [toasts, setToasts] = useState<ToastProps[]>([])
    const [counter, setCounter] = useState(0)

    const addToast = useCallback((toast: Omit<ToastProps, "id">) => {
        const id = `toast-${Date.now()}-${counter}`
        setCounter(prev => prev + 1)
        setToasts((prevToasts) => [...prevToasts, { id, ...toast }])
        return id
    }, [counter])

    const removeToast = useCallback((id: string) => {
        setToasts((prevToasts) => prevToasts.filter((toast) => toast.id !== id))
    }, [])

    const updateToast = useCallback((id: string, toast: Partial<ToastProps>) => {
        setToasts((prevToasts) => prevToasts.map((t) => (t.id === id ? { ...t, ...toast } : t)))
    }, [])

    return (
        <ToastContext.Provider value={{ toasts, addToast, removeToast, updateToast }}>
            {children}
            <ToastContainer />
        </ToastContext.Provider>
    )
}

const ToastContainer = () => {
    const { toasts, removeToast } = useToast()

    return (
        <div
            className="fixed top-4 right-4 z-200 flex flex-col gap-2 p-4 max-h-screen overflow-hidden pointer-events-none"
            aria-live="polite"
            role="region"
            aria-label="Notifications"
        >
            {toasts.map((toast) => (
                <ToastItem key={toast.id} toast={toast} onClose={() => removeToast(toast.id)} />
            ))}
        </div>
    )
}

const ToastItem = ({ toast, onClose }: { toast: ToastProps; onClose: () => void }) => {
    const { id, title, description, type = "info", duration = 8000, action } = toast

    useEffect(() => {
        if (duration === Number.POSITIVE_INFINITY) return

        const timer = setTimeout(() => {
            onClose()
        }, duration)

        return () => clearTimeout(timer)
    }, [duration, onClose])

    const getTypeStyles = () => {
        switch (type) {
            case "success":
                return "bg-green-100 border-green-500 text-green-800 dark:bg-green-900/50 dark:border-green-600 dark:text-green-100"
            case "error":
                return "bg-red-100 border-red-500 text-red-800 dark:bg-red-900/50 dark:border-red-600 dark:text-red-100"
            case "warning":
                return "bg-yellow-100 border-yellow-500 text-yellow-800 dark:bg-yellow-900/50 dark:border-yellow-600 dark:text-yellow-100"
            case "info":
            default:
                return "bg-blue-100 border-blue-500 text-blue-800 dark:bg-blue-900/50 dark:border-blue-600 dark:text-blue-100"
        }
    }

    return (
        <div
            className={cn(
                "animate-slide-in pointer-events-auto w-full max-w-sm overflow-hidden rounded-lg border p-4 shadow-md transition-all",
                getTypeStyles(),
            )}
            role="alert"
            aria-labelledby={`toast-${id}-title`}
            data-state="open"
            data-toast-type={type}
        >
            <div className="flex items-start justify-between">
                <div className="flex-1">
                    {title && (
                        <h3 id={`toast-${id}-title`} className="font-medium">
                            {title}
                        </h3>
                    )}
                    {description && <div className="mt-1 text-sm opacity-90">{description}</div>}
                    {action && <div className="mt-2">{action}</div>}
                </div>
                <button
                    onClick={onClose}
                    className="ml-4 inline-flex h-6 w-6 shrink-0 items-center justify-center rounded-md text-current opacity-50 hover:opacity-100 focus:outline-none focus:ring-2 focus:ring-current"
                    aria-label="Close notification"
                >
                    <X className="h-4 w-4" />
                </button>
            </div>
        </div>
    )
}
