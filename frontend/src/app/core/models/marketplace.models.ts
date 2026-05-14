/**
 * Modelos del marketplace público (Vitrina Empresarial).
 *
 * Define las estructuras de datos que representan los productos, las empresas
 * y el snapshot completo del marketplace. Estos modelos son retornados por el
 * backend a través del endpoint `/api/store/overview` y consumidos por los
 * componentes del frontend.
 *
 * @module marketplace.models
 */

/**
 * Producto ofrecido por una empresa dentro del marketplace.
 * Refleja exactamente el contrato `ProductDto` del backend.
 */
export interface BusinessProduct {
  /** Identificador único del producto en la base de datos. */
  productId: number;
  /** Identificador de la empresa propietaria del producto. */
  businessId: number;
  /** Nombre descriptivo del producto. */
  name: string;
  /** Categoría a la que pertenece el producto (ej. "Empaques corporativos"). */
  category: string;
  /** Descripción detallada del producto. */
  description: string;
  /** Precio unitario en pesos colombianos (COP). */
  price: number;
  /** Cantidad mínima que el cliente debe pedir. */
  minimumOrder: number;
  /** Unidades disponibles en inventario. */
  stock: number;
  /** URL relativa de la imagen principal del producto. */
  imageUrl: string;
  /** Indica si el producto aparece destacado en la página de inicio. */
  isFeatured: boolean;
  /** Indica si el producto está visible para los compradores. */
  isPublished: boolean;
}

/**
 * Empresa registrada en el marketplace con su catálogo de productos.
 * Refleja el contrato `MarketplaceBusinessDto` del backend.
 */
export interface MarketplaceBusiness {
  /** Identificador único de la empresa en la base de datos. */
  businessId: number;
  /** Slug URL-friendly generado a partir del nombre (ej. "aura-cafe-ejecutivo"). */
  slug: string;
  /** Nombre completo del propietario registrado. */
  ownerName: string;
  /** Nombre comercial de la empresa. */
  businessName: string;
  /** Correo electrónico de contacto de la empresa. */
  email: string;
  /** Teléfono de contacto de la empresa. */
  phone: string;
  /** Ciudad donde opera la empresa. */
  city: string;
  /** Dirección física de la empresa. */
  address: string;
  /** Eslogan corto que describe la propuesta de valor de la empresa. */
  tagline: string;
  /** Descripción larga de la empresa y sus servicios. */
  description: string;
  /** Tiempo estimado de entrega (ej. "24 horas en Bogotá"). */
  shippingLeadTime: string;
  /** Monto mínimo de pedido en pesos colombianos (COP). */
  minimumOrderAmount: number;
  /** URL relativa del logo de la empresa. */
  logoUrl: string;
  /** URL relativa del banner de portada de la empresa. */
  bannerUrl: string;
  /** Sitio web externo de la empresa. */
  websiteUrl: string;
  /** Lista de productos publicados pertenecientes a esta empresa. */
  products: BusinessProduct[];
}

/**
 * Producto destacado enriquecido con información de la empresa propietaria.
 * Extiende `BusinessProduct` para mostrar contexto de la empresa en la lista
 * de destacados de la página de inicio.
 */
export interface FeaturedProduct extends BusinessProduct {
  /** Nombre comercial de la empresa que ofrece el producto. */
  businessName: string;
  /** Slug de la empresa propietaria (para navegar al perfil). */
  businessSlug: string;
  /** Eslogan de la empresa propietaria. */
  businessTagline: string;
  /** Ciudad de la empresa propietaria. */
  city: string;
}

/**
 * Snapshot completo del marketplace cargado al iniciar la aplicación.
 * Este objeto agrupa toda la información necesaria para renderizar
 * la vitrina sin llamadas adicionales al backend.
 */
export interface MarketplaceSnapshot {
  /** Lista completa de empresas activas con sus productos. */
  businesses: MarketplaceBusiness[];
  /** Lista de productos marcados como destacados. */
  featuredProducts: FeaturedProduct[];
  /** Categorías únicas disponibles entre todos los productos. */
  categories: string[];
  /** Número total de empresas activas en el marketplace. */
  totalBusinesses: number;
  /** Número total de productos publicados en el marketplace. */
  totalProducts: number;
}

