// src/utils/api.js
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || '/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request Interceptor: đính kèm Bearer token nếu có
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token && !config.headers['Authorization']) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Quản lý hàng đợi refresh token để tránh bão gọi trùng lặp
let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

// Response Interceptor: xử lý tự động refresh token khi gặp 401
api.interceptors.response.use(
  (response) => {
    // Trả về data cho nhất quán
    return response.data;
  },
  async (error) => {
    const originalRequest = error.config;

    // Nếu không có response hoặc là request login/refresh-token thì không retry
    if (!error.response) {
      return Promise.reject(error);
    }

    const { status } = error.response;
    const isAuthEndpoint = originalRequest.url?.includes('/auth/login') || originalRequest.url?.includes('/auth/refresh-token');

    if (status === 401 && !originalRequest._retry && !isAuthEndpoint) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            originalRequest.headers['Authorization'] = `Bearer ${token}`;
            return api(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      const refreshToken = localStorage.getItem('refreshToken');

      if (!refreshToken) {
        isRefreshing = false;
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('currentUser');
        window.location.href = '/login';
        return Promise.reject(error);
      }

      try {
        const response = await axios.post(`${API_BASE_URL}/auth/refresh-token`, {
          refreshToken: refreshToken,
        });

        const data = response.data;
        const newToken = data.token;
        const newRefreshToken = data.refreshToken;

        if (newToken) {
          localStorage.setItem('token', newToken);
          if (newRefreshToken) {
            localStorage.setItem('refreshToken', newRefreshToken);
          }
          api.defaults.headers.common['Authorization'] = `Bearer ${newToken}`;
          processQueue(null, newToken);

          originalRequest.headers['Authorization'] = `Bearer ${newToken}`;
          return api(originalRequest);
        } else {
          throw new Error('Refresh token không hợp lệ.');
        }
      } catch (refreshErr) {
        processQueue(refreshErr, null);
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('currentUser');
        window.location.href = '/login';
        return Promise.reject(refreshErr);
      } finally {
        isRefreshing = false;
      }
    }

    // Trích xuất error message chi tiết
    let errorDetail = `HTTP ${status}`;
    if (error.response.data) {
      if (typeof error.response.data === 'string') {
        errorDetail = error.response.data;
      } else if (error.response.data.message) {
        errorDetail = error.response.data.message;
      } else {
        errorDetail = JSON.stringify(error.response.data);
      }
    }

    return Promise.reject(new Error(errorDetail));
  }
);

// Wrapper tương thích ngược với các lệnh gọi apiCall(endpoint, options) cũ
export const apiCall = async (endpoint, options = {}) => {
  const method = (options.method || 'GET').toLowerCase();
  let data = options.body;

  if (typeof data === 'string') {
    try {
      data = JSON.parse(data);
    } catch {
      // Giữ nguyên chuỗi nếu không parse được
    }
  }

  const config = {
    url: endpoint,
    method,
    headers: options.headers ? { ...options.headers } : {},
    params: options.params,
    data,
  };

  return api(config);
};

export default api;
