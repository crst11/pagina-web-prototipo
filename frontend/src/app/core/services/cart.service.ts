import { Injectable, computed, signal } from '@angular/core';

import { BusinessProduct, CartItem } from '../models/store.models';

@Injectable({
  providedIn: 'root',
})
export class CartService {
  private noticeTimeoutId: ReturnType<typeof setTimeout> | null = null;
  private pulseTimeoutId: ReturnType<typeof setTimeout> | null = null;

  readonly items = signal<CartItem[]>([]);
  readonly notice = signal<string | null>(null);
  readonly isPulsing = signal(false);
  readonly count = computed(() => this.items().reduce((total, item) => total + item.quantity, 0));
  readonly total = computed(() => this.items().reduce((total, item) => total + item.quantity * item.price, 0));

  addProduct(product: BusinessProduct, businessName: string): string {
    this.items.update((items) => {
      const current = items.find((item) => item.productId === product.productId);
      if (current) {
        return items.map((item) =>
          item.productId === product.productId
            ? { ...item, quantity: Math.min(item.quantity + 1, item.stock) }
            : item,
        );
      }

      return [
        ...items,
        {
          ...product,
          quantity: 1,
          businessName,
        },
      ];
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
        if (item.productId !== productId) {
          return [item];
        }

        const nextQuantity = item.quantity - 1;
        return nextQuantity > 0 ? [{ ...item, quantity: nextQuantity }] : [];
      }),
    );
  }

  removeFromCart(productId: number): void {
    this.items.update((items) => items.filter((item) => item.productId !== productId));
  }

  clearBusiness(businessId: number): void {
    this.items.update((items) => items.filter((item) => item.businessId !== businessId));
  }

  clear(): void {
    this.items.set([]);
    this.notice.set(null);
    this.isPulsing.set(false);
  }

  private showNotice(message: string): void {
    this.notice.set(message);
    this.isPulsing.set(true);

    if (this.noticeTimeoutId) {
      clearTimeout(this.noticeTimeoutId);
    }

    if (this.pulseTimeoutId) {
      clearTimeout(this.pulseTimeoutId);
    }

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
