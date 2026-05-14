/**
 * Modelos de pedidos del marketplace.
 *
 * Define las estructuras para las líneas de pedido, los pedidos completos
 * de una empresa y las respuestas del backend al crear pedidos.
 * Usados tanto por el portal de clientes como por el panel empresarial.
 *
 * @module order.models
 */

/**
 * Respuesta del backend al crear un pedido individual.
 */
export interface OrderCreatedResponse {
  /** Identificador del pedido recién creado. */
  orderId: number;
  /** Total del pedido en pesos colombianos (COP). */
  total: number;
  /** Mensaje de confirmación del servidor. */
  message: string;
}

/**
 * Línea de producto dentro de un pedido.
 * Refleja el contrato `BusinessOrderItemDto` del backend.
 */
export interface BusinessOrderItem {
  /** Identificador del producto. */
  productId: number;
  /** Nombre del producto al momento de la compra. */
  productName: string;
  /** Cantidad de unidades pedidas. */
  quantity: number;
  /** Precio unitario al momento de la compra (COP). */
  unitPrice: number;
  /** Total de la línea: `quantity × unitPrice` (COP). */
  lineTotal: number;
}

/**
 * Pedido recibido por una empresa, con todos los datos del cliente y del envío.
 * Refleja el contrato `BusinessOrderDto` del backend.
 */
export interface BusinessOrder {
  /** Identificador único del pedido. */
  orderId: number;
  /** Nombre completo del cliente que realizó el pedido. */
  customerFullName: string;
  /** Correo electrónico del cliente. */
  customerEmail: string;
  /** Teléfono de contacto del cliente. */
  customerPhone: string;
  /** Ciudad de entrega del pedido. */
  customerCity: string;
  /** Dirección de entrega del pedido. */
  deliveryAddress: string;
  /** Notas adicionales del cliente. */
  notes: string;
  /** Estado actual del pedido (ej. "Pendiente", "En camino"). */
  status: string;
  /** Total del pedido en COP. */
  total: number;
  /** Fecha y hora de creación (ISO 8601). */
  createdAt: string;
  /** Indica si el pedido aún no ha sido visto por el empresario. */
  isNew: boolean;
  /** Método de pago seleccionado por el cliente. */
  paymentMethod: string;
  /** Lista de productos incluidos en el pedido. */
  items: BusinessOrderItem[];
}

/**
 * Feed de pedidos del panel empresarial.
 * Refleja el contrato `BusinessOrdersFeedResponse` del backend.
 */
export interface BusinessOrdersFeed {
  /** Número de pedidos nuevos (no vistos) por el empresario. */
  newOrders: number;
  /** Lista completa de pedidos de la empresa. */
  orders: BusinessOrder[];
}

