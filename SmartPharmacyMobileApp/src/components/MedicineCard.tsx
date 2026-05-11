import React from 'react';
import { View, Text, Image, TouchableOpacity, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { Medicine } from '../services/catalogService';
import { getImageFullUrl } from '../config/api';
import { useFavorites } from '../context/FavoritesContext';
import { useCart } from '../context/CartContext';

interface Props {
  medicine: Medicine;
  onPress?: () => void;
}

const MedicineCard: React.FC<Props> = ({ medicine, onPress }) => {
  const { isFavorite, toggleFavorite } = useFavorites();
  const { addToCart } = useCart();
  const fav = isFavorite(medicine.id);

  return (
    <TouchableOpacity
      activeOpacity={0.9}
      onPress={onPress}
      style={styles.card}
    >
      <View style={styles.imageWrapper}>
        <Image
          source={
            medicine.imageUrl
              ? { uri: getImageFullUrl(medicine.imageUrl) || undefined }
              : require('../../assets/adaptive-icon.png')
          }
          style={styles.image}
          resizeMode="cover"
        />
        <View style={styles.priceBadge}>
          <Text style={styles.priceCurrency}>ر.ي</Text>
          <Text style={styles.priceText}>
            {Number(medicine.defaultSalePrice).toLocaleString('ar-YE')}
          </Text>
        </View>
        <TouchableOpacity
          style={styles.favBtn}
          onPress={() => toggleFavorite(medicine)}
          activeOpacity={0.7}
        >
          <Ionicons
            name={fav ? 'heart' : 'heart-outline'}
            size={22}
            color={fav ? '#e84393' : '#fff'}
          />
        </TouchableOpacity>
      </View>

      <View style={styles.body}>
        <Text style={styles.nameAr} numberOfLines={1}>{medicine.name}</Text>
        {medicine.scientificName ? (
          <Text style={styles.nameEn} numberOfLines={1}>{medicine.scientificName}</Text>
        ) : null}
        {medicine.notes ? (
          <Text style={styles.desc} numberOfLines={1}>{medicine.notes}</Text>
        ) : null}

        <View style={styles.ratingRow}>
          {[1, 2, 3, 4, 5].map((star) => (
            <Ionicons key={star} name="star" size={12} color="#f1c40f" />
          ))}
          <Text style={styles.ratingText}>(4.5)</Text>
        </View>

        <View style={styles.actions}>
          <TouchableOpacity
            style={styles.cartBtn}
            onPress={() => addToCart(medicine)}
            activeOpacity={0.7}
          >
            <Ionicons name="cart" size={16} color="#fff" />
          </TouchableOpacity>
        </View>
      </View>
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  card: {
    flex: 0.48,
    backgroundColor: '#fff',
    borderRadius: 20,
    marginBottom: 16,
    overflow: 'hidden',
    elevation: 8,
    shadowColor: '#0a3d62',
    shadowOpacity: 0.12,
    shadowRadius: 16,
    shadowOffset: { width: 0, height: 4 },
    borderWidth: 1,
    borderColor: '#f0f0f0',
  },
  imageWrapper: {
    width: '100%',
    height: 140,
    backgroundColor: '#fafafa',
    position: 'relative',
  },
  image: {
    width: '100%',
    height: '100%',
  },
  priceBadge: {
    position: 'absolute',
    bottom: 8,
    left: 8,
    backgroundColor: 'rgba(46, 204, 113, 0.92)',
    flexDirection: 'row-reverse',
    alignItems: 'baseline',
    paddingHorizontal: 10,
    paddingVertical: 4,
    borderRadius: 12,
    gap: 2,
  },
  priceCurrency: {
    color: '#fff',
    fontSize: 10,
    fontWeight: 'bold',
  },
  priceText: {
    color: '#fff',
    fontSize: 15,
    fontWeight: '900',
  },
  favBtn: {
    position: 'absolute',
    top: 8,
    right: 8,
    width: 36,
    height: 36,
    borderRadius: 18,
    backgroundColor: 'rgba(0,0,0,0.25)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  body: {
    padding: 12,
    gap: 4,
  },
  nameAr: {
    fontSize: 14,
    fontWeight: '900',
    color: '#2d3436',
    textAlign: 'right',
  },
  nameEn: {
    fontSize: 11,
    color: '#636e72',
    textAlign: 'right',
    fontStyle: 'italic',
  },
  desc: {
    fontSize: 11,
    color: '#b2bec3',
    textAlign: 'right',
    lineHeight: 14,
  },
  ratingRow: {
    flexDirection: 'row-reverse',
    alignItems: 'center',
    gap: 2,
    marginTop: 4,
  },
  ratingText: {
    fontSize: 10,
    color: '#b2bec3',
    marginRight: 4,
  },
  actions: {
    flexDirection: 'row-reverse',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginTop: 8,
  },
  cartBtn: {
    backgroundColor: '#2ecc71',
    width: 36,
    height: 36,
    borderRadius: 12,
    justifyContent: 'center',
    alignItems: 'center',
    elevation: 4,
    shadowColor: '#2ecc71',
    shadowOpacity: 0.3,
    shadowRadius: 6,
  },
});

export default MedicineCard;
