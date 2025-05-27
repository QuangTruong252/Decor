import React from 'react';
import { Metadata } from 'next';
import RegisterPageClient from './RegisterPageClient';

export const metadata: Metadata = {
  title: 'Create Account | Furniro',
  description: 'Join Furniro today and discover amazing furniture for your home.',
};

export default function RegisterPage() {
  return <RegisterPageClient />;
}
