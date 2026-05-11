import apiClient from './apiClient';

export interface OrderItemRequest {
  medicineId: number;
  quantity: number;
}

export interface PlaceOrderData {
  deliveryAddress: string;
  customerNotes?: string;
  paymentMethod: number; // 0 for Cash
  items: OrderItemRequest[];
}

export interface OrderItemDto {
  id: number;
  medicineId: number;
  medicineName: string;
  medicineImage?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface OrderDto {
  id: number;
  orderNumber: string;
  orderDate: string;
  totalAmount: number;
  paymentMethod: string;
  deliveryAddress: string;
  customerNotes?: string;
  status: string;
  statusCode: number;
  orderItems: OrderItemDto[];
}

export const OrderService = {
  async placeOrder(data: PlaceOrderData): Promise<OrderDto> {
    const response = await apiClient.post('/online-orders', data);
    return response.data.data;
  },

  async getMyOrders(): Promise<OrderDto[]> {
    const response = await apiClient.get('/online-orders/my-orders');
    return response.data.data;
  },

  async getOrderDetails(orderId: number): Promise<OrderDto> {
    const response = await apiClient.get(`/online-orders/my-orders/${orderId}`);
    return response.data.data;
  }
};
