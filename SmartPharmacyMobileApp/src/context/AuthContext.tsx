import React, { createContext, useContext, useState, useEffect } from 'react';
import { AuthService, AuthResponse } from '../services/authService';

interface AuthContextType {
  user: AuthResponse | null;
  loading: boolean;
  login: (userData: AuthResponse) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<AuthResponse | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const initAuth = async () => {
      try {
        const storedUser = await AuthService.getStoredUser();
        setUser(storedUser);
      } catch (e) {
        console.error('Failed to load auth', e);
      } finally {
        setLoading(false);
      }
    };
    initAuth();
  }, []);

  const login = (userData: AuthResponse) => {
    setUser(userData);
  };

  const logout = async () => {
    await AuthService.logout();
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within an AuthProvider');
  return context;
};
