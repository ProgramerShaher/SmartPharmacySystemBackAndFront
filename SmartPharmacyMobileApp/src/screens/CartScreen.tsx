import React, { useState } from 'react';
import { View, Text, FlatList, StyleSheet, TouchableOpacity, Image, Alert, ActivityIndicator } from 'react-native';
import { useCart } from '../context/CartContext';
import { OrderService } from '../services/orderService';

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
        paymentMethod: 0, // Cash
      });
      clearCart();
      Alert.alert('نجاح', 'تم إرسال طلبك بنجاح!', [
        { text: 'تتبع الطلبات', onPress: () => navigation.navigate('Orders') }
      ]);
    } catch (e: any) {
      Alert.alert('خطأ', e.message);
    } finally {
      setLoading(false);
    }
  };

  const renderItem = ({ item }: any) => (
    <View style={styles.item}>
      <View style={styles.quantityContainer}>
        <TouchableOpacity onPress={() => updateQuantity(item.id, item.quantity + 1)}>
          <Text style={styles.qtyBtn}>+</Text>
        </TouchableOpacity>
        <Text style={styles.qtyText}>{item.quantity}</Text>
        <TouchableOpacity onPress={() => updateQuantity(item.id, item.quantity - 1)}>
          <Text style={styles.qtyBtn}>-</Text>
        </TouchableOpacity>
      </View>
      <View style={styles.itemInfo}>
        <Text style={styles.itemName}>{item.name}</Text>
        <Text style={styles.itemPrice}>{item.defaultSalePrice * item.quantity} ريال</Text>
      </View>
      <TouchableOpacity onPress={() => removeFromCart(item.id)}>
        <Text style={styles.deleteBtn}>🗑️</Text>
      </TouchableOpacity>
    </View>
  );

  return (
    <View style={styles.container}>
      <FlatList
        data={cart}
        keyExtractor={(item) => item.id.toString()}
        renderItem={renderItem}
        ListEmptyComponent={
          <View style={styles.empty}>
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
            <Text style={styles.totalPrice}>{totalPrice} ريال</Text>
            <Text style={styles.totalLabel}>الإجمالي:</Text>
          </View>
          <TouchableOpacity style={styles.checkoutBtn} onPress={handleCheckout} disabled={loading}>
            {loading ? <ActivityIndicator color="#fff" /> : <Text style={styles.checkoutBtnText}>إتمام الطلب</Text>}
          </TouchableOpacity>
        </View>
      )}
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
  },
  item: {
    flexDirection: 'row',
    padding: 15,
    borderBottomWidth: 1,
    borderBottomColor: '#eee',
    alignItems: 'center',
  },
  itemInfo: {
    flex: 1,
    marginRight: 15,
    alignItems: 'flex-end',
  },
  itemName: {
    fontSize: 16,
    fontWeight: 'bold',
  },
  itemPrice: {
    color: '#2ecc71',
    fontWeight: 'bold',
  },
  quantityContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#f1f2f6',
    borderRadius: 20,
    paddingHorizontal: 10,
  },
  qtyBtn: {
    fontSize: 20,
    padding: 10,
    color: '#2ecc71',
  },
  qtyText: {
    fontSize: 16,
    marginHorizontal: 10,
  },
  deleteBtn: {
    fontSize: 20,
    marginLeft: 10,
  },
  empty: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    marginTop: 100,
  },
  emptyText: {
    fontSize: 18,
    color: '#7f8c8d',
  },
  shopBtn: {
    marginTop: 20,
    backgroundColor: '#2ecc71',
    padding: 12,
    borderRadius: 10,
  },
  shopBtnText: {
    color: '#fff',
    fontWeight: 'bold',
  },
  footer: {
    padding: 20,
    borderTopWidth: 1,
    borderTopColor: '#eee',
  },
  totalRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 15,
  },
  totalLabel: {
    fontSize: 18,
    fontWeight: 'bold',
  },
  totalPrice: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#2ecc71',
  },
  checkoutBtn: {
    backgroundColor: '#2ecc71',
    padding: 15,
    borderRadius: 10,
    alignItems: 'center',
  },
  checkoutBtnText: {
    color: '#fff',
    fontSize: 18,
    fontWeight: 'bold',
  },
});

export default CartScreen;
