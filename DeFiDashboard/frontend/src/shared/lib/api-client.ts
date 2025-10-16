import axios, { AxiosError } from 'axios';
import type { ApiErrorResponse } from '../types/api.types';

/**
 * Configured Axios instance for API communication
 */
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5280',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * Response interceptor for error handling
 */
apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiErrorResponse>) => {
    if (error.response) {
      // Server responded with error status
      console.error('API Error:', {
        status: error.response.status,
        data: error.response.data,
        url: error.config?.url,
      });

      // Handle specific error cases
      if (error.response.status === 401) {
        // Handle unauthorized (future: redirect to login)
        console.error('Unauthorized access');
      } else if (error.response.status === 404) {
        console.error('Resource not found');
      } else if (error.response.status >= 500) {
        console.error('Server error');
      }
    } else if (error.request) {
      // Request made but no response
      console.error('Network Error: No response from server', error.request);
    } else {
      // Request setup error
      console.error('Request Error:', error.message);
    }

    return Promise.reject(error);
  }
);

/**
 * Request interceptor (for future auth token injection)
 */
apiClient.interceptors.request.use(
  (config) => {
    // Future: Add auth token here
    // const token = localStorage.getItem('auth_token');
    // if (token) {
    //   config.headers.Authorization = `Bearer ${token}`;
    // }
    return config;
  },
  (error) => Promise.reject(error)
);
