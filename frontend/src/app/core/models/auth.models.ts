// Modelos de autenticacion y gestion de perfil para empresarios y clientes.

export interface AuthResponseApi {
  token: string;
  business: import('./marketplace.models').MarketplaceBusiness;
}

export interface LoginBusinessRequest {
  email: string;
  password: string;
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
