'use client';

import React, { useEffect, useState } from 'react';
import type { ShippingAddressForm as ShippingAddressFormType } from '@/api/types';
import { ShippingService } from '@/services/shippingService';

interface ShippingAddressFormProps {
  data: ShippingAddressFormType;
  errors: Record<string, string>;
  onChange: (data: ShippingAddressFormType) => void;
}

export default function ShippingAddressForm({ data, errors, onChange }: ShippingAddressFormProps) {
  const [addressValidation, setAddressValidation] = useState<any>(null);
  const [isValidating, setIsValidating] = useState(false);

  const handleInputChange = (field: keyof ShippingAddressFormType, value: string) => {
    const updatedData = {
      ...data,
      [field]: value
    };
    onChange(updatedData);

    // Validate address when key fields change
    if (['address', 'city', 'state', 'postalCode', 'country'].includes(field)) {
      validateAddress(updatedData);
    }
  };

  const validateAddress = async (addressData: ShippingAddressFormType) => {
    if (!addressData.address || !addressData.city || !addressData.country) {
      return;
    }

    setIsValidating(true);
    try {
      const validation = ShippingService.validateAddress(addressData);
      setAddressValidation(validation);
    } catch (error) {
      console.error('Address validation failed:', error);
    } finally {
      setIsValidating(false);
    }
  };

  const countries = [
    { code: 'US', name: 'United States' },
    { code: 'CA', name: 'Canada' },
    { code: 'GB', name: 'United Kingdom' },
    { code: 'AU', name: 'Australia' },
    { code: 'DE', name: 'Germany' },
    { code: 'FR', name: 'France' },
    { code: 'JP', name: 'Japan' },
    { code: 'VN', name: 'Vietnam' },
  ];

  return (
    <div>
      <h3 className="text-xl font-semibold mb-6">Shipping Address</h3>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* First Name */}
        <div>
          <label htmlFor="firstName" className="block text-sm font-medium text-gray-700 mb-2">
            First Name *
          </label>
          <input
            type="text"
            id="firstName"
            value={data.firstName}
            onChange={(e) => handleInputChange('firstName', e.target.value)}
            className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent ${
              errors.firstName ? 'border-red-500' : 'border-gray-300'
            }`}
            placeholder="Enter your first name"
          />
          {errors.firstName && (
            <p className="mt-1 text-sm text-red-600">{errors.firstName}</p>
          )}
        </div>

        {/* Last Name */}
        <div>
          <label htmlFor="lastName" className="block text-sm font-medium text-gray-700 mb-2">
            Last Name *
          </label>
          <input
            type="text"
            id="lastName"
            value={data.lastName}
            onChange={(e) => handleInputChange('lastName', e.target.value)}
            className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent ${
              errors.lastName ? 'border-red-500' : 'border-gray-300'
            }`}
            placeholder="Enter your last name"
          />
          {errors.lastName && (
            <p className="mt-1 text-sm text-red-600">{errors.lastName}</p>
          )}
        </div>

        {/* Address */}
        <div className="md:col-span-2">
          <label htmlFor="address" className="block text-sm font-medium text-gray-700 mb-2">
            Street Address *
          </label>
          <input
            type="text"
            id="address"
            value={data.address}
            onChange={(e) => handleInputChange('address', e.target.value)}
            className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent ${
              errors.address ? 'border-red-500' : 'border-gray-300'
            }`}
            placeholder="Enter your street address"
          />
          {errors.address && (
            <p className="mt-1 text-sm text-red-600">{errors.address}</p>
          )}
        </div>

        {/* City */}
        <div>
          <label htmlFor="city" className="block text-sm font-medium text-gray-700 mb-2">
            City *
          </label>
          <input
            type="text"
            id="city"
            value={data.city}
            onChange={(e) => handleInputChange('city', e.target.value)}
            className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent ${
              errors.city ? 'border-red-500' : 'border-gray-300'
            }`}
            placeholder="Enter your city"
          />
          {errors.city && (
            <p className="mt-1 text-sm text-red-600">{errors.city}</p>
          )}
        </div>

        {/* State */}
        <div>
          <label htmlFor="state" className="block text-sm font-medium text-gray-700 mb-2">
            State/Province *
          </label>
          <input
            type="text"
            id="state"
            value={data.state}
            onChange={(e) => handleInputChange('state', e.target.value)}
            className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent ${
              errors.state ? 'border-red-500' : 'border-gray-300'
            }`}
            placeholder="Enter your state or province"
          />
          {errors.state && (
            <p className="mt-1 text-sm text-red-600">{errors.state}</p>
          )}
        </div>

        {/* Postal Code */}
        <div>
          <label htmlFor="postalCode" className="block text-sm font-medium text-gray-700 mb-2">
            Postal Code *
          </label>
          <input
            type="text"
            id="postalCode"
            value={data.postalCode}
            onChange={(e) => handleInputChange('postalCode', e.target.value)}
            className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent ${
              errors.postalCode ? 'border-red-500' : 'border-gray-300'
            }`}
            placeholder="Enter your postal code"
          />
          {errors.postalCode && (
            <p className="mt-1 text-sm text-red-600">{errors.postalCode}</p>
          )}
        </div>

        {/* Country */}
        <div>
          <label htmlFor="country" className="block text-sm font-medium text-gray-700 mb-2">
            Country *
          </label>
          <select
            id="country"
            value={data.country}
            onChange={(e) => handleInputChange('country', e.target.value)}
            className={`w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent ${
              errors.country ? 'border-red-500' : 'border-gray-300'
            }`}
          >
            <option value="">Select a country</option>
            {countries.map((country) => (
              <option key={country.code} value={country.name}>
                {country.name}
              </option>
            ))}
          </select>
          {errors.country && (
            <p className="mt-1 text-sm text-red-600">{errors.country}</p>
          )}
        </div>

        {/* Phone */}
        <div className="md:col-span-2">
          <label htmlFor="phone" className="block text-sm font-medium text-gray-700 mb-2">
            Phone Number
          </label>
          <input
            type="tel"
            id="phone"
            value={data.phone || ''}
            onChange={(e) => handleInputChange('phone', e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent"
            placeholder="Enter your phone number (optional)"
          />
        </div>
      </div>

      {/* Address validation feedback */}
      {addressValidation && (
        <div className="mt-6">
          {addressValidation.isValid ? (
            <div className="p-4 bg-green-50 border border-green-200 rounded-lg">
              <div className="flex items-start">
                <div className="flex-shrink-0">
                  <svg className="h-5 w-5 text-green-400" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                  </svg>
                </div>
                <div className="ml-3">
                  <h4 className="text-sm font-medium text-green-800">Address Verified</h4>
                  <p className="mt-1 text-sm text-green-700">
                    Your address looks good! Confidence: {addressValidation.confidence}%
                  </p>
                  {ShippingService.isShippingAvailable(data.country) && (
                    <p className="mt-1 text-sm text-green-700">
                      ✓ Shipping available to {data.country}
                    </p>
                  )}
                </div>
              </div>
            </div>
          ) : (
            <div className="p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
              <div className="flex items-start">
                <div className="flex-shrink-0">
                  <svg className="h-5 w-5 text-yellow-400" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                  </svg>
                </div>
                <div className="ml-3">
                  <h4 className="text-sm font-medium text-yellow-800">Address Needs Review</h4>
                  <p className="mt-1 text-sm text-yellow-700">
                    Please check your address for accuracy. Confidence: {addressValidation.confidence}%
                  </p>
                  {!ShippingService.isShippingAvailable(data.country) && (
                    <p className="mt-1 text-sm text-red-700">
                      ⚠️ Shipping not available to {data.country}
                    </p>
                  )}
                </div>
              </div>
            </div>
          )}
        </div>
      )}

      {isValidating && (
        <div className="mt-6 p-4 bg-gray-50 border border-gray-200 rounded-lg">
          <div className="flex items-center">
            <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-primary mr-3"></div>
            <span className="text-sm text-gray-600">Validating address...</span>
          </div>
        </div>
      )}

      {/* Save address option */}
      <div className="mt-6">
        <label className="flex items-center">
          <input
            type="checkbox"
            className="h-4 w-4 text-primary focus:ring-primary border-gray-300 rounded"
          />
          <span className="ml-2 text-sm text-gray-700">
            Save this address for future orders
          </span>
        </label>
      </div>
    </div>
  );
}
