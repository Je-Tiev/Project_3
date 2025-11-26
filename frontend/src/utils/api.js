//src/utils/api.js
const API_BASE_URL = import.meta.env.VITE_API_URL || '/api';

export const apiCall = async (endpoint, options = {}) => {
  const token = localStorage.getItem('token');
  
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers,
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
   const url = `${API_BASE_URL}${endpoint}`;
  console.log("👉 Calling API URL:", url);
  try {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      ...options,
      headers,
    });

    if (!response.ok) {
      if (response.status === 401 && endpoint !== '/auth/login') {
        // Token hết hạn - cần refresh hoặc logout
        localStorage.removeItem('token');
        window.location.href = '/login';
      }
      const msg = await response.text();
      throw new Error(msg || `HTTP ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error('API Error:', error);
    throw error;
  }
};