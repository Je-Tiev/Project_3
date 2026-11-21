import React from 'react';
import { LayoutDashboard, Users, DoorOpen, Settings, FileText, BarChart3, LogOut } from 'lucide-react';
import { useApp } from '../../contexts/AppContext';
import { useNavigate, useLocation } from 'react-router-dom';

const Sidebar = () => {
  const { logout } = useApp();
  const navigate = useNavigate();
  const location = useLocation();

  const menuItems = [
    { key: 'dashboard', path: '/admin', label: 'Bảng Điều Khiển', icon: LayoutDashboard },
    { key: 'meetings', path: '/admin/meetings', label: 'Cuộc Họp', icon: Users },
    { key: 'rooms', path: '/admin/rooms', label: 'Phòng Họp Vật Lý', icon: DoorOpen },
    { key: 'users', path: '/admin/users', label: 'Người Dùng', icon: Users },
    { key: 'documents', path: '/admin/documents', label: 'Tài Liệu', icon: FileText },
    { key: 'reports', path: '/admin/reports', label: 'Báo Cáo', icon: BarChart3 },
    { key: 'settings', path: '/admin/settings', label: 'Cài Đặt', icon: Settings }
  ];

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const isItemActive = (item) => {
    if (item.key === 'dashboard') {
      return location.pathname === '/admin' || location.pathname === '/admin/' || location.pathname === '/admin/dashboard';
    }
    return location.pathname.startsWith(item.path);
  };

  return (
    <div className="w-64 bg-slate-900 text-slate-100 h-screen fixed left-0 top-0 flex flex-col border-r border-slate-800 z-30">
      {/* Logo */}
      <div className="p-6 border-b border-slate-800">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-red-600 rounded-xl flex items-center justify-center shadow-md">
            <span className="text-white text-sm font-black">BK</span>
          </div>
          <div>
            <h1 className="text-base font-bold text-white tracking-tight">Paperless Meeting</h1>
            <p className="text-xs text-slate-400">Quản trị hệ thống</p>
          </div>
        </div>
      </div>

      {/* Menu Items */}
      <div className="flex-1 py-4 space-y-1 px-3 overflow-y-auto">
        {menuItems.map((item) => {
          const Icon = item.icon;
          const active = isItemActive(item);
          return (
            <button
              key={item.key}
              onClick={() => navigate(item.path)}
              className={`w-full flex items-center gap-3 px-4 py-2.5 rounded-xl text-left text-sm font-medium transition-all ${
                active
                  ? 'bg-red-600 text-white shadow-sm'
                  : 'text-slate-300 hover:bg-slate-800 hover:text-white'
              }`}
            >
              <Icon className="w-5 h-5 shrink-0" />
              <span>{item.label}</span>
            </button>
          );
        })}
      </div>

      {/* Bottom User / Logout */}
      <div className="p-4 border-t border-slate-800">
        <button
          onClick={handleLogout}
          className="w-full flex items-center gap-2.5 px-3 py-2 text-sm text-slate-400 hover:text-red-400 hover:bg-slate-800 rounded-lg transition-colors"
        >
          <LogOut className="w-4 h-4" />
          <span>Đăng xuất</span>
        </button>
      </div>
    </div>
  );
};

export default Sidebar;
