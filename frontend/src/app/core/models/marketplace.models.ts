

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

