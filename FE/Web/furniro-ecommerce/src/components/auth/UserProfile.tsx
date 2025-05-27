'use client';

import React, { useState } from 'react';
import { useAuth } from '@/context/AuthContext';
import { Button } from '@/components/ui/Button';
import { ClipLoader } from 'react-spinners';

interface UserProfileProps {
  showLogoutButton?: boolean;
  onLogout?: () => void;
}

export function UserProfile({ showLogoutButton = true, onLogout }: UserProfileProps) {
  const { user, logout, refreshUser, isLoading } = useAuth();
  const [isRefreshing, setIsRefreshing] = useState(false);

  if (!user) {
    return (
      <div className="text-center py-8">
        <p className="text-gray-600">No user data available</p>
      </div>
    );
  }

  const handleLogout = () => {
    logout();
    if (onLogout) {
      onLogout();
    }
  };

  const handleRefresh = async () => {
    try {
      setIsRefreshing(true);
      await refreshUser();
    } catch (error) {
      console.error('Error refreshing user data:', error);
    } finally {
      setIsRefreshing(false);
    }
  };

  const getRoleBadgeColor = (role: string) => {
    switch (role.toLowerCase()) {
      case 'admin':
        return 'bg-red-100 text-red-800';
      case 'moderator':
        return 'bg-yellow-100 text-yellow-800';
      case 'customer':
        return 'bg-green-100 text-green-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  return (
    <div className="bg-white shadow-lg rounded-lg p-6">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-bold text-gray-900">User Profile</h2>
        <button
          onClick={handleRefresh}
          disabled={isRefreshing}
          className="text-blue-600 hover:text-blue-700 disabled:opacity-50"
          title="Refresh user data"
        >
          {isRefreshing ? (
            <ClipLoader size={16} color="#3B82F6" />
          ) : (
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
          )}
        </button>
      </div>

      <div className="space-y-4">
        {/* User Avatar */}
        <div className="flex items-center space-x-4">
          <div className="w-16 h-16 bg-blue-500 rounded-full flex items-center justify-center">
            <span className="text-2xl font-bold text-white">
              {user.username.charAt(0).toUpperCase()}
            </span>
          </div>
          <div>
            <h3 className="text-xl font-semibold text-gray-900">{user.username}</h3>
            <p className="text-gray-600">{user.email}</p>
          </div>
        </div>

        {/* User Details */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="bg-gray-50 p-4 rounded-lg">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              User ID
            </label>
            <p className="text-gray-900">{user.id}</p>
          </div>

          <div className="bg-gray-50 p-4 rounded-lg">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Username
            </label>
            <p className="text-gray-900">{user.username}</p>
          </div>

          <div className="bg-gray-50 p-4 rounded-lg">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Email Address
            </label>
            <p className="text-gray-900">{user.email}</p>
          </div>

          <div className="bg-gray-50 p-4 rounded-lg">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Role
            </label>
            <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getRoleBadgeColor(user.role)}`}>
              {user.role.charAt(0).toUpperCase() + user.role.slice(1)}
            </span>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="flex flex-col sm:flex-row gap-3 pt-6 border-t border-gray-200">
          <Button
            variant="outline"
            className="flex-1"
            onClick={() => {
              // TODO: Implement edit profile functionality
              console.log('Edit profile clicked');
            }}
          >
            Edit Profile
          </Button>

          <Button
            variant="outline"
            className="flex-1"
            onClick={() => {
              // TODO: Implement change password functionality
              console.log('Change password clicked');
            }}
          >
            Change Password
          </Button>

          {showLogoutButton && (
            <Button
              variant="destructive"
              className="flex-1"
              onClick={handleLogout}
              disabled={isLoading}
            >
              {isLoading ? (
                <div className="flex items-center justify-center">
                  <ClipLoader size={16} color="white" />
                  <span className="ml-2">Logging out...</span>
                </div>
              ) : (
                'Logout'
              )}
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}

// Compact version for header/navbar
export function UserProfileCompact() {
  const { user, logout } = useAuth();

  if (!user) return null;

  return (
    <div className="flex items-center space-x-3">
      <div className="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center">
        <span className="text-sm font-bold text-white">
          {user.username.charAt(0).toUpperCase()}
        </span>
      </div>
      <div className="hidden md:block">
        <p className="text-sm font-medium text-gray-900">{user.username}</p>
        <p className="text-xs text-gray-600">{user.role}</p>
      </div>
    </div>
  );
}

export default UserProfile;
