/*
 * cart.service.ts
 *
 * Gestiona el estado global del carrito de compras.
 *
 * Comportamiento por sesion:
 *
 *   - SIN sesion de cliente (invitado):
 *       Los productos se guardan en sessionStorage.
 *       sessionStorage se borra automaticamente cuando el usuario cierra la
 *       pestana o recarga la pagina sin una sesion activa. Esto impide que el
 *       carrito de un invitado quede visible en otra cuenta.
 *
 *   - CON sesion de cliente:
 *       Los productos se guardan en localStorage con una clave que incluye
 *       el token de la sesion activa. Cada cuenta tiene su propio carrito
 *       independiente y persiste entre recargas mientras la sesion este activa.
 *
 *   - Al INICIAR sesion:
 *       Llamar a bindToCustomerSession(token) fusiona cualquier item que el
 *       invitado haya agregado antes de iniciar sesion con los que ya tenia
 *       guardados en su cuenta, y limpia el carrito de invitado.
 *
 *   - Al CERRAR sesion:
 *       Llamar a unbindFromSession() limpia el carrito en memoria y elimina
 *       los datos de sessionStorage del invitado. El carrito de la cuenta
 *       queda intacto en localStorage para la proxima vez.
 */

import { Injectable, computed, effect, signal } from '@angular/core';

import { CartItem, ShipmentCustomer } from '../models/cart.models';
import { BusinessProduct } from '../models/marketplace.models';

const GUEST_CART_KEY = 'localshop_guest_cart';
const SHIPMENT_STORAGE_KEY = 'localshop_pending_shipment';
const CUSTOMER_TOKEN_KEY = 'marketplace_customer_token';

function buildAccountCartKey(token: string): string {
  return `localshop_cart_${token}`;
}

@Injectable({
  providedIn: 'root',
})
export class CartService {
  private noticeTimeoutId: ReturnType<typeof setTimeout> | null = null;
  private pulseTimeoutId: ReturnType<typeof setTimeout> | null = null;

  readonly items = signal<CartItem[]>(this.loadItemsOnStartup());
  readonly notice = signal<string | null>(null);
  readonly isPulsing = signal(false);
  readonly pendingShipment = signal<ShipmentCustomer | null>(this.loadShipmentFromStorage());
  readonly count = computed(() => this.items().reduce((total, item) => total + item.quantity, 0));
  readonly total = computed(() => this.items().reduce((total, item) => total + item.quantity * item.price, 0));

  constructor() {
    // Cada vez que cambien los items se persisten en el almacenamiento correcto.
    effect(() => {
      this.saveItems(this.items());
    });

    // Cada vez que cambie el envio pendiente, se guarda en localStorage.
    effect(() => {
      this.saveShipmentToStorage(this.pendingShipment());
    });
  }

  /*
   * Debe llamarse justo despues de un inicio de sesion exitoso.
   * Fusiona los items de invitado con los de la cuenta y limpia el
   * carrito de invitado de sessionStorage.
   */
  bindToCustomerSession(token: string): void {
    const guestItems = this.loadGuestCart();
    const accountItems = this.loadAccountCart(token);

    // Merge: los items de la cuenta tienen prioridad; los de invitado se
    // agregan solo si no existen ya en la cuenta.
    const merged = [...accountItems];
    for (const guestItem of guestItems) {
      const exists = merged.find((item) => item.productId === guestItem.productId);
      if (!exists) {
        merged.push(guestItem);
      }
    }

    this.items.set(merged);
    this.clearGuestCart();
    this.saveAccountCart(token, merged);
  }

  /*
   * Debe llamarse justo despues de cerrar sesion.
   * Limpia el carrito en memoria y el sessionStorage de invitado.
   * El carrito de la cuenta permanece en localStorage para la proxima sesion.
   */
  unbindFromSession(): void {
    this.items.set([]);
    this.notice.set(null);
    this.isPulsing.set(false);
    this.pendingShipment.set(null);
    this.clearGuestCart();
  }

  addProduct(product: BusinessProduct, businessName: string): string {
    this.items.update((items) => {
      const existing = items.find((item) => item.productId === product.productId);
      if (existing) {
        return items.map((item) =>
          item.productId === product.productId
            ? { ...item, quantity: Math.min(item.quantity + 1, item.stock) }
            : item,
        );
      }

      return [...items, { ...product, quantity: 1, businessName }];
    });

    const message = `"${product.name}" se agrego al carrito de ${businessName}.`;
    this.showNotice(message);
    return message;
  }

  increaseQuantity(productId: number): void {
    this.items.update((items) =>
      items.map((item) =>
        item.productId === productId
          ? { ...item, quantity: Math.min(item.quantity + 1, item.stock) }
          : item,
      ),
    );
  }

  decreaseQuantity(productId: number): void {
    this.items.update((items) =>
      items.flatMap((item) => {
        if (item.productId !== productId) return [item];
        const next = item.quantity - 1;
        return next > 0 ? [{ ...item, quantity: next }] : [];
      }),
    );
  }

  removeFromCart(productId: number): void {
    this.items.update((items) => items.filter((item) => item.productId !== productId));
  }

  clearBusiness(businessId: number): void {
    this.items.update((items) => items.filter((item) => item.businessId !== businessId));
  }

  // Vacia el carrito completamente (usado tras un pago exitoso).
  clear(): void {
    this.items.set([]);
    this.notice.set(null);
    this.isPulsing.set(false);
    this.pendingShipment.set(null);
    this.clearGuestCart();
    const token = this.readCustomerToken();
    if (token) {
      this.saveAccountCart(token, []);
    }
  }

  savePendingShipment(shipment: ShipmentCustomer): void {
    this.pendingShipment.set(shipment);
  }

  clearPendingShipment(): void {
    this.pendingShipment.set(null);
  }

  // Determina en el arranque de que almacenamiento cargar los items.
  private loadItemsOnStartup(): CartItem[] {
    const token = this.readCustomerToken();
    if (token) {
      // Hay sesion activa: cargar el carrito de la cuenta.
      return this.loadAccountCart(token);
    }
    // Sin sesion: cargar el carrito de invitado (sessionStorage).
    return this.loadGuestCart();
  }

  // Persiste en el almacenamiento correcto segun si hay sesion activa.
  private saveItems(items: CartItem[]): void {
    const token = this.readCustomerToken();
    if (token) {
      this.saveAccountCart(token, items);
    } else {
      this.saveGuestCart(items);
    }
  }

  private loadAccountCart(token: string): CartItem[] {
    try {
      const raw = localStorage.getItem(buildAccountCartKey(token));
      return raw ? (JSON.parse(raw) as CartItem[]) : [];
    } catch {
      return [];
    }
  }

  private saveAccountCart(token: string, items: CartItem[]): void {
    try {
      localStorage.setItem(buildAccountCartKey(token), JSON.stringify(items));
    } catch {
      // localStorage no disponible, se ignora.
    }
  }

  private loadGuestCart(): CartItem[] {
    try {
      const raw = sessionStorage.getItem(GUEST_CART_KEY);
      return raw ? (JSON.parse(raw) as CartItem[]) : [];
    } catch {
      return [];
    }
  }

  private saveGuestCart(items: CartItem[]): void {
    try {
      sessionStorage.setItem(GUEST_CART_KEY, JSON.stringify(items));
    } catch {
      // sessionStorage no disponible, se ignora.
    }
  }

  private clearGuestCart(): void {
    try {
      sessionStorage.removeItem(GUEST_CART_KEY);
    } catch {
      // sessionStorage no disponible, se ignora.
    }
  }

  private readCustomerToken(): string | null {
    try {
      return localStorage.getItem(CUSTOMER_TOKEN_KEY);
    } catch {
      return null;
    }
  }

  private loadShipmentFromStorage(): ShipmentCustomer | null {
    try {
      const raw = localStorage.getItem(SHIPMENT_STORAGE_KEY);
      return raw ? (JSON.parse(raw) as ShipmentCustomer) : null;
    } catch {
      return null;
    }
  }

  private saveShipmentToStorage(shipment: ShipmentCustomer | null): void {
    try {
      if (shipment) {
        localStorage.setItem(SHIPMENT_STORAGE_KEY, JSON.stringify(shipment));
      } else {
        localStorage.removeItem(SHIPMENT_STORAGE_KEY);
      }
    } catch {
      // localStorage no disponible, se ignora silenciosamente.
    }
  }

  private showNotice(message: string): void {
    this.notice.set(message);
    this.isPulsing.set(true);

    if (this.noticeTimeoutId) clearTimeout(this.noticeTimeoutId);
    if (this.pulseTimeoutId) clearTimeout(this.pulseTimeoutId);

    this.noticeTimeoutId = setTimeout(() => {
      this.notice.set(null);
      this.noticeTimeoutId = null;
    }, 2800);

    this.pulseTimeoutId = setTimeout(() => {
      this.isPulsing.set(false);
      this.pulseTimeoutId = null;
    }, 700);
  }
}
