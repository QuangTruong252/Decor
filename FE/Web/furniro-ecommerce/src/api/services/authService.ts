import { api } from '../client';
import { AUTH } from '../endpoints';
import type {
  LoginDTO,
  RegisterDTO,
  AuthResponseDTO,
  UserDTO,
} from '../types';

export class AuthService {
  /**
   * User login
   */
  static async login(credentials: LoginDTO): Promise<AuthResponseDTO> {
    const response = await api.post<AuthResponseDTO>(AUTH.LOGIN, credentials);
    return response.data;
  }

  /**
   * User registration
   */
  static async register(userData: RegisterDTO): Promise<AuthResponseDTO> {
    const response = await api.post<AuthResponseDTO>(AUTH.REGISTER, userData);
    return response.data;
  }

  /**
   * Get current user info
   */
  static async getCurrentUser(): Promise<UserDTO> {
    const response = await api.get<UserDTO>(AUTH.USER);
    return response.data;
  }

  /**
   * Check user claims/permissions
   */
  static async checkClaims(): Promise<any> {
    const response = await api.get(AUTH.CHECK_CLAIMS);
    return response.data;
  }

  /**
   * Make user admin (admin only)
   */
  static async makeAdmin(userId: number): Promise<void> {
    await api.post(AUTH.MAKE_ADMIN, { userId });
  }

  /**
   * Logout user (client-side token cleanup)
   */
  static logout(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('auth_token');
      localStorage.removeItem('user');
      localStorage.removeItem('refresh_token');
    }
  }

  /**
   * Check if user is authenticated
   */
  static isAuthenticated(): boolean {
    if (typeof window === 'undefined') return false;
    const token = localStorage.getItem('auth_token');
    return !!token && !this.isTokenExpired(token);
  }

  /**
   * Get stored auth token
   */
  static getToken(): string | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem('auth_token');
  }

  /**
   * Store auth token
   */
  static setToken(token: string): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem('auth_token', token);
    }
  }

  /**
   * Get stored user data
   */
  static getStoredUser(): UserDTO | null {
    if (typeof window === 'undefined') return null;
    const userStr = localStorage.getItem('user');
    if (!userStr) return null;
    
    try {
      return JSON.parse(userStr);
    } catch {
      return null;
    }
  }

  /**
   * Store user data
   */
  static setStoredUser(user: UserDTO): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem('user', JSON.stringify(user));
    }
  }

  /**
   * Check if token is expired
   */
  static isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Date.now() / 1000;
      return payload.exp < currentTime;
    } catch {
      return true;
    }
  }

  /**
   * Refresh user data
   */
  static async refreshUser(): Promise<UserDTO> {
    const user = await this.getCurrentUser();
    this.setStoredUser(user);
    return user;
  }

  /**
   * Clear all auth data
   */
  static clearAuthData(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('auth_token');
      localStorage.removeItem('user');
      localStorage.removeItem('refresh_token');
    }
  }
}

export default AuthService;
