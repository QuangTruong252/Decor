// Authentication related types

export interface UserDTO {
  id: number;
  username: string;
  email: string;
  role: string;
}

export interface LoginDTO {
  email: string;
  password: string;
}

export interface RegisterDTO {
  username: string;
  email: string;
  password: string;
  confirmPassword?: string;
}

export interface AuthResponseDTO {
  token: string;
  user: UserDTO;
}

export interface AuthState {
  user: UserDTO | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

export interface LoginFormData {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterFormData {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  acceptTerms: boolean;
}

export interface ChangePasswordData {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface ForgotPasswordData {
  email: string;
}

export interface ResetPasswordData {
  token: string;
  password: string;
  confirmPassword: string;
}

export interface AuthContextType {
  user: UserDTO | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  login: (credentials: LoginDTO) => Promise<void>;
  register: (userData: RegisterDTO) => Promise<void>;
  logout: () => void;
  clearError: () => void;
  refreshUser: () => Promise<void>;
}

// JWT Token payload
export interface JWTPayload {
  sub: string; // user id
  email: string;
  username: string;
  role: string;
  iat: number; // issued at
  exp: number; // expires at
}

// Permission and role types
export type UserRole = 'admin' | 'customer' | 'moderator';

export interface Permission {
  id: number;
  name: string;
  description: string;
}

export interface Role {
  id: number;
  name: UserRole;
  permissions: Permission[];
}

// Auth hooks return types
export interface UseAuthReturn extends AuthContextType {}

export interface UseLoginReturn {
  login: (credentials: LoginDTO) => Promise<void>;
  isLoading: boolean;
  error: string | null;
  clearError: () => void;
}

export interface UseRegisterReturn {
  register: (userData: RegisterDTO) => Promise<void>;
  isLoading: boolean;
  error: string | null;
  clearError: () => void;
}
