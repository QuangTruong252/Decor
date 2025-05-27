import type { ShippingAddressForm } from '@/api/types';

export interface ShippingMethod {
  id: string;
  name: string;
  description: string;
  estimatedDays: number;
  cost: number;
  isAvailable: boolean;
  icon: string;
}

export interface ShippingZone {
  id: string;
  name: string;
  countries: string[];
  states?: string[];
  cities?: string[];
  baseCost: number;
  costPerKg: number;
  freeShippingThreshold: number;
}

export interface ShippingCalculation {
  subtotal: number;
  shippingCost: number;
  tax: number;
  total: number;
  freeShippingEligible: boolean;
  freeShippingThreshold: number;
  amountToFreeShipping: number;
  estimatedDelivery: Date;
}

export interface AddressValidationResult {
  isValid: boolean;
  errors: Record<string, string>;
  suggestions?: ShippingAddressForm[];
  confidence: number;
}

export class ShippingService {
  // Define shipping zones
  private static shippingZones: ShippingZone[] = [
    {
      id: 'domestic',
      name: 'Domestic',
      countries: ['United States'],
      baseCost: 5.99,
      costPerKg: 2.50,
      freeShippingThreshold: 75,
    },
    {
      id: 'canada',
      name: 'Canada',
      countries: ['Canada'],
      baseCost: 12.99,
      costPerKg: 4.00,
      freeShippingThreshold: 100,
    },
    {
      id: 'international',
      name: 'International',
      countries: ['United Kingdom', 'Australia', 'Germany', 'France', 'Japan'],
      baseCost: 19.99,
      costPerKg: 6.50,
      freeShippingThreshold: 150,
    },
  ];

  // Define shipping methods
  private static shippingMethods: ShippingMethod[] = [
    {
      id: 'standard',
      name: 'Standard Shipping',
      description: 'Delivered within 5-7 business days',
      estimatedDays: 7,
      cost: 0, // Will be calculated based on zone
      isAvailable: true,
      icon: '📦',
    },
    {
      id: 'express',
      name: 'Express Shipping',
      description: 'Delivered within 2-3 business days',
      estimatedDays: 3,
      cost: 0, // Will be calculated (standard + 50%)
      isAvailable: true,
      icon: '⚡',
    },
    {
      id: 'overnight',
      name: 'Overnight Shipping',
      description: 'Delivered next business day',
      estimatedDays: 1,
      cost: 0, // Will be calculated (standard + 200%)
      isAvailable: true,
      icon: '🚀',
    },
  ];

  /**
   * Validate shipping address
   */
  static validateAddress(address: ShippingAddressForm): AddressValidationResult {
    const errors: Record<string, string> = {};
    let confidence = 100;

    // Basic validation
    if (!address.firstName?.trim()) {
      errors.firstName = 'First name is required';
      confidence -= 10;
    }

    if (!address.lastName?.trim()) {
      errors.lastName = 'Last name is required';
      confidence -= 10;
    }

    if (!address.address?.trim()) {
      errors.address = 'Street address is required';
      confidence -= 20;
    }

    if (!address.city?.trim()) {
      errors.city = 'City is required';
      confidence -= 15;
    }

    if (!address.state?.trim()) {
      errors.state = 'State/Province is required';
      confidence -= 15;
    }

    if (!address.postalCode?.trim()) {
      errors.postalCode = 'Postal code is required';
      confidence -= 15;
    } else if (!this.isValidPostalCode(address.postalCode, address.country)) {
      errors.postalCode = 'Invalid postal code format';
      confidence -= 10;
    }

    if (!address.country?.trim()) {
      errors.country = 'Country is required';
      confidence -= 20;
    }

    // Phone validation (optional but recommended)
    if (address.phone && !this.isValidPhoneNumber(address.phone)) {
      errors.phone = 'Invalid phone number format';
      confidence -= 5;
    }

    return {
      isValid: Object.keys(errors).length === 0,
      errors,
      confidence: Math.max(0, confidence),
    };
  }

  /**
   * Get shipping zone for address
   */
  static getShippingZone(address: ShippingAddressForm): ShippingZone | null {
    return this.shippingZones.find(zone => 
      zone.countries.includes(address.country)
    ) || null;
  }

  /**
   * Calculate shipping cost
   */
  static calculateShipping(
    address: ShippingAddressForm,
    subtotal: number,
    weight: number = 1, // kg
    methodId: string = 'standard'
  ): ShippingCalculation {
    const zone = this.getShippingZone(address);
    
    if (!zone) {
      throw new Error('Shipping not available to this location');
    }

    // Calculate base shipping cost
    let shippingCost = zone.baseCost + (weight * zone.costPerKg);

    // Apply method multiplier
    if (methodId === 'express') {
      shippingCost *= 1.5;
    } else if (methodId === 'overnight') {
      shippingCost *= 3;
    }

    // Check for free shipping
    const freeShippingEligible = subtotal >= zone.freeShippingThreshold;
    if (freeShippingEligible) {
      shippingCost = 0;
    }

    // Calculate tax (simplified - 10% for domestic, 0% for international)
    const taxRate = zone.id === 'domestic' ? 0.1 : 0;
    const tax = subtotal * taxRate;

    // Calculate total
    const total = subtotal + shippingCost + tax;

    // Calculate estimated delivery
    const method = this.shippingMethods.find(m => m.id === methodId);
    const estimatedDays = method?.estimatedDays || 7;
    const estimatedDelivery = new Date();
    estimatedDelivery.setDate(estimatedDelivery.getDate() + estimatedDays);

    return {
      subtotal,
      shippingCost: Math.round(shippingCost * 100) / 100,
      tax: Math.round(tax * 100) / 100,
      total: Math.round(total * 100) / 100,
      freeShippingEligible,
      freeShippingThreshold: zone.freeShippingThreshold,
      amountToFreeShipping: Math.max(0, zone.freeShippingThreshold - subtotal),
      estimatedDelivery,
    };
  }

  /**
   * Get available shipping methods for address
   */
  static getAvailableShippingMethods(
    address: ShippingAddressForm,
    subtotal: number,
    weight: number = 1
  ): ShippingMethod[] {
    const zone = this.getShippingZone(address);
    
    if (!zone) {
      return [];
    }

    return this.shippingMethods
      .filter(method => method.isAvailable)
      .map(method => {
        const calculation = this.calculateShipping(address, subtotal, weight, method.id);
        
        return {
          ...method,
          cost: calculation.shippingCost,
        };
      });
  }

  /**
   * Validate postal code format
   */
  private static isValidPostalCode(postalCode: string, country: string): boolean {
    const patterns: Record<string, RegExp> = {
      'United States': /^\d{5}(-\d{4})?$/,
      'Canada': /^[A-Za-z]\d[A-Za-z] ?\d[A-Za-z]\d$/,
      'United Kingdom': /^[A-Z]{1,2}\d[A-Z\d]? ?\d[A-Z]{2}$/i,
      'Australia': /^\d{4}$/,
      'Germany': /^\d{5}$/,
      'France': /^\d{5}$/,
      'Japan': /^\d{3}-\d{4}$/,
    };

    const pattern = patterns[country];
    return pattern ? pattern.test(postalCode) : true; // Default to valid for unknown countries
  }

  /**
   * Validate phone number format
   */
  private static isValidPhoneNumber(phone: string): boolean {
    // Simple phone validation - accepts various formats
    const phoneRegex = /^[\+]?[1-9][\d]{0,15}$/;
    const cleanPhone = phone.replace(/[\s\-\(\)\.]/g, '');
    return phoneRegex.test(cleanPhone) && cleanPhone.length >= 10;
  }

  /**
   * Format address for display
   */
  static formatAddress(address: ShippingAddressForm): string {
    const parts = [
      `${address.firstName} ${address.lastName}`,
      address.address,
      `${address.city}, ${address.state} ${address.postalCode}`,
      address.country,
    ].filter(Boolean);

    return parts.join('\n');
  }

  /**
   * Get shipping zones
   */
  static getShippingZones(): ShippingZone[] {
    return this.shippingZones;
  }

  /**
   * Check if shipping is available to country
   */
  static isShippingAvailable(country: string): boolean {
    return this.shippingZones.some(zone => zone.countries.includes(country));
  }

  /**
   * Get estimated delivery date
   */
  static getEstimatedDeliveryDate(
    methodId: string = 'standard',
    orderDate: Date = new Date()
  ): Date {
    const method = this.shippingMethods.find(m => m.id === methodId);
    const estimatedDays = method?.estimatedDays || 7;
    
    const deliveryDate = new Date(orderDate);
    deliveryDate.setDate(deliveryDate.getDate() + estimatedDays);
    
    return deliveryDate;
  }

  /**
   * Generate tracking number
   */
  static generateTrackingNumber(): string {
    const prefix = 'TRK';
    const timestamp = Date.now().toString(36).toUpperCase();
    const random = Math.random().toString(36).substr(2, 6).toUpperCase();
    
    return `${prefix}${timestamp}${random}`;
  }

  /**
   * Get shipping cost breakdown
   */
  static getShippingBreakdown(
    address: ShippingAddressForm,
    subtotal: number,
    weight: number = 1,
    methodId: string = 'standard'
  ): {
    baseCost: number;
    weightCost: number;
    methodMultiplier: number;
    totalBeforeDiscount: number;
    discount: number;
    finalCost: number;
  } {
    const zone = this.getShippingZone(address);
    
    if (!zone) {
      throw new Error('Shipping not available to this location');
    }

    const baseCost = zone.baseCost;
    const weightCost = weight * zone.costPerKg;
    
    let methodMultiplier = 1;
    if (methodId === 'express') methodMultiplier = 1.5;
    if (methodId === 'overnight') methodMultiplier = 3;
    
    const totalBeforeDiscount = (baseCost + weightCost) * methodMultiplier;
    const freeShippingEligible = subtotal >= zone.freeShippingThreshold;
    const discount = freeShippingEligible ? totalBeforeDiscount : 0;
    const finalCost = totalBeforeDiscount - discount;

    return {
      baseCost,
      weightCost,
      methodMultiplier,
      totalBeforeDiscount: Math.round(totalBeforeDiscount * 100) / 100,
      discount: Math.round(discount * 100) / 100,
      finalCost: Math.round(finalCost * 100) / 100,
    };
  }

  /**
   * Get shipping method by ID
   */
  static getShippingMethod(methodId: string): ShippingMethod | null {
    return this.shippingMethods.find(method => method.id === methodId) || null;
  }

  /**
   * Calculate delivery time range
   */
  static getDeliveryTimeRange(methodId: string): { min: number; max: number } {
    const method = this.getShippingMethod(methodId);
    
    if (!method) {
      return { min: 5, max: 7 };
    }

    switch (methodId) {
      case 'overnight':
        return { min: 1, max: 1 };
      case 'express':
        return { min: 2, max: 3 };
      case 'standard':
      default:
        return { min: 5, max: 7 };
    }
  }
}

export default ShippingService;
