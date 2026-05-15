

import { BusinessProduct } from './marketplace.models';

export interface CartItem extends BusinessProduct {
  
  quantity: number;
  
  businessName: string;
}

export interface CartBusinessGroup {
  
  businessId: number;
  
  businessName: string;
  
  businessSlug: string;
  
  bannerUrl: string;
  
  minimumOrderAmount: number;
  
  city: string;
  
  address: string;
  
  tagline: string;
  
  phone: string;
  
  email: string;
  
  items: CartItem[];
  
  subtotal: number;
}

export interface ShipmentCustomer {
  
  fullName: string;
  
  email: string;
  
  phone: string;
  
  city: string;
  
  address: string;
  
  notes: string;
}

export interface CreateShipmentRequest extends ShipmentCustomer {
  
  businessId: number;
  
  items: Array<{ productId: number; quantity: number }>;
  
  paymentMethod: string;
}

export interface CheckoutCartRequest extends ShipmentCustomer {
  
  items: Array<{ businessId: number; productId: number; quantity: number }>;
  
  paymentMethod: string;
}

export interface BusinessCheckoutResult {
  
  orderId: number;
  
  businessId: number;
  
  businessName: string;
  
  total: number;
}

export interface CheckoutCartResponse {
  
  orderCount: number;
  
  total: number;
  
  message: string;
  
  orders: BusinessCheckoutResult[];
}

