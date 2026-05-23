import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  Image,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  ActivityIndicator,
  Dimensions,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { CatalogService, Medicine } from '../services/catalogService';
import { getImageFullUrl } from '../config/api';
import { useFavorites } from '../context/FavoritesContext';
import { useCart } from '../context/CartContext';

const { width } = Dimensions.get('window');

const MedicineDetailsScreen = ({ route, navigation }: any) => {
  const { id } = route.params;
  const [medicine, setMedicine] = useState<Medicine | null>(null);
  const [loading, setLoading] = useState(true);
  const [quantity, setQuantity] = useState(1);
  const { isFavorite, toggleFavorite } = useFavorites();
  const { addToCart } = useCart();

  useEffect(() => {
    (async () => {
      try {
        const data = await CatalogService.getMedicineDetails(id);
        setMedicine(data);
      } catch (e) {
        console.error('Error loading medicine details:', e);
      } finally {
        setLoading(false);
      }
    })();
  }, [id]);

  const handleAddToCart = () => {
    if (!medicine) return;
    for (let i = 0; i < quantity; i++) {
      addToCart(medicine);
    }
    navigation.goBack();
  };

  if (loading) {
    return (
      <View style={styles.loader}>
        <ActivityIndicator size="large" color="#0a3d62" />
      </View>
    );
  }

  if (!medicine) {
    return (
      <View style={styles.loader}>
        <Text style={styles.errorText}>لم يتم العثور على المنتج</Text>
      </View>
    );
  }

  const fav = isFavorite(medicine.id);

  return (
    <View style={styles.container}>
      <ScrollView bounces={false} showsVerticalScrollIndicator={false}>
        <View style={styles.imageSection}>
          <Image
            source={
              medicine.imageUrl
                ? { uri: getImageFullUrl(medicine.imageUrl) || undefined }
                : require('../../assets/adaptive-icon.png')
            }
            style={styles.image}
            resizeMode="cover"
          />
          <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
            <Ionicons name="arrow-forward" size={24} color="#fff" />
          </TouchableOpacity>
          <TouchableOpacity
            style={styles.favBtn}
            onPress={() => toggleFavorite(medicine)}
          >
            <Ionicons
              name={fav ? 'heart' : 'heart-outline'}
              size={24}
              color={fav ? '#e84393' : '#fff'}
            />
          </TouchableOpacity>
          {medicine.totalStock !== undefined && medicine.totalStock <= 5 && (
            <View style={styles.stockBadge}>
              <Text style={styles.stockText}>متبقي {medicine.totalStock} فقط</Text>
            </View>
          )}
        </View>

        <View style={styles.body}>
          <Text style={styles.nameAr}>{medicine.name}</Text>
          {medicine.scientificName ? (
            <Text style={styles.nameEn}>{medicine.scientificName}</Text>
          ) : null}

          <View style={styles.priceRow}>
            <Text style={styles.price}>
              {Number(medicine.defaultSalePrice).toLocaleString('ar-YE')}
            </Text>
            <Text style={styles.currency}>ريال</Text>
          </View>

          <View style={styles.ratingRow}>
            {[1, 2, 3, 4, 5].map((star) => (
              <Ionicons key={star} name="star" size={16} color="#f1c40f" />
            ))}
            <Text style={styles.ratingText}>(4.5)</Text>
          </View>

          <View style={styles.divider} />

          {medicine.activeIngredient ? (
            <View style={styles.infoRow}>
              <Text style={styles.infoLabel}>المادة الفعالة</Text>
              <Text style={styles.infoValue}>{medicine.activeIngredient}</Text>
            </View>
          ) : null}

          {medicine.manufacturer ? (
            <View style={styles.infoRow}>
              <Text style={styles.infoLabel}>الشركة المصنعة</Text>
              <Text style={styles.infoValue}>{medicine.manufacturer}</Text>
            </View>
          ) : null}

          {medicine.categoryName ? (
            <View style={styles.infoRow}>
              <Text style={styles.infoLabel}>التصنيف</Text>
              <Text style={styles.infoValue}>{medicine.categoryName}</Text>
            </View>
          ) : null}

          {medicine.totalStock !== undefined ? (
            <View style={styles.infoRow}>
              <Text style={styles.infoLabel}>المخزون</Text>
              <Text style={[
                styles.infoValue,
                { color: medicine.totalStock > 10 ? '#2ecc71' : '#e74c3c' }
              ]}>
                {medicine.totalStock} وحدة
              </Text>
            </View>
          ) : null}

          {medicine.notes ? (
            <>
              <View style={styles.divider} />
              <Text style={styles.descLabel}>ملاحظات</Text>
              <Text style={styles.descText}>{medicine.notes}</Text>
            </>
          ) : null}
        </View>
      </ScrollView>

      <View style={styles.footer}>
        <View style={styles.quantityRow}>
          <TouchableOpacity
            style={styles.qtyBtn}
            onPress={() => setQuantity(q => Math.max(1, q - 1))}
          >
            <Ionicons name="remove" size={20} color="#fff" />
          </TouchableOpacity>
          <Text style={styles.qtyText}>{quantity}</Text>
          <TouchableOpacity
            style={styles.qtyBtn}
            onPress={() => setQuantity(q => q + 1)}
          >
            <Ionicons name="add" size={20} color="#fff" />
          </TouchableOpacity>
        </View>
        <TouchableOpacity style={styles.addBtn} onPress={handleAddToCart}>
          <Ionicons name="cart" size={20} color="#fff" />
          <Text style={styles.addBtnText}>أضف إلى السلة</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f5f6fa',
  },
  loader: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#f5f6fa',
  },
  errorText: {
    fontSize: 16,
    color: '#636e72',
    fontWeight: '700',
  },
  imageSection: {
    width,
    height: width * 0.75,
    backgroundColor: '#eee',
    position: 'relative',
  },
  image: {
    width: '100%',
    height: '100%',
  },
  backBtn: {
    position: 'absolute',
    top: 50,
    right: 16,
    width: 44,
    height: 44,
    borderRadius: 22,
    backgroundColor: 'rgba(0,0,0,0.35)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  favBtn: {
    position: 'absolute',
    top: 50,
    left: 16,
    width: 44,
    height: 44,
    borderRadius: 22,
    backgroundColor: 'rgba(0,0,0,0.35)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  stockBadge: {
    position: 'absolute',
    bottom: 16,
    right: 16,
    backgroundColor: 'rgba(231, 76, 60, 0.9)',
    paddingHorizontal: 14,
    paddingVertical: 6,
    borderRadius: 20,
  },
  stockText: {
    color: '#fff',
    fontSize: 12,
    fontWeight: 'bold',
  },
  body: {
    padding: 20,
    paddingBottom: 120,
  },
  nameAr: {
    fontSize: 24,
    fontWeight: '900',
    color: '#2d3436',
    textAlign: 'right',
  },
  nameEn: {
    fontSize: 14,
    color: '#636e72',
    textAlign: 'right',
    fontStyle: 'italic',
    marginTop: 4,
  },
  priceRow: {
    flexDirection: 'row-reverse',
    alignItems: 'baseline',
    marginTop: 12,
    gap: 6,
  },
  price: {
    fontSize: 32,
    fontWeight: '900',
    color: '#2ecc71',
  },
  currency: {
    fontSize: 16,
    fontWeight: '600',
    color: '#2ecc71',
  },
  ratingRow: {
    flexDirection: 'row-reverse',
    alignItems: 'center',
    gap: 3,
    marginTop: 8,
  },
  ratingText: {
    fontSize: 13,
    color: '#b2bec3',
    marginRight: 6,
    fontWeight: '600',
  },
  divider: {
    height: 1,
    backgroundColor: '#eef0f3',
    marginVertical: 16,
  },
  infoRow: {
    flexDirection: 'row-reverse',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: 8,
    borderBottomWidth: 1,
    borderBottomColor: '#f5f6fa',
  },
  infoLabel: {
    fontSize: 14,
    color: '#b2bec3',
    fontWeight: '600',
  },
  infoValue: {
    fontSize: 14,
    color: '#2d3436',
    fontWeight: '700',
    textAlign: 'left',
    flex: 1,
    marginRight: 16,
  },
  descLabel: {
    fontSize: 16,
    fontWeight: '800',
    color: '#2d3436',
    textAlign: 'right',
    marginBottom: 8,
  },
  descText: {
    fontSize: 14,
    color: '#636e72',
    textAlign: 'right',
    lineHeight: 22,
  },
  footer: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    backgroundColor: '#fff',
    paddingHorizontal: 20,
    paddingVertical: 16,
    paddingBottom: 30,
    flexDirection: 'row-reverse',
    alignItems: 'center',
    gap: 12,
    elevation: 12,
    shadowColor: '#000',
    shadowOpacity: 0.15,
    shadowRadius: 20,
    shadowOffset: { width: 0, height: -4 },
  },
  quantityRow: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#f1f2f6',
    borderRadius: 16,
    paddingHorizontal: 4,
  },
  qtyBtn: {
    width: 40,
    height: 40,
    borderRadius: 12,
    backgroundColor: '#0a3d62',
    justifyContent: 'center',
    alignItems: 'center',
  },
  qtyText: {
    fontSize: 18,
    fontWeight: '900',
    color: '#2d3436',
    marginHorizontal: 16,
    minWidth: 24,
    textAlign: 'center',
  },
  addBtn: {
    flex: 1,
    flexDirection: 'row-reverse',
    backgroundColor: '#2ecc71',
    height: 50,
    borderRadius: 16,
    justifyContent: 'center',
    alignItems: 'center',
    gap: 8,
    elevation: 6,
    shadowColor: '#2ecc71',
    shadowOpacity: 0.35,
    shadowRadius: 10,
  },
  addBtnText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '900',
  },
});

export default MedicineDetailsScreen;
