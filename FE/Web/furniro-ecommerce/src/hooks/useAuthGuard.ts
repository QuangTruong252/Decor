'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';

interface UseAuthGuardOptions {
  requiredRole?: string;
  redirectTo?: string;
  requireAuth?: boolean;
}

/**
 * Hook to guard routes based on authentication and role requirements
 */
export function useAuthGuard(options: UseAuthGuardOptions = {}) {
  const {
    requiredRole,
    redirectTo = '/login',
    requireAuth = true,
  } = options;

  const { user, isAuthenticated, isLoading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    // Don't redirect while loading
    if (isLoading) return;

    // Check authentication requirement
    if (requireAuth && !isAuthenticated) {
      router.push(redirectTo);
      return;
    }

    // Check role requirement
    if (requiredRole && (!user || user.role !== requiredRole)) {
      router.push('/unauthorized');
      return;
    }
  }, [isAuthenticated, isLoading, user, requiredRole, router, redirectTo, requireAuth]);

  return {
    user,
    isAuthenticated,
    isLoading,
    hasRequiredRole: !requiredRole || (user?.role === requiredRole),
    canAccess: isAuthenticated && (!requiredRole || user?.role === requiredRole),
  };
}

/**
 * Hook to redirect authenticated users away from auth pages
 */
export function useGuestGuard(redirectTo = '/') {
  const { isAuthenticated, isLoading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      router.push(redirectTo);
    }
  }, [isAuthenticated, isLoading, router, redirectTo]);

  return {
    isAuthenticated,
    isLoading,
    shouldRedirect: !isLoading && isAuthenticated,
  };
}

/**
 * Hook to check if user has specific role
 */
export function useRole(requiredRole: string) {
  const { user, isAuthenticated } = useAuth();

  return {
    hasRole: isAuthenticated && user?.role === requiredRole,
    userRole: user?.role,
    isAuthenticated,
  };
}

/**
 * Hook to check if user has any of the specified roles
 */
export function useRoles(requiredRoles: string[]) {
  const { user, isAuthenticated } = useAuth();

  return {
    hasAnyRole: isAuthenticated && user?.role && requiredRoles.includes(user.role),
    userRole: user?.role,
    isAuthenticated,
  };
}
