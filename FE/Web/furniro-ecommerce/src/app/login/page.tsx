import React from 'react';
import LoginPageClient from './LoginPageClient';
import { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Login | Furniro',
  description: 'Sign in to your Furniro account to access your orders, wishlist, and more.',
};

export default function LoginPage() {
  return <LoginPageClient />;
}
