'use client'

import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { SessionProvider } from 'next-auth/react' // Import SessionProvider
import { ReactNode, useState } from 'react'
import { ToastProvider } from '@/providers/ToastProvider'
import { CategoryStoreProvider } from '@/providers/CategoryStoreProvider'
import { FileManagerProvider } from '@/contexts/FileManagerContext'
import { ConfirmationDialogProvider } from '@/components/ui/confirmation-dialog'

export function Providers({ children }: { children: ReactNode }) {
  const [queryClient] = useState(() => new QueryClient())

  return (
    // Wrap with SessionProvider
    <SessionProvider>
      <QueryClientProvider client={queryClient}>
        <CategoryStoreProvider>
          <ToastProvider>
            <FileManagerProvider>
              <ConfirmationDialogProvider>
                {children}
              </ConfirmationDialogProvider>
            </FileManagerProvider>
          </ToastProvider>
        </CategoryStoreProvider>
      </QueryClientProvider>
    </SessionProvider>
  )
}
