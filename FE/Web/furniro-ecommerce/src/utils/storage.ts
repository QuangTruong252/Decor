/**
 * Storage utilities for localStorage and sessionStorage
 */
export class StorageUtils {
  /**
   * Check if storage is available
   */
  static isStorageAvailable(type: 'localStorage' | 'sessionStorage' = 'localStorage'): boolean {
    if (typeof window === 'undefined') return false;
    
    try {
      const storage = window[type];
      const test = '__storage_test__';
      storage.setItem(test, test);
      storage.removeItem(test);
      return true;
    } catch {
      return false;
    }
  }

  /**
   * Set item in localStorage with JSON serialization
   */
  static setLocal<T>(key: string, value: T): boolean {
    if (!this.isStorageAvailable('localStorage')) return false;
    
    try {
      const serializedValue = JSON.stringify(value);
      localStorage.setItem(key, serializedValue);
      return true;
    } catch (error) {
      console.error('Error setting localStorage item:', error);
      return false;
    }
  }

  /**
   * Get item from localStorage with JSON deserialization
   */
  static getLocal<T>(key: string): T | null {
    if (!this.isStorageAvailable('localStorage')) return null;
    
    try {
      const item = localStorage.getItem(key);
      return item ? JSON.parse(item) : null;
    } catch (error) {
      console.error('Error getting localStorage item:', error);
      return null;
    }
  }

  /**
   * Remove item from localStorage
   */
  static removeLocal(key: string): boolean {
    if (!this.isStorageAvailable('localStorage')) return false;
    
    try {
      localStorage.removeItem(key);
      return true;
    } catch (error) {
      console.error('Error removing localStorage item:', error);
      return false;
    }
  }

  /**
   * Clear all localStorage
   */
  static clearLocal(): boolean {
    if (!this.isStorageAvailable('localStorage')) return false;
    
    try {
      localStorage.clear();
      return true;
    } catch (error) {
      console.error('Error clearing localStorage:', error);
      return false;
    }
  }

  /**
   * Set item in sessionStorage with JSON serialization
   */
  static setSession<T>(key: string, value: T): boolean {
    if (!this.isStorageAvailable('sessionStorage')) return false;
    
    try {
      const serializedValue = JSON.stringify(value);
      sessionStorage.setItem(key, serializedValue);
      return true;
    } catch (error) {
      console.error('Error setting sessionStorage item:', error);
      return false;
    }
  }

  /**
   * Get item from sessionStorage with JSON deserialization
   */
  static getSession<T>(key: string): T | null {
    if (!this.isStorageAvailable('sessionStorage')) return null;
    
    try {
      const item = sessionStorage.getItem(key);
      return item ? JSON.parse(item) : null;
    } catch (error) {
      console.error('Error getting sessionStorage item:', error);
      return null;
    }
  }

  /**
   * Remove item from sessionStorage
   */
  static removeSession(key: string): boolean {
    if (!this.isStorageAvailable('sessionStorage')) return false;
    
    try {
      sessionStorage.removeItem(key);
      return true;
    } catch (error) {
      console.error('Error removing sessionStorage item:', error);
      return false;
    }
  }

  /**
   * Clear all sessionStorage
   */
  static clearSession(): boolean {
    if (!this.isStorageAvailable('sessionStorage')) return false;
    
    try {
      sessionStorage.clear();
      return true;
    } catch (error) {
      console.error('Error clearing sessionStorage:', error);
      return false;
    }
  }

  /**
   * Set item with expiration
   */
  static setWithExpiry<T>(key: string, value: T, ttl: number): boolean {
    const now = new Date();
    const item = {
      value,
      expiry: now.getTime() + ttl,
    };
    return this.setLocal(key, item);
  }

  /**
   * Get item with expiration check
   */
  static getWithExpiry<T>(key: string): T | null {
    const item = this.getLocal<{ value: T; expiry: number }>(key);
    
    if (!item) return null;
    
    const now = new Date();
    if (now.getTime() > item.expiry) {
      this.removeLocal(key);
      return null;
    }
    
    return item.value;
  }

  /**
   * Get storage size in bytes
   */
  static getStorageSize(type: 'localStorage' | 'sessionStorage' = 'localStorage'): number {
    if (!this.isStorageAvailable(type)) return 0;
    
    let total = 0;
    const storage = window[type];
    
    for (const key in storage) {
      if (storage.hasOwnProperty(key)) {
        total += storage[key].length + key.length;
      }
    }
    
    return total;
  }

  /**
   * Get storage size in human readable format
   */
  static getStorageSizeFormatted(type: 'localStorage' | 'sessionStorage' = 'localStorage'): string {
    const bytes = this.getStorageSize(type);
    
    if (bytes === 0) return '0 Bytes';
    
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  /**
   * Check if storage is nearly full (>90% of 5MB limit)
   */
  static isStorageNearlyFull(type: 'localStorage' | 'sessionStorage' = 'localStorage'): boolean {
    const size = this.getStorageSize(type);
    const limit = 5 * 1024 * 1024; // 5MB typical limit
    return size > (limit * 0.9);
  }

  /**
   * Get all keys from storage
   */
  static getAllKeys(type: 'localStorage' | 'sessionStorage' = 'localStorage'): string[] {
    if (!this.isStorageAvailable(type)) return [];
    
    const storage = window[type];
    const keys: string[] = [];
    
    for (let i = 0; i < storage.length; i++) {
      const key = storage.key(i);
      if (key) keys.push(key);
    }
    
    return keys;
  }

  /**
   * Export storage data
   */
  static exportStorage(type: 'localStorage' | 'sessionStorage' = 'localStorage'): Record<string, any> {
    if (!this.isStorageAvailable(type)) return {};
    
    const storage = window[type];
    const data: Record<string, any> = {};
    
    for (let i = 0; i < storage.length; i++) {
      const key = storage.key(i);
      if (key) {
        try {
          data[key] = JSON.parse(storage.getItem(key) || '');
        } catch {
          data[key] = storage.getItem(key);
        }
      }
    }
    
    return data;
  }

  /**
   * Import storage data
   */
  static importStorage(data: Record<string, any>, type: 'localStorage' | 'sessionStorage' = 'localStorage'): boolean {
    if (!this.isStorageAvailable(type)) return false;
    
    try {
      const storage = window[type];
      
      Object.entries(data).forEach(([key, value]) => {
        const serializedValue = typeof value === 'string' ? value : JSON.stringify(value);
        storage.setItem(key, serializedValue);
      });
      
      return true;
    } catch (error) {
      console.error('Error importing storage data:', error);
      return false;
    }
  }
}

export default StorageUtils;
