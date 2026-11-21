import React from 'react';
import { User, LogOut } from 'lucide-react';
import { useApp } from '../contexts/AppContext';
import LiveClock from './LiveClock';

const Header = ({ title, showBackButton = false }) => {
  const { currentUser, logout } = useApp();

  const handleLogout = () => {
    logout();
    window.location.href = '/login';
  };

  const handleBack = () => {
    window.location.href = '/';
  };

  return (
    <div className="bg-gradient-to-r from-red-600 to-red-700 text-white p-4 shadow-lg">
      <div className="flex justify-between items-center max-w-7xl mx-auto">
        <div className="flex items-center gap-4">
          {showBackButton && (
            <button
              onClick={handleBack}
              className="bg-white/20 hover:bg-white/30 px-4 py-2 rounded-lg transition-colors"
            >
              ← Quay Lại
            </button>
          )}
          <h1 className="text-2xl font-bold">{title}</h1>
        </div>
        <div className="flex items-center gap-4">
          <LiveClock className="text-sm font-medium" />
          {currentUser && (
            <>
              <div className="flex items-center gap-2 bg-white/20 px-3 py-2 rounded-lg">
                <User className="w-4 h-4" />
                <span className="text-sm font-semibold">{currentUser.fullName}</span>
              </div>
              <button
                onClick={handleLogout}
                className="bg-white/20 hover:bg-white/30 p-2 rounded-lg transition-colors"
                title="Đăng xuất"
              >
                <LogOut className="w-4 h-4" />
              </button>
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default Header;