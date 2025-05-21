"use client"

// Inspired by react-hot-toast library
import type * as React from "react"
import { useToast as useToastInternal } from "@/providers/ToastProvider"
import type { ToastProps } from "@/providers/ToastProvider"

const TOAST_LIMIT = 1
const TOAST_REMOVE_DELAY = 1000000

type ToasterToast = ToastProps & {
  id: string
  title?: React.ReactNode
  description?: React.ReactNode
  action?: React.ReactNode
}

const actionTypes = {
  ADD_TOAST: "ADD_TOAST",
  UPDATE_TOAST: "UPDATE_TOAST",
  DISMISS_TOAST: "DISMISS_TOAST",
  REMOVE_TOAST: "REMOVE_TOAST",
} as const

let count = 0

function genId() {
  count = (count + 1) % Number.MAX_SAFE_INTEGER
  return count.toString()
}

type ActionType = typeof actionTypes

type Action =
  | {
      type: ActionType["ADD_TOAST"]
      toast: ToasterToast
    }
  | {
      type: ActionType["UPDATE_TOAST"]
      toast: Partial<ToasterToast>
    }
  | {
      type: ActionType["DISMISS_TOAST"]
      toastId?: ToasterToast["id"]
    }
  | {
      type: ActionType["REMOVE_TOAST"]
      toastId?: ToasterToast["id"]
    }

interface State {
  toasts: ToasterToast[]
}

const toastTimeouts = new Map<string, ReturnType<typeof setTimeout>>()

const addToRemoveQueue = (toastId: string) => {
  if (toastTimeouts.has(toastId)) {
    return
  }

  const timeout = setTimeout(() => {
    toastTimeouts.delete(toastId)
    dispatch({
      type: "REMOVE_TOAST",
      toastId: toastId,
    })
  }, TOAST_REMOVE_DELAY)

  toastTimeouts.set(toastId, timeout)
}

export const reducer = (state: State, action: Action): State => {
  switch (action.type) {
    case "ADD_TOAST":
      return {
        ...state,
        toasts: [action.toast, ...state.toasts].slice(0, TOAST_LIMIT),
      }

    case "UPDATE_TOAST":
      return {
        ...state,
        toasts: state.toasts.map((t) => (t.id === action.toast.id ? { ...t, ...action.toast } : t)),
      }

    case "DISMISS_TOAST": {
      const { toastId } = action

      // ! Side effects ! - This could be extracted into a dismissToast() action,
      // but I'll keep it here for simplicity
      if (toastId) {
        addToRemoveQueue(toastId)
      } else {
        state.toasts.forEach((toast) => {
          addToRemoveQueue(toast.id)
        })
      }

      return {
        ...state,
        toasts: state.toasts.map((t) =>
          t.id === toastId || toastId === undefined
            ? {
                ...t,
                open: false,
              }
            : t,
        ),
      }
    }
    case "REMOVE_TOAST":
      if (action.toastId === undefined) {
        return {
          ...state,
          toasts: [],
        }
      }
      return {
        ...state,
        toasts: state.toasts.filter((t) => t.id !== action.toastId),
      }
  }
}

const listeners: Array<(state: State) => void> = []

let memoryState: State = { toasts: [] }

function dispatch(action: Action) {
  memoryState = reducer(memoryState, action)
  listeners.forEach((listener) => {
    listener(memoryState)
  })
}

type Toast = Omit<ToasterToast, "id">

function useToast() {
  const { addToast, removeToast, updateToast } = useToastInternal()

  return {
    success: (props: Omit<ToastProps, "id" | "type">) => addToast({ ...props, type: "success" }),

    error: (props: Omit<ToastProps, "id" | "type">) => addToast({ ...props, type: "error" }),

    warning: (props: Omit<ToastProps, "id" | "type">) => addToast({ ...props, type: "warning" }),

    info: (props: Omit<ToastProps, "id" | "type">) => addToast({ ...props, type: "info" }),

    custom: (props: Omit<ToastProps, "id">) => addToast(props),

    dismiss: (id: string) => removeToast(id),

    update: (id: string, props: Partial<ToastProps>) => updateToast(id, props),

    promise: async <T,>(
      promise: Promise<T>,
      options: {
        loading: Omit<ToastProps, "id">
        success: (data: T) => Omit<ToastProps, "id">
        error: (err: unknown) => Omit<ToastProps, "id">
      },
    ) => {
      const toastId = addToast({ ...options.loading, type: "info" })

      try {
        const data = await promise
        updateToast(toastId, { ...options.success(data), type: "success" })
        return data
      } catch (err) {
        updateToast(toastId, { ...options.error(err), type: "error" })
        throw err
      }
    },
  }
}

export { useToast }
