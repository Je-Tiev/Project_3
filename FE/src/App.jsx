import React from 'react';
import { AppProvider, useApp } from './contexts/AppContext';
import { Routes, Route, Navigate } from 'react-router-dom';

import LoginPage from './pages/LoginPage';
import HomePage from './pages/HomePage';
import MeetingDetailPage from './pages/MeetingDetailPage';
import MeetingDetailsPage from './pages/admin/MeetingDetailsPage';
import AdminLayout from './pages/admin/AdminLayout';
import MeetingPreJoinPage from './pages/MeetingPreJoinPage';
import MeetingDocumentsPage from './pages/MeetingDocumentsPage';

const AppContent = () => {
  const { isAuthenticated, currentUser } = useApp();

  // Nếu chưa đăng nhập → chỉ vào được /login
  if (!isAuthenticated) {
    return (
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    );
  }

  const role = currentUser?.role?.toString().toLowerCase();
  const isAdmin = role === 'admin' || role === 'superadmin';

  // Nếu là Admin / SuperAdmin
  if (isAdmin) {
    return (
      <Routes>
        {/* Admin layout */}
        <Route path="/admin/*" element={<AdminLayout />} />

        {/* Chi tiết meeting trong admin */}
        <Route path="/admin/meeting/:meetingId" element={<MeetingDetailsPage />} />

        {/* Prejoin, Documents và Tham gia meeting */}
        <Route path="/meeting/:meetingId/prejoin" element={<MeetingPreJoinPage />} />
        <Route path="/meeting/:meetingId/documents" element={<MeetingDocumentsPage />} />
        <Route path="/meeting/:meetingId" element={<MeetingDetailPage />} />

        {/* Trang chủ người dùng */}
        <Route path="/" element={<HomePage />} />

        {/* Các route không hợp lệ khác → chuyển hướng về admin */}
        <Route path="*" element={<Navigate to="/admin" replace />} />
      </Routes>
    );
  }

  // Nếu là User thông thường
  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/meeting/:meetingId/prejoin" element={<MeetingPreJoinPage />} />
      <Route path="/meeting/:meetingId/documents" element={<MeetingDocumentsPage />} />
      <Route path="/meeting/:meetingId" element={<MeetingDetailPage />} />

      {/* Sai link → chuyển hướng về trang chủ */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};

function App() {
  return (
    <AppProvider>
      <AppContent />
    </AppProvider>
  );
}

export default App;
