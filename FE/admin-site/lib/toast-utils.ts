"use client"

import { useToast } from "@/hooks/use-toast"

/**
 * Show a success toast notification
 * @param message The message to display
 * @param title Optional title (defaults to "Success")
 */
export function showSuccess(message: string, title = "Success") {
  const { success } = useToast()
  success({
    title,
    description: message,
  })
}

/**
 * Show an error toast notification
 * @param message The message to display
 * @param title Optional title (defaults to "Error")
 */
export function showError(message: string, title = "Error") {
  const { error } = useToast()
  error({
    title,
    description: message,
  })
}

/**
 * Show a warning toast notification
 * @param message The message to display
 * @param title Optional title (defaults to "Warning")
 */
export function showWarning(message: string, title = "Warning") {
  const { warning } = useToast()
  warning({
    title,
    description: message,
  })
}

/**
 * Show an info toast notification
 * @param message The message to display
 * @param title Optional title (defaults to "Information")
 */
export function showInfo(message: string, title = "Information") {
  const { info } = useToast()
  info({
    title,
    description: message,
  })
}

/**
 * Show a generic toast notification
 * @param message The message to display
 * @param title Optional title
 */
export function showToast(message: string, title?: string) {
  const { custom } = useToast()
  custom({
    title,
    description: message,
  })
}