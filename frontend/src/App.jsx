import React from 'react';
import { AppProvider } from './contexts/AppContext';
import { Router, Route } from './contexts/RouterContext';
import LoginPage from './pages/LoginPage';
import HomePage from './pages/HomePage';
import MeetingDetailPage from './pages/MeetingDetailPage';

function App() {
  return (
    <AppProvider>
      <Router>
        <Route path="/login" component={LoginPage} />
        <Route path="/" component={HomePage} />
        <Route path="/meeting/:id" component={MeetingDetailPage} />
      </Router>
    </AppProvider>
  );
}

export default App;