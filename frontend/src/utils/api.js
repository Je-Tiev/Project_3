// src/utils/api.js
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5075/api';

export const apiCall = async (endpoint, options = {}) => {
  const token = localStorage.getItem('token');
  
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers,
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  try {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      ...options,
      headers,
    });

    if (!response.ok) {
      let errorDetail = `HTTP ${response.status}`;
      try {
          const errorData = await response.json(); 
          errorDetail = errorData.message || JSON.stringify(errorData); 
      } catch (parseErr) {
        
          try {
              errorDetail = await response.text();
          } catch (textErr) {
             
          }
      }

      if (response.status === 401) {
          localStorage.removeItem('token');
          window.location.href = '/login';
      }
      throw new Error(errorDetail);
    }

    // >>> BƯỚC KHẮC PHỤC CHÍNH: Kiểm tra Content-Type
    const contentType = response.headers.get("content-type");
    
    // Nếu Content-Type chứa 'application/json', thì đọc JSON
    if (contentType && contentType.includes("application/json")) {
      return await response.json();
    } 
    
    // Nếu không phải JSON (là 'text/plain' như trong ảnh), thì đọc Text
    // Hàm này sẽ trả về chuỗi "Đăng ký thành công."
    return await response.text(); 
    
  } catch (error) {
    console.error('API Error:', error);
    throw error;
  }
};