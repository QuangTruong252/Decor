'use client';

import React from 'react';

export type ViewMode = 'grid' | 'list';

interface ViewToggleProps {
  currentView: ViewMode;
  onViewChange: (view: ViewMode) => void;
  className?: string;
}

const ViewToggle: React.FC<ViewToggleProps> = ({
  currentView,
  onViewChange,
  className = ""
}) => {
  const buttonBaseClass = "p-2 border transition-colors duration-200";
  const activeClass = "bg-primary text-white border-primary";
  const inactiveClass = "bg-white text-gray-600 border-gray-300 hover:bg-gray-50";

  return (
    <div className={`flex items-center ${className}`}>
      <span className="text-sm text-gray-700 mr-3">View:</span>
      <div className="flex border border-gray-300 rounded-md overflow-hidden">
        <button
          onClick={() => onViewChange('grid')}
          className={`${buttonBaseClass} ${
            currentView === 'grid' ? activeClass : inactiveClass
          }`}
          aria-label="Grid view"
          title="Grid view"
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z" />
          </svg>
        </button>
        <button
          onClick={() => onViewChange('list')}
          className={`${buttonBaseClass} ${
            currentView === 'list' ? activeClass : inactiveClass
          }`}
          aria-label="List view"
          title="List view"
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 10h16M4 14h16M4 18h16" />
          </svg>
        </button>
      </div>
    </div>
  );
};

export default ViewToggle;
