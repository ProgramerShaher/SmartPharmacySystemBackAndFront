import React, { useEffect, useState } from 'react';
import { View, Text, FlatList, StyleSheet, ActivityIndicator, RefreshControl } from 'react-native';
import { OrderService, OrderDto } from '../services/orderService';

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

  const getStatusColor = (code: number) => {
    switch (code) {
      case 1: return '#f39c12'; // Pending
      case 2: return '#3498db'; // Preparing
      case 3: return '#9b59b6'; // OutForDelivery
      case 4: return '#2ecc71'; // Delivered
      case 5:
      case 6: return '#e74c3c'; // Cancelled/Rejected
      default: return '#7f8c8d';
    }
  };

  const renderOrder = ({ item }: { item: OrderDto }) => (
    <View style={styles.card}>
      <View style={styles.cardHeader}>
        <Text style={styles.orderNumber}>#{item.orderNumber}</Text>
        <View style={[styles.statusBadge, { backgroundColor: getStatusColor(item.statusCode) }]}>
          <Text style={styles.statusText}>{item.status}</Text>
        </View>
      </View>
      <View style={styles.cardBody}>
        <Text style={styles.date}>{new Date(item.orderDate).toLocaleDateString('ar-YE')}</Text>
        <Text style={styles.amount}>الإجمالي: {item.totalAmount} ريال</Text>
      </View>
      <View style={styles.itemsList}>
        {item.orderItems.map((oi, idx) => (
          <Text key={idx} style={styles.itemLine}>- {oi.medicineName} (x{oi.quantity})</Text>
        ))}
      </View>
    </View>
  );

  if (loading) return <ActivityIndicator size="large" color="#2ecc71" style={{ flex: 1 }} />;

  return (
    <View style={styles.container}>
      <FlatList
        data={orders}
        keyExtractor={(item) => item.id.toString()}
        renderItem={renderOrder}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
        ListEmptyComponent={
          <View style={styles.empty}>
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
    backgroundColor: '#f8f9fa',
    padding: 10,
  },
  card: {
    backgroundColor: '#fff',
    borderRadius: 15,
    padding: 15,
    marginBottom: 15,
    elevation: 2,
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 10,
  },
  orderNumber: {
    fontSize: 16,
    fontWeight: 'bold',
  },
  statusBadge: {
    paddingHorizontal: 12,
    paddingVertical: 4,
    borderRadius: 12,
  },
  statusText: {
    color: '#fff',
    fontSize: 12,
    fontWeight: 'bold',
  },
  cardBody: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 10,
  },
  date: {
    color: '#7f8c8d',
  },
  amount: {
    fontWeight: 'bold',
    color: '#2ecc71',
  },
  itemsList: {
    borderTopWidth: 1,
    borderTopColor: '#eee',
    paddingTop: 10,
  },
  itemLine: {
    fontSize: 13,
    color: '#34495e',
    textAlign: 'right',
  },
  empty: {
    alignItems: 'center',
    marginTop: 100,
  },
  emptyText: {
    fontSize: 18,
    color: '#7f8c8d',
  },
});

export default OrdersScreen;
