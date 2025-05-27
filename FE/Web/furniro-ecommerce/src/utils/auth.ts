import type { JWTPayload, UserDTO } from '@/api/types';

/**
 * JWT Token utilities
 */
export class AuthUtils {
  /**
   * Decode JWT token payload
   */
  static decodeToken(token: string): JWTPayload | null {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }

  /**
   * Check if token is expired
   */
  static isTokenExpired(token: string): boolean {
    const payload = this.decodeToken(token);
    if (!payload) return true;

    const currentTime = Date.now() / 1000;
    return payload.exp < currentTime;
  }

  /**
   * Get token expiration time
   */
  static getTokenExpiration(token: string): Date | null {
    const payload = this.decodeToken(token);
    if (!payload) return null;

    return new Date(payload.exp * 1000);
  }

  /**
   * Check if token expires soon (within 5 minutes)
   */
  static isTokenExpiringSoon(token: string, minutesThreshold: number = 5): boolean {
    const payload = this.decodeToken(token);
    if (!payload) return true;

    const currentTime = Date.now() / 1000;
    const thresholdTime = currentTime + (minutesThreshold * 60);
    return payload.exp < thresholdTime;
  }

  /**
   * Extract user info from token
   */
  static getUserFromToken(token: string): Partial<UserDTO> | null {
    const payload = this.decodeToken(token);
    if (!payload) return null;

    return {
      id: parseInt(payload.sub),
      email: payload.email,
      username: payload.username,
      role: payload.role,
    };
  }

  /**
   * Validate email format
   */
  static isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  /**
   * Validate password strength
   */
  static validatePassword(password: string): {
    isValid: boolean;
    errors: string[];
    strength: 'weak' | 'medium' | 'strong';
  } {
    const errors: string[] = [];
    let score = 0;

    // Length check
    if (password.length < 6) {
      errors.push('Password must be at least 6 characters long');
    } else if (password.length >= 8) {
      score += 1;
    }

    // Uppercase check
    if (!/[A-Z]/.test(password)) {
      errors.push('Password must contain at least one uppercase letter');
    } else {
      score += 1;
    }

    // Lowercase check
    if (!/[a-z]/.test(password)) {
      errors.push('Password must contain at least one lowercase letter');
    } else {
      score += 1;
    }

    // Number check
    if (!/\d/.test(password)) {
      errors.push('Password must contain at least one number');
    } else {
      score += 1;
    }

    // Special character check
    if (!/[!@#$%^&*(),.?":{}|<>]/.test(password)) {
      errors.push('Password must contain at least one special character');
    } else {
      score += 1;
    }

    // Determine strength
    let strength: 'weak' | 'medium' | 'strong' = 'weak';
    if (score >= 4) strength = 'strong';
    else if (score >= 2) strength = 'medium';

    return {
      isValid: errors.length === 0,
      errors,
      strength,
    };
  }

  /**
   * Generate random password
   */
  static generateRandomPassword(length: number = 12): string {
    const charset = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*';
    let password = '';
    
    for (let i = 0; i < length; i++) {
      password += charset.charAt(Math.floor(Math.random() * charset.length));
    }
    
    return password;
  }

  /**
   * Hash password (client-side hashing for additional security)
   */
  static async hashPassword(password: string): Promise<string> {
    const encoder = new TextEncoder();
    const data = encoder.encode(password);
    const hashBuffer = await crypto.subtle.digest('SHA-256', data);
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    return hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
  }

  /**
   * Check user permissions
   */
  static hasPermission(user: UserDTO | null, permission: string): boolean {
    if (!user) return false;
    
    // Admin has all permissions
    if (user.role === 'admin') return true;
    
    // Add more permission logic here based on your requirements
    const userPermissions: Record<string, string[]> = {
      admin: ['*'], // All permissions
      customer: ['view_products', 'create_order', 'view_own_orders'],
      moderator: ['view_products', 'moderate_reviews'],
    };
    
    const permissions = userPermissions[user.role] || [];
    return permissions.includes('*') || permissions.includes(permission);
  }

  /**
   * Check if user has role
   */
  static hasRole(user: UserDTO | null, role: string): boolean {
    return user?.role === role;
  }

  /**
   * Check if user is admin
   */
  static isAdmin(user: UserDTO | null): boolean {
    return this.hasRole(user, 'admin');
  }

  /**
   * Check if user is customer
   */
  static isCustomer(user: UserDTO | null): boolean {
    return this.hasRole(user, 'customer');
  }

  /**
   * Format user display name
   */
  static formatUserDisplayName(user: UserDTO): string {
    return user.username || user.email || 'Unknown User';
  }

  /**
   * Get user initials for avatar
   */
  static getUserInitials(user: UserDTO): string {
    const name = this.formatUserDisplayName(user);
    const words = name.split(' ');
    
    if (words.length >= 2) {
      return (words[0][0] + words[1][0]).toUpperCase();
    }
    
    return name.substring(0, 2).toUpperCase();
  }
}

export default AuthUtils;
