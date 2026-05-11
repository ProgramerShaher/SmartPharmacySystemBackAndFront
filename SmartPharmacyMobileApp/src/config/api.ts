export const API_BASE_URL = 'http://10.72.96.158:5150/api';
export const SERVER_URL = 'http://10.72.96.158:5150';

/**
 * Helper to fix Image URLs for Mobile.
 * Replaces localhost with correct IP and prepends server URL if path is relative.
 */
export const getImageFullUrl = (url?: string) => {
  if (!url) return null;
  
  // If it's already a full external URL (not localhost), return it
  if (url.startsWith('http') && !url.includes('localhost') && !url.includes('127.0.0.1')) {
    return url;
  }

  // If it's a relative path starting with /
  if (url.startsWith('/')) {
    return `${SERVER_URL}${url}`;
  }

  // If it contains localhost, replace it with our server IP
  if (url.includes('localhost') || url.includes('127.0.0.1')) {
    return url.replace(/https?:\/\/(localhost|127\.0\.0\.1):?\d*/, SERVER_URL);
  }

  return url;
};
