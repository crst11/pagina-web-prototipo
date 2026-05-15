

import { BusinessOrderItem } from './order.models';

export interface CustomerProfile {
  
  customerId: number;
  
  fullName: string;
  
  email: string;
  
  phone: string;
  
  city: string;
  
  address: string;
  
  authProvider: string;
}

export interface RegisterCustomerRequest {
  
  fullName: string;
  
  email: string;
  
  password: string;
  
  phone: string;
  
  city: string;
  
  address: string;
}

export interface UpdateCustomerProfileRequest {
  
  fullName: string;
  
  phone: string;
  
  city: string;
  
  address: string;
}

export interface LoginCustomerRequest {
  
  email: string;
  
  password: string;
}

export interface CustomerAuthResponseApi {
  
  token: string;
  
  customer: CustomerProfile;
}

export interface CustomerOrder {
  
  orderId: number;
  
  businessId: number;
  
  businessName: string;
  
  status: string;
  
  total: number;
  
  createdAt: string;
  
  deliveryAddress: string;
  
  notes: string;
  
  paymentMethod: string;
  
  items: BusinessOrderItem[];
}

export interface CustomerOrdersHistory {
  
  orderCount: number;
  
  totalSpent: number;
  
  orders: CustomerOrder[];
}

