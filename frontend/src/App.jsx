import React from 'react';
import { AppProvider, useApp } from './contexts/AppContext';
import { Router, Route } from './contexts/RouterContext';
import LoginPage from './pages/LoginPage';
import HomePage from './pages/HomePage';
import MeetingDetailPage from './pages/MeetingDetailPage';
import AdminLayout from './pages/admin/AdminLayout';

const AppContent = () => {
  const { isAuthenticated, currentUser } = useApp();

  return (
    <Router>
      <Route path="/login" component={LoginPage} />
      {isAuthenticated && currentUser?.role === 'admin' ? (
        <>
          <Route path="/admin" component={AdminLayout} />
          <Route path="/" component={AdminLayout} />
        </>
      ) : (
        <>
          <Route path="/" component={HomePage} />
          <Route path="/meeting/:id" component={MeetingDetailPage} />
        </>
      )}
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