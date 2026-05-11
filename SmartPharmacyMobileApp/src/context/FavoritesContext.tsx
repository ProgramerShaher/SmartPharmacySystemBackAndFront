import React, { createContext, useContext, useState, useEffect } from 'react';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { Medicine } from '../services/catalogService';

interface FavoritesContextType {
  favorites: Medicine[];
  addFavorite: (medicine: Medicine) => void;
  removeFavorite: (medicineId: number) => void;
  isFavorite: (medicineId: number) => boolean;
  toggleFavorite: (medicine: Medicine) => void;
}

const FavoritesContext = createContext<FavoritesContextType | undefined>(undefined);

export const FavoritesProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [favorites, setFavorites] = useState<Medicine[]>([]);

  useEffect(() => {
    const load = async () => {
      const saved = await AsyncStorage.getItem('favorites');
      if (saved) setFavorites(JSON.parse(saved));
    };
    load();
  }, []);

  useEffect(() => {
    AsyncStorage.setItem('favorites', JSON.stringify(favorites));
  }, [favorites]);

  const addFavorite = (medicine: Medicine) => {
    setFavorites(prev => {
      if (prev.find(f => f.id === medicine.id)) return prev;
      return [...prev, medicine];
    });
  };

  const removeFavorite = (medicineId: number) => {
    setFavorites(prev => prev.filter(f => f.id !== medicineId));
  };

  const isFavorite = (medicineId: number) => {
    return favorites.some(f => f.id === medicineId);
  };

  const toggleFavorite = (medicine: Medicine) => {
    if (isFavorite(medicine.id)) {
      removeFavorite(medicine.id);
    } else {
      addFavorite(medicine);
    }
  };

  return (
    <FavoritesContext.Provider value={{ favorites, addFavorite, removeFavorite, isFavorite, toggleFavorite }}>
      {children}
    </FavoritesContext.Provider>
  );
};

export const useFavorites = () => {
  const context = useContext(FavoritesContext);
  if (!context) throw new Error('useFavorites must be used within a FavoritesProvider');
  return context;
};
