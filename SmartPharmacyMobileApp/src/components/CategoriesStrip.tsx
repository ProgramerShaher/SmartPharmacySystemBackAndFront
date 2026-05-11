import React, { useRef, useEffect } from 'react';
import { View, Text, ScrollView, TouchableOpacity, StyleSheet, Animated } from 'react-native';

interface CategoryItem {
  id: number;
  name: string;
}

interface Props {
  categories: CategoryItem[];
  selectedId?: number;
  onSelect: (category: CategoryItem) => void;
}

const CategoriesStrip: React.FC<Props> = ({ categories, selectedId, onSelect }) => {
  const scrollRef = useRef<ScrollView>(null);

  const allCategories: CategoryItem[] = [{ id: 0, name: 'الكل' }, ...categories];

  return (
    <View style={styles.container}>
      <ScrollView
        ref={scrollRef}
        horizontal
        showsHorizontalScrollIndicator={false}
        contentContainerStyle={styles.scrollContent}
      >
        {allCategories.map((cat) => {
          const isActive = selectedId === cat.id;
          return (
            <TouchableOpacity
              key={cat.id}
              activeOpacity={0.7}
              onPress={() => onSelect(cat)}
              style={[
                styles.chip,
                isActive ? styles.chipActive : styles.chipInactive,
              ]}
            >
              <Text style={[
                styles.chipText,
                isActive ? styles.chipTextActive : styles.chipTextInactive,
              ]}>
                {cat.name}
              </Text>
            </TouchableOpacity>
          );
        })}
      </ScrollView>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    paddingVertical: 12,
  },
  scrollContent: {
    paddingHorizontal: 16,
    gap: 10,
    flexDirection: 'row-reverse',
  },
  chip: {
    paddingHorizontal: 20,
    paddingVertical: 10,
    borderRadius: 25,
    elevation: 2,
    shadowColor: '#000',
    shadowOpacity: 0.08,
    shadowRadius: 6,
    shadowOffset: { width: 0, height: 2 },
  },
  chipActive: {
    backgroundColor: '#0a3d62',
    shadowOpacity: 0.25,
    shadowRadius: 8,
    elevation: 6,
  },
  chipInactive: {
    backgroundColor: '#fff',
  },
  chipText: {
    fontSize: 14,
    fontWeight: '700',
  },
  chipTextActive: {
    color: '#fff',
  },
  chipTextInactive: {
    color: '#636e72',
  },
});

export default CategoriesStrip;
