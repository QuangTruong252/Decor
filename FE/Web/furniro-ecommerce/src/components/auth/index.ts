// Authentication components exports
export { LoginForm } from './LoginForm';
export { RegisterForm } from './RegisterForm';
export { ProtectedRoute, withAuth, useRequireAuth } from './ProtectedRoute';
export { UserProfile, UserProfileCompact } from './UserProfile';

// Re-export auth context and hook
export { useAuth, AuthProvider } from '@/context/AuthContext';

// Types
export type {
  AuthContextType,
  UserDTO,
  LoginDTO,
  RegisterDTO,
  AuthResponseDTO,
  LoginFormData,
  RegisterFormData,
} from '@/api/types/auth';
