import React from 'react';
import { AppProvider, useApp } from './contexts/AppContext';
import { Routes, Route, Navigate } from 'react-router-dom'; 

import LoginPage from './pages/LoginPage';
import HomePage from './pages/HomePage';
import MeetingDetailPage from './pages/MeetingDetailPage';
import AdminLayout from './pages/admin/AdminLayout';

const AppContent = () => {
  const { isAuthenticated, currentUser } = useApp();

  // Nếu chưa đăng nhập
  if (!isAuthenticated) {
    return (
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        {/* Tất cả các đường dẫn khác (vd: "/", "/admin") sẽ tự động chuyển về /login */}
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    );
  }

  // Nếu đã đăng nhập và là Admin
  if (currentUser?.role === 'Admin') {
    return (
      <Routes>
        {/* Dùng "/*" để AdminLayout có thể quản lý các route con của nó 
          (vd: /admin/users, /admin/dashboard)
        */}
        <Route path="/admin/*" element={<AdminLayout />} />
        {/* Tất cả các đường dẫn khác (vd: "/", "/login") sẽ tự động chuyển về /admin */}
        <Route path="*" element={<Navigate to="/admin" replace />} />
      </Routes>
    );
  }

  // Nếu đã đăng nhập và là User
  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/meeting/:id" element={<MeetingDetailPage />} />
      {/* Tất cả các đường dẫn không hợp lệ (vd: "/admin", "/login", "/abc") 
        sẽ tự động chuyển về trang chủ "/" 
      */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};

function App() {
  return (
    // AppProvider vẫn bọc bên ngoài...
    <AppProvider>
      {/* ...nhưng BrowserRouter đã bọc ở file main.jsx rồi, 
          nên AppContent chỉ cần chứa Routes là đủ */}
      <AppContent />
    </AppProvider>
  );
}

export default App;