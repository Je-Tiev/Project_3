import React from 'react';
import { AppProvider, useApp } from './contexts/AppContext';
import { Router, Route } from './contexts/RouterContext';
import LoginPage from './pages/LoginPage';
import HomePage from './pages/HomePage';
import MeetingDetailPage from './pages/MeetingDetailPage';
import AdminLayout from './pages/admin/AdminLayout';

const AppContent = () => {
  const { isAuthenticated, currentUser } = useApp();

  // Chưa đăng nhập: hiển thị login page
  if (!isAuthenticated) {
    return (
      <Router>
        <Route path="/login" component={LoginPage} />
        <Route path="/" component={LoginPage} />
      </Router>
    );
  }

  // Đã đăng nhập và role = Admin: hiển thị AdminLayout
  if (currentUser?.role === 'Admin') {
    return (
      <Router>
        <Route path="/admin" component={AdminLayout} />
        <Route path="/" component={AdminLayout} />
      </Router>
    );
  }

  // Role = User (bình thường): hiển thị HomePage (giao diện user)
  return (
    <Router>
      <Route path="/" component={HomePage} />
      <Route path="/meeting/:id" component={MeetingDetailPage} />
    </Router>
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