import AsyncStorage from '@react-native-async-storage/async-storage';
import apiClient from './apiClient';

export interface RegisterData {
  fullName: string;
  phoneNumber: string;
  password: string;
  email?: string;
}

export interface LoginData {
  phoneNumber: string;
  password: string;
}

export interface AuthResponse {
  customerId: number;
  fullName: string;
  phoneNumber: string;
  token: string;
}

export const AuthService = {
  async register(data: RegisterData): Promise<AuthResponse> {
    const response = await apiClient.post('/mobile/auth/register', data);
    return response.data.data;
  },

  async login(data: LoginData): Promise<AuthResponse> {
    const response = await apiClient.post('/mobile/auth/login', data);
    const auth: AuthResponse = response.data.data;
    // Persist token and user info
    await AsyncStorage.setItem('customerToken', auth.token);
    await AsyncStorage.setItem('customerInfo', JSON.stringify(auth));
    return auth;
  },

  async logout(): Promise<void> {
    await AsyncStorage.removeItem('customerToken');
    await AsyncStorage.removeItem('customerInfo');
  },

  async getStoredUser(): Promise<AuthResponse | null> {
    const raw = await AsyncStorage.getItem('customerInfo');
    return raw ? JSON.parse(raw) : null;
  },

  async isLoggedIn(): Promise<boolean> {
    const token = await AsyncStorage.getItem('customerToken');
    return !!token;
  },
};
