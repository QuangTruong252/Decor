'use client';

import React, { useState, useEffect } from 'react';
import { NotificationService } from '@/services/notificationService';
import NotificationCenter from './NotificationCenter';

interface NotificationBellProps {
  userId: number;
  className?: string;
}

export default function NotificationBell({ userId, className = '' }: NotificationBellProps) {
  const [unreadCount, setUnreadCount] = useState(0);
  const [isOpen, setIsOpen] = useState(false);
  const [isAnimating, setIsAnimating] = useState(false);

  useEffect(() => {
    // Load initial unread count
    const count = NotificationService.getUnreadCount(userId);
    setUnreadCount(count);

    // Listen for new notifications
    const handleNewNotification = (event: CustomEvent) => {
      const notification = event.detail;
      if (notification.userId === userId) {
        setUnreadCount(prev => prev + 1);
        
        // Animate bell
        setIsAnimating(true);
        setTimeout(() => setIsAnimating(false), 1000);
      }
    };

    window.addEventListener('newNotification', handleNewNotification as EventListener);
    
    return () => {
      window.removeEventListener('newNotification', handleNewNotification as EventListener);
    };
  }, [userId]);

  const handleClick = () => {
    setIsOpen(true);
    
    // Update unread count when opening
    const currentCount = NotificationService.getUnreadCount(userId);
    setUnreadCount(currentCount);
  };

  const handleClose = () => {
    setIsOpen(false);
    
    // Refresh unread count when closing
    const currentCount = NotificationService.getUnreadCount(userId);
    setUnreadCount(currentCount);
  };

  return (
    <>
      <button
        onClick={handleClick}
        className={`relative p-2 text-gray-600 hover:text-gray-900 transition-colors ${className} ${
          isAnimating ? 'animate-bounce' : ''
        }`}
        aria-label="Notifications"
      >
        {/* Bell icon */}
        <svg 
          className="w-6 h-6" 
          fill="none" 
          stroke="currentColor" 
          viewBox="0 0 24 24"
        >
          <path 
            strokeLinecap="round" 
            strokeLinejoin="round" 
            strokeWidth={2} 
            d="M15 17h5l-5 5v-5zM9 7H4l5-5v5zm6 10V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2h6a2 2 0 002-2z" 
          />
        </svg>

        {/* Unread count badge */}
        {unreadCount > 0 && (
          <span className="absolute -top-1 -right-1 inline-flex items-center justify-center px-2 py-1 text-xs font-bold leading-none text-white transform translate-x-1/2 -translate-y-1/2 bg-red-600 rounded-full min-w-[1.25rem] h-5">
            {unreadCount > 99 ? '99+' : unreadCount}
          </span>
        )}

        {/* Pulse animation for new notifications */}
        {isAnimating && (
          <span className="absolute -top-1 -right-1 inline-flex h-6 w-6">
            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-red-400 opacity-75"></span>
          </span>
        )}
      </button>

      {/* Notification Center */}
      <NotificationCenter
        userId={userId}
        isOpen={isOpen}
        onClose={handleClose}
      />
    </>
  );
}
