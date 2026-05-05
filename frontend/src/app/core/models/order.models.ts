// Modelos de pedidos: items de linea, pedidos de empresa y respuestas de creacion.

export interface OrderCreatedResponse {
  orderId: number;
  total: number;
  message: string;
}

export interface BusinessOrderItem {
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface BusinessOrder {
  orderId: number;
  customerFullName: string;
  customerEmail: string;
  customerPhone: string;
  customerCity: string;
  deliveryAddress: string;
  notes: string;
  status: string;
  total: number;
  createdAt: string;
  isNew: boolean;
  paymentMethod: string;
  items: BusinessOrderItem[];
}

export interface BusinessOrdersFeed {
  newOrders: number;
  orders: BusinessOrder[];
}
