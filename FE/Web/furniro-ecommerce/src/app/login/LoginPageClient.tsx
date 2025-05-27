'use client';

import React from 'react';
import { LoginForm } from '@/components/auth/LoginForm';
import { useGuestGuard } from '@/hooks/useAuthGuard';
import { ClipLoader } from 'react-spinners';

export default function LoginPageClient() {
  const { isLoading, shouldRedirect } = useGuestGuard();

  // Show loading while checking auth status
  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <ClipLoader size={50} color="#3B82F6" />
          <p className="mt-4 text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  // Don't render if user should be redirected
  if (shouldRedirect) {
    return null;
  }

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
      <div className="sm:mx-auto sm:w-full sm:max-w-md">
        <div className="text-center">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">Furniro</h1>
          <p className="text-gray-600">Welcome back to your furniture store</p>
        </div>
      </div>

      <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
        <LoginForm />
      </div>

      {/* Additional Links */}
      <div className="mt-8 text-center">
        <div className="text-sm text-gray-600">
          <p className="mb-2">
            Need help?{' '}
            <a href="/contact" className="text-blue-600 hover:text-blue-500">
              Contact Support
            </a>
          </p>
          <p>
            <a href="/privacy" className="text-blue-600 hover:text-blue-500">
              Privacy Policy
            </a>
            {' • '}
            <a href="/terms" className="text-blue-600 hover:text-blue-500">
              Terms of Service
            </a>
          </p>
        </div>
      </div>
    </div>
  );
}
