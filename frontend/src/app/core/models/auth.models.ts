/**
 * Modelos de autenticación y gestión de perfil empresarial.
 *
 * Define los contratos de entrada y salida para el registro, inicio de sesión,
 * actualización de perfil y gestión de productos de los empresarios (owners).
 * Consumidos por `AuthService` y los componentes del portal empresarial.
 *
 * @module auth.models
 */

/**
 * Respuesta del backend al registrar o iniciar sesión como empresario.
 * Contiene el token de sesión y el perfil completo de la empresa.
 */
export interface AuthResponseApi {
  /** Token de sesión generado por el backend. Se almacena en localStorage. */
  token: string;
  /** Perfil completo de la empresa autenticada. */
  business: import('./marketplace.models').MarketplaceBusiness;
}

/**
 * Datos requeridos para iniciar sesión como empresario.
 */
export interface LoginBusinessRequest {
  /** Correo electrónico registrado de la empresa. */
  email: string;
  /** Contraseña en texto plano (se envía por HTTPS). */
  password: string;
}

/**
 * Datos requeridos para registrar una nueva empresa en la plataforma.
 */
export interface RegisterBusinessRequest {
  /** Nombre completo del propietario de la empresa. */
  ownerName: string;
  /** Nombre comercial de la empresa. */
  businessName: string;
  /** Correo electrónico único de la empresa. */
  email: string;
  /** Contraseña en texto plano (se almacena hasheada en el servidor). */
  password: string;
  /** Teléfono de contacto de la empresa. */
  phone: string;
  /** Ciudad donde opera la empresa. */
  city: string;
  /** Dirección física de la empresa. */
  address: string;
  /** Eslogan corto de la empresa. */
  tagline: string;
  /** Descripción detallada de la empresa y sus servicios. */
  description: string;
  /** Tiempo estimado de entrega (ej. "24 horas en Bogotá"). */
  shippingLeadTime: string;
  /** Pedido mínimo en pesos colombianos (COP). */
  minimumOrderAmount: number;
  /** URL del sitio web externo de la empresa. */
  websiteUrl: string;
}

/**
 * Datos para actualizar el perfil de una empresa ya autenticada.
 */
export interface UpdateBusinessProfileRequest {
  /** Nombre del propietario. */
  ownerName: string;
  /** Nombre comercial de la empresa. */
  businessName: string;
  /** Nuevo correo electrónico (debe ser único). */
  email: string;
  /** Nuevo teléfono de contacto. */
  phone: string;
  /** Ciudad de operación. */
  city: string;
  /** Dirección física. */
  address: string;
  /** Eslogan actualizado. */
  tagline: string;
  /** Descripción actualizada. */
  description: string;
  /** Tiempo de entrega actualizado. */
  shippingLeadTime: string;
  /** Pedido mínimo actualizado en COP. */
  minimumOrderAmount: number;
  /** URL del logo (relativa o externa). */
  logoUrl: string;
  /** URL del banner (relativa o externa). */
  bannerUrl: string;
  /** URL del sitio web externo. */
  websiteUrl: string;
}

/**
 * Datos para crear o actualizar un producto en el catálogo de la empresa.
 */
export interface UpsertBusinessProductRequest {
  /** Nombre del producto. */
  name: string;
  /** Categoría del producto. */
  category: string;
  /** Descripción del producto. */
  description: string;
  /** Precio unitario en COP. */
  price: number;
  /** Pedido mínimo de unidades. */
  minimumOrder: number;
  /** Unidades en inventario. */
  stock: number;
  /** URL de la imagen del producto. */
  imageUrl: string;
  /** Si el producto aparece en la sección de destacados. */
  isFeatured: boolean;
  /** Si el producto está visible para los compradores. */
  isPublished: boolean;
}

