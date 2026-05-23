import React, { useState } from 'react';
import {
  View,
  Text,
  FlatList,
  StyleSheet,
  TouchableOpacity,
  Image,
  Alert,
  ActivityIndicator,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useCart } from '../context/CartContext';
import { OrderService } from '../services/orderService';
import { getImageFullUrl } from '../config/api';

const CartScreen = ({ navigation }: any) => {
  const { cart, updateQuantity, removeFromCart, totalPrice, clearCart } = useCart();
  const [loading, setLoading] = useState(false);

  const handleCheckout = async () => {
    if (cart.length === 0) return;
    setLoading(true);
    try {
      await OrderService.placeOrder({
        deliveryAddress: 'عنوان افتراضي (يتم تحديده لاحقاً)',
        items: cart.map(item => ({ medicineId: item.id, quantity: item.quantity })),
        paymentMethod: 0,
      });
      clearCart();
      Alert.alert('نجاح', 'تم إرسال طلبك بنجاح!', [
        { text: 'تتبع الطلبات', onPress: () => navigation.navigate('Orders') },
      ]);
    } catch (e: any) {
      Alert.alert('خطأ', e.message);
    } finally {
      setLoading(false);
    }
  };

  const renderItem = ({ item }: any) => (
    <View style={styles.item}>
      <Image
        source={
          item.imageUrl
            ? { uri: getImageFullUrl(item.imageUrl) || undefined }
            : require('../../assets/adaptive-icon.png')
        }
        style={styles.itemImage}
        resizeMode="cover"
      />
      <View style={styles.itemInfo}>
        <Text style={styles.itemName} numberOfLines={1}>{item.name}</Text>
        {item.scientificName ? (
          <Text style={styles.itemNameEn} numberOfLines={1}>{item.scientificName}</Text>
        ) : null}
        <Text style={styles.itemPrice}>
          {Number(item.defaultSalePrice).toLocaleString('ar-YE')} ريال
        </Text>
      </View>
      <View style={styles.rightCol}>
        <TouchableOpacity onPress={() => removeFromCart(item.id)} style={styles.deleteBtn}>
          <Ionicons name="trash-outline" size={18} color="#e74c3c" />
        </TouchableOpacity>
        <View style={styles.quantityRow}>
          <TouchableOpacity onPress={() => updateQuantity(item.id, item.quantity + 1)}>
            <Ionicons name="add" size={16} color="#0a3d62" />
          </TouchableOpacity>
          <Text style={styles.qtyText}>{item.quantity}</Text>
          <TouchableOpacity onPress={() => updateQuantity(item.id, item.quantity - 1)}>
            <Ionicons name="remove" size={16} color="#0a3d62" />
          </TouchableOpacity>
        </View>
      </View>
    </View>
  );

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <View style={styles.headerTop}>
          <Text style={styles.headerTitle}>سلة التسوق</Text>
          <View style={styles.cartIconWrap}>
            <Ionicons name="cart" size={22} color="#fff" />
            {cart.length > 0 && (
              <View style={styles.badge}>
                <Text style={styles.badgeText}>{cart.length}</Text>
              </View>
            )}
          </View>
        </View>
      </View>

      <FlatList
        data={cart}
        keyExtractor={(item) => item.id.toString()}
        renderItem={renderItem}
        contentContainerStyle={styles.listContent}
        showsVerticalScrollIndicator={false}
        ListEmptyComponent={
          <View style={styles.empty}>
            <Ionicons name="basket-outline" size={80} color="#dfe6e9" />
            <Text style={styles.emptyText}>السلة فارغة</Text>
            <TouchableOpacity style={styles.shopBtn} onPress={() => navigation.navigate('Home')}>
              <Text style={styles.shopBtnText}>تسوق الآن</Text>
            </TouchableOpacity>
          </View>
        }
      />

      {cart.length > 0 && (
        <View style={styles.footer}>
          <View style={styles.totalRow}>
            <Text style={styles.totalLabel}>الإجمالي</Text>
            <View style={styles.totalPriceRow}>
              <Text style={styles.totalPrice}>{Number(totalPrice).toLocaleString('ar-YE')}</Text>
              <Text style={styles.totalCurrency}>ريال</Text>
            </View>
          </View>
          <TouchableOpacity style={styles.checkoutBtn} onPress={handleCheckout} disabled={loading}>
            {loading ? (
              <ActivityIndicator color="#fff" />
            ) : (
              <>
                <Text style={styles.checkoutBtnText}>إتمام الطلب</Text>
                <Ionicons name="arrow-back" size={20} color="#fff" />
              </>
            )}
          </TouchableOpacity>
        </View>
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
  },
  headerTitle: {
    color: '#fff',
    fontSize: 22,
    fontWeight: '900',
  },
  cartIconWrap: {
    position: 'relative',
  },
  badge: {
    position: 'absolute',
    top: -8,
    left: -8,
    backgroundColor: '#e74c3c',
    width: 20,
    height: 20,
    borderRadius: 10,
    justifyContent: 'center',
    alignItems: 'center',
  },
  badgeText: {
    color: '#fff',
    fontSize: 11,
    fontWeight: 'bold',
  },
  listContent: {
    padding: 16,
    paddingBottom: 120,
  },
  item: {
    flexDirection: 'row-reverse',
    backgroundColor: '#fff',
    borderRadius: 20,
    padding: 12,
    marginBottom: 12,
    alignItems: 'center',
    elevation: 4,
    shadowColor: '#000',
    shadowOpacity: 0.06,
    shadowRadius: 10,
    shadowOffset: { width: 0, height: 2 },
    borderWidth: 1,
    borderColor: '#f0f0f0',
  },
  itemImage: {
    width: 64,
    height: 64,
    borderRadius: 16,
    backgroundColor: '#fafafa',
  },
  itemInfo: {
    flex: 1,
    marginHorizontal: 12,
    alignItems: 'flex-end',
  },
  itemName: {
    fontSize: 15,
    fontWeight: '900',
    color: '#2d3436',
    textAlign: 'right',
  },
  itemNameEn: {
    fontSize: 11,
    color: '#636e72',
    textAlign: 'right',
    fontStyle: 'italic',
    marginTop: 2,
  },
  itemPrice: {
    fontSize: 13,
    fontWeight: '800',
    color: '#2ecc71',
    marginTop: 4,
  },
  rightCol: {
    alignItems: 'center',
    gap: 8,
  },
  deleteBtn: {
    width: 32,
    height: 32,
    borderRadius: 10,
    backgroundColor: '#fef0f0',
    justifyContent: 'center',
    alignItems: 'center',
  },
  quantityRow: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#f1f2f6',
    borderRadius: 12,
    paddingHorizontal: 6,
    gap: 6,
  },
  qtyText: {
    fontSize: 14,
    fontWeight: '900',
    color: '#2d3436',
    minWidth: 20,
    textAlign: 'center',
  },
  empty: {
    alignItems: 'center',
    marginTop: 80,
  },
  emptyText: {
    fontSize: 18,
    color: '#bdc3c7',
    fontWeight: '700',
    marginTop: 16,
  },
  shopBtn: {
    marginTop: 20,
    backgroundColor: '#2ecc71',
    paddingHorizontal: 30,
    paddingVertical: 14,
    borderRadius: 16,
    elevation: 6,
    shadowColor: '#2ecc71',
    shadowOpacity: 0.3,
    shadowRadius: 10,
  },
  shopBtnText: {
    color: '#fff',
    fontWeight: '900',
    fontSize: 15,
  },
  footer: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    backgroundColor: '#fff',
    paddingHorizontal: 20,
    paddingTop: 16,
    paddingBottom: 30,
    borderTopLeftRadius: 30,
    borderTopRightRadius: 30,
    elevation: 12,
    shadowColor: '#000',
    shadowOpacity: 0.15,
    shadowRadius: 20,
    shadowOffset: { width: 0, height: -4 },
  },
  totalRow: {
    flexDirection: 'row-reverse',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 16,
  },
  totalLabel: {
    fontSize: 16,
    fontWeight: '700',
    color: '#636e72',
  },
  totalPriceRow: {
    flexDirection: 'row-reverse',
    alignItems: 'baseline',
    gap: 4,
  },
  totalPrice: {
    fontSize: 24,
    fontWeight: '900',
    color: '#2ecc71',
  },
  totalCurrency: {
    fontSize: 14,
    fontWeight: '600',
    color: '#2ecc71',
  },
  checkoutBtn: {
    flexDirection: 'row-reverse',
    backgroundColor: '#2ecc71',
    height: 52,
    borderRadius: 16,
    justifyContent: 'center',
    alignItems: 'center',
    gap: 8,
    elevation: 6,
    shadowColor: '#2ecc71',
    shadowOpacity: 0.35,
    shadowRadius: 10,
  },
  checkoutBtnText: {
    color: '#fff',
    fontSize: 17,
    fontWeight: '900',
  },
});

export default CartScreen;
