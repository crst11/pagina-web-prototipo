/**
 * Modelos del carrito de compras.
 *
 * Define las estructuras de datos para los ítems del carrito, la agrupación
 * por empresa, los datos de envío del cliente y los contratos de
 * solicitud/respuesta del proceso de compra (checkout).
 *
 * El carrito agrupa los productos por empresa porque cada empresa genera
 * un pedido independiente al completar la compra.
 *
 * @module cart.models
 */

import { BusinessProduct } from './marketplace.models';

/**
 * Producto agregado al carrito de compras.
 * Extiende `BusinessProduct` con la cantidad seleccionada y el nombre de la empresa.
 */
export interface CartItem extends BusinessProduct {
  /** Cantidad de unidades que el cliente desea comprar. */
  quantity: number;
  /** Nombre de la empresa que ofrece el producto. */
  businessName: string;
}

/**
 * Agrupación de ítems del carrito pertenecientes a la misma empresa.
 * Cada empresa genera un pedido independiente en el checkout.
 */
export interface CartBusinessGroup {
  /** Identificador de la empresa. */
  businessId: number;
  /** Nombre comercial de la empresa. */
  businessName: string;
  /** Slug URL-friendly de la empresa. */
  businessSlug: string;
  /** URL del banner de la empresa (mostrado en el carrito). */
  bannerUrl: string;
  /** Monto mínimo de pedido requerido por la empresa (COP). */
  minimumOrderAmount: number;
  /** Ciudad de la empresa. */
  city: string;
  /** Dirección de la empresa. */
  address: string;
  /** Eslogan de la empresa. */
  tagline: string;
  /** Teléfono de la empresa. */
  phone: string;
  /** Correo electrónico de la empresa. */
  email: string;
  /** Lista de productos de esta empresa en el carrito. */
  items: CartItem[];
  /** Subtotal de esta agrupación en COP (suma de `precio × cantidad`). */
  subtotal: number;
}

/**
 * Datos del cliente para el envío del pedido.
 * Estos campos se pre-rellenan con el perfil del cliente autenticado.
 */
export interface ShipmentCustomer {
  /** Nombre completo del destinatario. */
  fullName: string;
  /** Correo electrónico de contacto. */
  email: string;
  /** Teléfono de contacto. */
  phone: string;
  /** Ciudad de entrega. */
  city: string;
  /** Dirección de entrega. */
  address: string;
  /** Instrucciones adicionales para el envío. */
  notes: string;
}

/**
 * Solicitud para crear un pedido individual a una empresa específica.
 * Usada por el endpoint `POST /api/orders`.
 */
export interface CreateShipmentRequest extends ShipmentCustomer {
  /** Identificador de la empresa a la que va dirigido el pedido. */
  businessId: number;
  /** Líneas de productos del pedido (id de producto + cantidad). */
  items: Array<{ productId: number; quantity: number }>;
  /** Método de pago seleccionado (ej. "Contraentrega"). */
  paymentMethod: string;
}

/**
 * Solicitud para procesar el carrito completo en una sola llamada.
 * Usada por el endpoint `POST /api/orders/checkout`.
 * El backend genera un pedido separado por cada empresa involucrada.
 */
export interface CheckoutCartRequest extends ShipmentCustomer {
  /** Líneas de productos con empresa, producto y cantidad. */
  items: Array<{ businessId: number; productId: number; quantity: number }>;
  /** Método de pago seleccionado. */
  paymentMethod: string;
}

/**
 * Resultado del pedido generado para una empresa durante el checkout masivo.
 */
export interface BusinessCheckoutResult {
  /** Identificador del pedido creado para esta empresa. */
  orderId: number;
  /** Identificador de la empresa. */
  businessId: number;
  /** Nombre de la empresa. */
  businessName: string;
  /** Total del pedido para esta empresa (COP). */
  total: number;
}

/**
 * Respuesta del backend al completar el checkout del carrito completo.
 * Refleja el contrato `CheckoutCartResponse` del backend.
 */
export interface CheckoutCartResponse {
  /** Número de pedidos generados (uno por empresa). */
  orderCount: number;
  /** Suma total de todos los pedidos generados (COP). */
  total: number;
  /** Mensaje de confirmación del servidor. */
  message: string;
  /** Lista de pedidos generados, uno por empresa. */
  orders: BusinessCheckoutResult[];
}

