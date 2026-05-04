export interface BusinessProduct {
  productId: number;
  businessId: number;
  name: string;
  category: string;
  description: string;
  price: number;
  minimumOrder: number;
  stock: number;
  imageUrl: string;
  isFeatured: boolean;
  isPublished: boolean;
}

export interface MarketplaceBusiness {
  businessId: number;
  slug: string;
  ownerName: string;
  businessName: string;
  email: string;
  phone: string;
  city: string;
  address: string;
  tagline: string;
  description: string;
  shippingLeadTime: string;
  minimumOrderAmount: number;
  logoUrl: string;
  bannerUrl: string;
  websiteUrl: string;
  products: BusinessProduct[];
}

export interface FeaturedProduct extends BusinessProduct {
  businessName: string;
  businessSlug: string;
  businessTagline: string;
  city: string;
}

export interface MarketplaceSnapshot {
  businesses: MarketplaceBusiness[];
  featuredProducts: FeaturedProduct[];
  categories: string[];
  totalBusinesses: number;
  totalProducts: number;
}

export interface RegisterBusinessRequest {
  ownerName: string;
  businessName: string;
  email: string;
  password: string;
  phone: string;
  city: string;
  address: string;
  tagline: string;
  description: string;
  shippingLeadTime: string;
  minimumOrderAmount: number;
  websiteUrl: string;
}

export interface LoginBusinessRequest {
  email: string;
  password: string;
}

export interface UpdateBusinessProfileRequest {
  ownerName: string;
  businessName: string;
  email: string;
  phone: string;
  city: string;
  address: string;
  tagline: string;
  description: string;
  shippingLeadTime: string;
  minimumOrderAmount: number;
  logoUrl: string;
  bannerUrl: string;
  websiteUrl: string;
}

export interface UpsertBusinessProductRequest {
  name: string;
  category: string;
  description: string;
  price: number;
  minimumOrder: number;
  stock: number;
  imageUrl: string;
  isFeatured: boolean;
  isPublished: boolean;
}

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
  items: Array<{
    productId: number;
    quantity: number;
  }>;
}

export interface CheckoutCartRequest extends ShipmentCustomer {
  items: Array<{
    businessId: number;
    productId: number;
    quantity: number;
  }>;
}

export interface OrderCreatedResponse {
  orderId: number;
  total: number;
  message: string;
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
  items: BusinessOrderItem[];
}

export interface BusinessOrdersFeed {
  newOrders: number;
  orders: BusinessOrder[];
}

export interface AuthResponseApi {
  token: string;
  business: MarketplaceBusiness;
}
