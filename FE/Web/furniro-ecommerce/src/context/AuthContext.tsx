'use client';

import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { AuthService } from '@/api/services/authService';
import type {
  AuthContextType,
  UserDTO,
  LoginDTO,
  RegisterDTO,
} from '@/api/types/auth';
import toast from 'react-hot-toast';

// Create the context
const AuthContext = createContext<AuthContextType | undefined>(undefined);

// AuthProvider props
interface AuthProviderProps {
  children: ReactNode;
}

// AuthProvider component
export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<UserDTO | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Computed state
  const isAuthenticated = !!user && !!token;

  // Initialize auth state from localStorage
  useEffect(() => {
    const initializeAuth = async () => {
      try {
        const storedToken = AuthService.getToken();
        const storedUser = AuthService.getStoredUser();

        if (storedToken && storedUser && !AuthService.isTokenExpired(storedToken)) {
          setToken(storedToken);
          setUser(storedUser);
          
          // Optionally refresh user data from server
          try {
            const refreshedUser = await AuthService.refreshUser();
            setUser(refreshedUser);
          } catch (refreshError) {
            // If refresh fails, keep stored user data
            console.warn('Failed to refresh user data:', refreshError);
          }
        } else {
          // Clear invalid/expired auth data
          AuthService.clearAuthData();
        }
      } catch (error) {
        console.error('Error initializing auth:', error);
        AuthService.clearAuthData();
      } finally {
        setIsLoading(false);
      }
    };

    initializeAuth();
  }, []);

  // Login function
  const login = async (credentials: LoginDTO): Promise<void> => {
    try {
      setIsLoading(true);
      setError(null);

      const response = await AuthService.login(credentials);
      
      // Store auth data
      AuthService.setToken(response.token);
      AuthService.setStoredUser(response.user);
      
      // Update state
      setToken(response.token);
      setUser(response.user);
      
      toast.success(`Welcome back, ${response.user.username}!`);
    } catch (error: any) {
      const errorMessage = error.message || 'Login failed. Please try again.';
      setError(errorMessage);
      toast.error(errorMessage);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  // Register function
  const register = async (userData: RegisterDTO): Promise<void> => {
    try {
      setIsLoading(true);
      setError(null);

      const response = await AuthService.register(userData);
      
      // Store auth data
      AuthService.setToken(response.token);
      AuthService.setStoredUser(response.user);
      
      // Update state
      setToken(response.token);
      setUser(response.user);
      
      toast.success(`Welcome to Furniro, ${response.user.username}!`);
    } catch (error: any) {
      const errorMessage = error.message || 'Registration failed. Please try again.';
      setError(errorMessage);
      toast.error(errorMessage);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  // Logout function
  const logout = (): void => {
    try {
      // Clear auth data
      AuthService.clearAuthData();
      
      // Update state
      setUser(null);
      setToken(null);
      setError(null);
      
      toast.success('Logged out successfully');
    } catch (error) {
      console.error('Error during logout:', error);
      toast.error('Error during logout');
    }
  };

  // Clear error function
  const clearError = (): void => {
    setError(null);
  };

  // Refresh user data
  const refreshUser = async (): Promise<void> => {
    try {
      if (!token) return;
      
      const refreshedUser = await AuthService.refreshUser();
      setUser(refreshedUser);
    } catch (error: any) {
      console.error('Error refreshing user:', error);
      // If refresh fails and it's an auth error, logout
      if (error.status === 401) {
        logout();
      }
    }
  };

  // Context value
  const contextValue: AuthContextType = {
    user,
    token,
    isAuthenticated,
    isLoading,
    error,
    login,
    register,
    logout,
    clearError,
    refreshUser,
  };

  return (
    <AuthContext.Provider value={contextValue}>
      {children}
    </AuthContext.Provider>
  );
}

// Custom hook to use auth context
export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  
  return context;
}

// Export context for advanced usage
export { AuthContext };
export default AuthProvider;
