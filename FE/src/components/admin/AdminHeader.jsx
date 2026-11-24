import React from 'react';
import { Search, User, LogOut } from 'lucide-react';
import { useApp } from '../../contexts/AppContext';
import LiveClock from '../LiveClock';
import { useNavigate } from 'react-router-dom';

const AdminHeader = () => {
  const { currentUser, logout } = useApp();
  const [searchQuery, setSearchQuery] = React.useState('');
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="h-16 bg-white border-b border-gray-200 flex items-center justify-between px-6 fixed top-0 left-64 right-0 z-20 shadow-sm">
      {/* Search Bar */}
      <div className="flex-1 max-w-md">
        <div className="relative">
          <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Tìm kiếm cuộc họp, phòng họp, tài liệu..."
            className="w-full pl-10 pr-4 py-2 bg-gray-50 text-gray-800 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-all"
          />
        </div>
      </div>

      {/* User Info & LiveClock */}
      <div className="flex items-center gap-4">
        <LiveClock className="text-sm font-semibold text-gray-600 bg-gray-100 px-3 py-1 rounded-md" />
        <div className="flex items-center gap-2.5">
          <div className="w-8 h-8 bg-red-100 text-red-700 rounded-full flex items-center justify-center font-bold text-sm">
            {currentUser?.fullName ? currentUser.fullName.charAt(0) : 'A'}
          </div>
          <span className="text-gray-800 text-sm font-semibold">{currentUser?.fullName || 'Administrator'}</span>
        </div>
        <button
          onClick={handleLogout}
          className="p-2 text-gray-400 hover:text-red-600 hover:bg-gray-100 rounded-lg transition-colors"
          title="Đăng xuất"
        >
          <LogOut className="w-4 h-4" />
        </button>
      </div>
    </div>
  );
};

export default AdminHeader;

