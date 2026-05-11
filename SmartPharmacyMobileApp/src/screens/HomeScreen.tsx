import React, { useEffect, useState, useCallback } from 'react';
import {
  View,
  Text,
  FlatList,
  StyleSheet,
  ActivityIndicator,
  TextInput,
  RefreshControl,
  TouchableOpacity,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { CatalogService, Category, Medicine } from '../services/catalogService';
import { useAuth } from '../context/AuthContext';
import CategoriesStrip from '../components/CategoriesStrip';
import MedicineCard from '../components/MedicineCard';

const HomeScreen = ({ navigation }: any) => {
  const [categories, setCategories] = useState<Category[]>([]);
  const [medicines, setMedicines] = useState<Medicine[]>([]);
  const [selectedCategoryId, setSelectedCategoryId] = useState<number>(0);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const { logout } = useAuth();

  const loadMedicines = useCallback(async (catId: number, q: string) => {
    try {
      const meds = await CatalogService.getMedicines(
        catId > 0 ? catId : undefined,
        q || undefined
      );
      setMedicines(Array.isArray(meds) ? meds : []);
    } catch (e) {
      console.error('Error loading medicines:', e);
    }
  }, []);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const cats = await CatalogService.getCategories();
      if (Array.isArray(cats)) setCategories(cats);
      await loadMedicines(selectedCategoryId, search);
    } catch (e) {
      console.error('Data Loading Error:', e);
    } finally {
      setLoading(false);
    }
  }, [selectedCategoryId, search]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const onRefresh = async () => {
    setRefreshing(true);
    try {
      const cats = await CatalogService.getCategories();
      if (Array.isArray(cats)) setCategories(cats);
      await loadMedicines(selectedCategoryId, search);
    } catch (e) {
      console.error('Refresh Error:', e);
    } finally {
      setRefreshing(false);
    }
  };

  const handleCategorySelect = (cat: { id: number; name: string }) => {
    setSelectedCategoryId(cat.id);
  };

  const renderMedicine = ({ item }: { item: Medicine }) => (
    <MedicineCard
      medicine={item}
      onPress={() => navigation.navigate('MedicineDetails', { id: item.id })}
    />
  );

  const selectedCategoryName =
    selectedCategoryId === 0
      ? 'كل المنتجات'
      : categories.find((c) => c.id === selectedCategoryId)?.name || '';

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <View style={styles.headerTop}>
          <TouchableOpacity style={styles.iconBtn} onPress={logout}>
            <Ionicons name="log-out-outline" size={24} color="#fff" />
          </TouchableOpacity>
          <View style={styles.logoBlock}>
            <Text style={styles.headerTitle}>الصيدلية الملكية</Text>
            <View style={styles.onlineDot} />
          </View>
          <TouchableOpacity style={styles.iconBtn}>
            <Ionicons name="notifications-outline" size={24} color="#fff" />
          </TouchableOpacity>
        </View>

        <View style={styles.searchRow}>
          <Ionicons name="search-outline" size={18} color="#b2bec3" style={styles.searchIcon} />
          <TextInput
            style={styles.searchInput}
            placeholder="ابحث عن دواء..."
            placeholderTextColor="#b2bec3"
            value={search}
            onChangeText={setSearch}
          />
        </View>
      </View>

      <CategoriesStrip
        categories={categories}
        selectedId={selectedCategoryId}
        onSelect={handleCategorySelect}
      />

      <View style={styles.resultBar}>
        <Text style={styles.resultTitle}>{selectedCategoryName}</Text>
        <Text style={styles.resultCount}>{medicines.length} منتج</Text>
      </View>

      {loading ? (
        <View style={styles.loader}>
          <ActivityIndicator size="large" color="#0a3d62" />
          <Text style={styles.loadingText}>جاري التحميل...</Text>
        </View>
      ) : (
        <FlatList
          data={medicines}
          renderItem={renderMedicine}
          keyExtractor={(item) => item.id.toString()}
          numColumns={2}
          contentContainerStyle={styles.listContent}
          columnWrapperStyle={styles.columnWrap}
          showsVerticalScrollIndicator={false}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={onRefresh}
              colors={['#0a3d62']}
              tintColor="#0a3d62"
            />
          }
          ListEmptyComponent={
            <View style={styles.emptyBox}>
              <Ionicons name="cube-outline" size={70} color="#dfe6e9" />
              <Text style={styles.emptyText}>لا توجد منتجات</Text>
            </View>
          }
        />
      )}
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f5f6fa',
  },
  header: {
    backgroundColor: '#0a3d62',
    paddingHorizontal: 20,
    paddingTop: 50,
    paddingBottom: 20,
    borderBottomLeftRadius: 30,
    borderBottomRightRadius: 30,
    elevation: 10,
    shadowColor: '#0a3d62',
    shadowOpacity: 0.3,
    shadowRadius: 20,
  },
  headerTop: {
    flexDirection: 'row-reverse',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 16,
  },
  logoBlock: {
    flexDirection: 'row-reverse',
    alignItems: 'center',
    gap: 8,
  },
  headerTitle: {
    color: '#fff',
    fontSize: 22,
    fontWeight: '900',
  },
  onlineDot: {
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: '#2ecc71',
    borderWidth: 1.5,
    borderColor: '#fff',
  },
  iconBtn: {
    width: 40,
    height: 40,
    borderRadius: 14,
    backgroundColor: 'rgba(255,255,255,0.15)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  searchRow: {
    flexDirection: 'row-reverse',
    backgroundColor: '#fff',
    borderRadius: 16,
    paddingHorizontal: 14,
    height: 46,
    alignItems: 'center',
    elevation: 4,
    shadowColor: '#000',
    shadowOpacity: 0.1,
    shadowRadius: 8,
  },
  searchIcon: {
    marginLeft: 8,
  },
  searchInput: {
    flex: 1,
    textAlign: 'right',
    fontSize: 14,
    color: '#2d3436',
  },
  resultBar: {
    flexDirection: 'row-reverse',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 20,
    paddingVertical: 8,
  },
  resultTitle: {
    fontSize: 18,
    fontWeight: '900',
    color: '#0a3d62',
  },
  resultCount: {
    fontSize: 13,
    color: '#b2bec3',
    fontWeight: '600',
  },
  listContent: {
    paddingHorizontal: 14,
    paddingBottom: 30,
  },
  columnWrap: {
    justifyContent: 'space-between',
  },
  loader: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  loadingText: {
    marginTop: 12,
    fontSize: 14,
    color: '#0a3d62',
    fontWeight: '700',
  },
  emptyBox: {
    alignItems: 'center',
    marginTop: 80,
  },
  emptyText: {
    marginTop: 16,
    fontSize: 16,
    color: '#bdc3c7',
    fontWeight: '700',
  },
});

export default HomeScreen;
