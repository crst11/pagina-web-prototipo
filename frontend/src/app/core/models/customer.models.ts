/**
 * Modelos del portal de clientes (compradores).
 *
 * Define los contratos de datos para el registro, inicio de sesión,
 * actualización de perfil e historial de pedidos de los clientes.
 * Consumidos por `CustomerService` y los componentes de la cuenta del cliente.
 *
 * @module customer.models
 */

import { BusinessOrderItem } from './order.models';

/**
 * Perfil del cliente autenticado.
 * Refleja el contrato `CustomerDto` del backend.
 */
export interface CustomerProfile {
  /** Identificador único del cliente en la base de datos. */
  customerId: number;
  /** Nombre completo del cliente. */
  fullName: string;
  /** Correo electrónico del cliente. */
  email: string;
  /** Teléfono de contacto del cliente. */
  phone: string;
  /** Ciudad de residencia del cliente. */
  city: string;
  /** Dirección de envío predeterminada del cliente. */
  address: string;
  /** Proveedor de autenticación (ej. "local"). */
  authProvider: string;
}

/**
 * Datos requeridos para registrar un nuevo cliente.
 */
export interface RegisterCustomerRequest {
  /** Nombre completo del cliente. */
  fullName: string;
  /** Correo electrónico único del cliente. */
  email: string;
  /** Contraseña en texto plano (hasheada en el servidor). */
  password: string;
  /** Teléfono de contacto. */
  phone: string;
  /** Ciudad de residencia. */
  city: string;
  /** Dirección de envío predeterminada. */
  address: string;
}

/**
 * Datos para actualizar el perfil de un cliente autenticado.
 * El correo electrónico no es modificable desde este formulario.
 */
export interface UpdateCustomerProfileRequest {
  /** Nuevo nombre completo del cliente. */
  fullName: string;
  /** Nuevo teléfono de contacto. */
  phone: string;
  /** Nueva ciudad de residencia. */
  city: string;
  /** Nueva dirección de envío. */
  address: string;
}

/**
 * Datos requeridos para iniciar sesión como cliente.
 */
export interface LoginCustomerRequest {
  /** Correo electrónico registrado del cliente. */
  email: string;
  /** Contraseña en texto plano. */
  password: string;
}

/**
 * Respuesta del backend al registrar o iniciar sesión como cliente.
 */
export interface CustomerAuthResponseApi {
  /** Token de sesión del cliente. Se almacena en localStorage. */
  token: string;
  /** Perfil del cliente autenticado. */
  customer: CustomerProfile;
}

/**
 * Pedido realizado por el cliente a una empresa específica.
 * Refleja el contrato `CustomerOrderDto` del backend.
 */
export interface CustomerOrder {
  /** Identificador único del pedido. */
  orderId: number;
  /** Identificador de la empresa que recibe el pedido. */
  businessId: number;
  /** Nombre comercial de la empresa receptora. */
  businessName: string;
  /** Estado actual del pedido (ej. "Pendiente", "Entregado"). */
  status: string;
  /** Total del pedido en pesos colombianos (COP). */
  total: number;
  /** Fecha y hora de creación del pedido (ISO 8601). */
  createdAt: string;
  /** Dirección de entrega del pedido. */
  deliveryAddress: string;
  /** Notas o instrucciones adicionales del cliente. */
  notes: string;
  /** Método de pago seleccionado (ej. "Contraentrega"). */
  paymentMethod: string;
  /** Lista de productos incluidos en el pedido. */
  items: BusinessOrderItem[];
}

/**
 * Historial completo de pedidos del cliente autenticado.
 * Refleja el contrato `CustomerOrdersHistoryResponse` del backend.
 */
export interface CustomerOrdersHistory {
  /** Número total de pedidos realizados por el cliente. */
  orderCount: number;
  /** Suma total gastada por el cliente en todos sus pedidos (COP). */
  totalSpent: number;
  /** Lista de pedidos ordenados del más reciente al más antiguo. */
  orders: CustomerOrder[];
}

