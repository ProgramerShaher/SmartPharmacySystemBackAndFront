import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  FlatList,
  StyleSheet,
  ActivityIndicator,
  RefreshControl,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { OrderService, OrderDto } from '../services/orderService';

const STATUS_CONFIG: Record<number, { label: string; icon: keyof typeof Ionicons.glyphMap; color: string }> = {
  1: { label: 'قيد الانتظار', icon: 'time-outline', color: '#f39c12' },
  2: { label: 'قيد التحضير', icon: 'flask-outline', color: '#3498db' },
  3: { label: 'في الطريق', icon: 'bicycle-outline', color: '#9b59b6' },
  4: { label: 'تم التوصيل', icon: 'checkmark-circle-outline', color: '#2ecc71' },
  5: { label: 'ملغي', icon: 'close-circle-outline', color: '#e74c3c' },
  6: { label: 'مرفوض', icon: 'close-circle-outline', color: '#e74c3c' },
};

const getStatusConfig = (code: number) =>
  STATUS_CONFIG[code] || { label: 'غير معروف', icon: 'help-outline' as keyof typeof Ionicons.glyphMap, color: '#7f8c8d' };

const OrdersScreen = () => {
  const [orders, setOrders] = useState<OrderDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const loadOrders = async () => {
    try {
      const data = await OrderService.getMyOrders();
      setOrders(data);
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  useEffect(() => {
    loadOrders();
  }, []);

  const onRefresh = () => {
    setRefreshing(true);
    loadOrders();
  };

  const renderOrder = ({ item }: { item: OrderDto }) => {
    const statusCfg = getStatusConfig(item.statusCode);

    return (
      <View style={styles.card}>
        <View style={styles.cardHeader}>
          <Text style={styles.orderNumber}>طلب #{item.orderNumber}</Text>
          <View style={[styles.statusBadge, { backgroundColor: statusCfg.color }]}>
            <Ionicons name={statusCfg.icon} size={14} color="#fff" />
            <Text style={styles.statusText}>{statusCfg.label}</Text>
          </View>
        </View>

        <View style={styles.cardBody}>
          <View style={styles.dateRow}>
            <Ionicons name="calendar-outline" size={14} color="#b2bec3" />
            <Text style={styles.date}>
              {new Date(item.orderDate).toLocaleDateString('ar-YE')}
            </Text>
          </View>
          <View style={styles.amountRow}>
            <Text style={styles.amount}>
              {Number(item.totalAmount).toLocaleString('ar-YE')}
            </Text>
            <Text style={styles.amountCurrency}>ريال</Text>
          </View>
        </View>

        <View style={styles.itemsSection}>
          <Text style={styles.itemsTitle}>المنتجات</Text>
          {item.orderItems.map((oi, idx) => (
            <View key={idx} style={styles.itemRow}>
              <Text style={styles.itemName}>{oi.medicineName}</Text>
              <Text style={styles.itemQty}>x{oi.quantity}</Text>
            </View>
          ))}
        </View>
      </View>
    );
  };

  if (loading) {
    return (
      <View style={styles.loader}>
        <ActivityIndicator size="large" color="#0a3d62" />
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <View style={styles.headerTop}>
          <Text style={styles.headerTitle}>طلباتي</Text>
          <Ionicons name="receipt-outline" size={24} color="#fff" />
        </View>
      </View>

      <FlatList
        data={orders}
        keyExtractor={(item) => item.id.toString()}
        renderItem={renderOrder}
        contentContainerStyle={styles.listContent}
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
          <View style={styles.empty}>
            <Ionicons name="document-text-outline" size={80} color="#dfe6e9" />
            <Text style={styles.emptyText}>لا يوجد لديك طلبات حالياً</Text>
          </View>
        }
      />
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
  listContent: {
    padding: 16,
    paddingBottom: 30,
  },
  card: {
    backgroundColor: '#fff',
    borderRadius: 20,
    padding: 16,
    marginBottom: 14,
    elevation: 4,
    shadowColor: '#000',
    shadowOpacity: 0.06,
    shadowRadius: 10,
    shadowOffset: { width: 0, height: 2 },
    borderWidth: 1,
    borderColor: '#f0f0f0',
  },
  cardHeader: {
    flexDirection: 'row-reverse',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 12,
  },
  orderNumber: {
    fontSize: 16,
    fontWeight: '900',
    color: '#2d3436',
  },
  statusBadge: {
    flexDirection: 'row-reverse',
    alignItems: 'center',
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 20,
    gap: 4,
  },
  statusText: {
    color: '#fff',
    fontSize: 12,
    fontWeight: 'bold',
  },
  cardBody: {
    flexDirection: 'row-reverse',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 12,
  },
  dateRow: {
    flexDirection: 'row-reverse',
    alignItems: 'center',
    gap: 4,
  },
  date: {
    color: '#b2bec3',
    fontSize: 13,
    fontWeight: '600',
  },
  amountRow: {
    flexDirection: 'row-reverse',
    alignItems: 'baseline',
    gap: 3,
  },
  amount: {
    fontSize: 18,
    fontWeight: '900',
    color: '#2ecc71',
  },
  amountCurrency: {
    fontSize: 12,
    fontWeight: '600',
    color: '#2ecc71',
  },
  itemsSection: {
    borderTopWidth: 1,
    borderTopColor: '#f0f0f0',
    paddingTop: 12,
  },
  itemsTitle: {
    fontSize: 13,
    fontWeight: '800',
    color: '#636e72',
    textAlign: 'right',
    marginBottom: 8,
  },
  itemRow: {
    flexDirection: 'row-reverse',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: 4,
  },
  itemName: {
    fontSize: 13,
    color: '#2d3436',
    fontWeight: '600',
    textAlign: 'right',
    flex: 1,
  },
  itemQty: {
    fontSize: 13,
    color: '#b2bec3',
    fontWeight: '700',
    marginRight: 12,
  },
  empty: {
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

export default OrdersScreen;
