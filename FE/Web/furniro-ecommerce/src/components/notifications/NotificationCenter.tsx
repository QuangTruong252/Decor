'use client';

import React, { useState, useEffect } from 'react';
import { NotificationService, type Notification } from '@/services/notificationService';

interface NotificationCenterProps {
  userId: number;
  isOpen: boolean;
  onClose: () => void;
}

export default function NotificationCenter({ userId, isOpen, onClose }: NotificationCenterProps) {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);

  useEffect(() => {
    if (isOpen) {
      loadNotifications();
    }
  }, [isOpen, userId]);

  useEffect(() => {
    // Listen for real-time notifications
    const handleNewNotification = (event: CustomEvent) => {
      const notification = event.detail as Notification;
      if (notification.userId === userId) {
        setNotifications(prev => [notification, ...prev]);
        setUnreadCount(prev => prev + 1);
      }
    };

    window.addEventListener('newNotification', handleNewNotification as EventListener);
    
    return () => {
      window.removeEventListener('newNotification', handleNewNotification as EventListener);
    };
  }, [userId]);

  const loadNotifications = () => {
    const userNotifications = NotificationService.getUserNotifications(userId);
    const unread = NotificationService.getUnreadCount(userId);
    
    setNotifications(userNotifications);
    setUnreadCount(unread);
  };

  const handleMarkAsRead = (notificationId: string) => {
    NotificationService.markAsRead(notificationId);
    setNotifications(prev => 
      prev.map(notif => 
        notif.id === notificationId 
          ? { ...notif, isRead: true, readAt: new Date().toISOString() }
          : notif
      )
    );
    setUnreadCount(prev => Math.max(0, prev - 1));
  };

  const handleMarkAllAsRead = () => {
    NotificationService.markAllAsRead(userId);
    setNotifications(prev => 
      prev.map(notif => ({ 
        ...notif, 
        isRead: true, 
        readAt: new Date().toISOString() 
      }))
    );
    setUnreadCount(0);
  };

  const handleDeleteNotification = (notificationId: string) => {
    NotificationService.deleteNotification(notificationId);
    setNotifications(prev => prev.filter(notif => notif.id !== notificationId));
    
    // Update unread count if deleted notification was unread
    const deletedNotification = notifications.find(n => n.id === notificationId);
    if (deletedNotification && !deletedNotification.isRead) {
      setUnreadCount(prev => Math.max(0, prev - 1));
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffInHours = (now.getTime() - date.getTime()) / (1000 * 60 * 60);

    if (diffInHours < 1) {
      const diffInMinutes = Math.floor(diffInHours * 60);
      return `${diffInMinutes}m ago`;
    } else if (diffInHours < 24) {
      return `${Math.floor(diffInHours)}h ago`;
    } else {
      const diffInDays = Math.floor(diffInHours / 24);
      return `${diffInDays}d ago`;
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'urgent': return 'text-red-600';
      case 'high': return 'text-orange-600';
      case 'medium': return 'text-blue-600';
      default: return 'text-gray-600';
    }
  };

  const getPriorityIcon = (priority: string) => {
    switch (priority) {
      case 'urgent': return '🚨';
      case 'high': return '⚠️';
      case 'medium': return 'ℹ️';
      default: return '📢';
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-hidden">
      {/* Backdrop */}
      <div className="absolute inset-0 bg-black bg-opacity-50" onClick={onClose}></div>
      
      {/* Notification panel */}
      <div className="absolute right-0 top-0 h-full w-full max-w-md bg-white shadow-xl">
        <div className="flex flex-col h-full">
          {/* Header */}
          <div className="flex items-center justify-between p-4 border-b border-gray-200">
            <div className="flex items-center space-x-2">
              <h2 className="text-lg font-semibold text-gray-900">Notifications</h2>
              {unreadCount > 0 && (
                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
                  {unreadCount}
                </span>
              )}
            </div>
            
            <div className="flex items-center space-x-2">
              {unreadCount > 0 && (
                <button
                  onClick={handleMarkAllAsRead}
                  className="text-sm text-blue-600 hover:text-blue-700"
                >
                  Mark all read
                </button>
              )}
              
              <button
                onClick={onClose}
                className="text-gray-400 hover:text-gray-600"
              >
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          </div>

          {/* Notifications list */}
          <div className="flex-1 overflow-y-auto">
            {notifications.length === 0 ? (
              <div className="flex flex-col items-center justify-center h-full text-gray-500">
                <svg className="w-12 h-12 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 17h5l-5 5v-5zM9 7H4l5-5v5zm6 10V7a2 2 0 00-2-2H7a2 2 0 00-2 2v10a2 2 0 002 2h6a2 2 0 002-2z" />
                </svg>
                <p className="text-sm">No notifications yet</p>
              </div>
            ) : (
              <div className="divide-y divide-gray-200">
                {notifications.map((notification) => (
                  <div
                    key={notification.id}
                    className={`p-4 hover:bg-gray-50 transition-colors ${
                      !notification.isRead ? 'bg-blue-50' : ''
                    }`}
                  >
                    <div className="flex items-start space-x-3">
                      {/* Priority icon */}
                      <div className="flex-shrink-0 mt-1">
                        <span className="text-lg">
                          {getPriorityIcon(notification.priority)}
                        </span>
                      </div>

                      {/* Notification content */}
                      <div className="flex-1 min-w-0">
                        <div className="flex items-start justify-between">
                          <div className="flex-1">
                            <h4 className={`text-sm font-medium ${
                              !notification.isRead ? 'text-gray-900' : 'text-gray-700'
                            }`}>
                              {notification.title}
                            </h4>
                            <p className={`mt-1 text-sm ${
                              !notification.isRead ? 'text-gray-700' : 'text-gray-500'
                            }`}>
                              {notification.message}
                            </p>
                            
                            <div className="flex items-center justify-between mt-2">
                              <span className="text-xs text-gray-500">
                                {formatDate(notification.createdAt)}
                              </span>
                              
                              <div className="flex items-center space-x-2">
                                {!notification.isRead && (
                                  <button
                                    onClick={() => handleMarkAsRead(notification.id)}
                                    className="text-xs text-blue-600 hover:text-blue-700"
                                  >
                                    Mark read
                                  </button>
                                )}
                                
                                <button
                                  onClick={() => handleDeleteNotification(notification.id)}
                                  className="text-xs text-red-600 hover:text-red-700"
                                >
                                  Delete
                                </button>
                              </div>
                            </div>
                          </div>

                          {/* Unread indicator */}
                          {!notification.isRead && (
                            <div className="flex-shrink-0 ml-2">
                              <div className="w-2 h-2 bg-blue-600 rounded-full"></div>
                            </div>
                          )}
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="p-4 border-t border-gray-200">
            <button className="w-full text-sm text-center text-blue-600 hover:text-blue-700">
              View All Notifications
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
