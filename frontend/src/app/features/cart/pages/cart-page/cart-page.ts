import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { CartBusinessGroup, MarketplaceSnapshot, ShipmentCustomer } from '../../../../core/models/store.models';
import { CartService } from '../../../../core/services/cart.service';
import { StoreService } from '../../../../core/services/store.service';

@Component({
  selector: 'app-cart-page',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.css',
})
export class CartPage implements OnInit {
  private readonly storeService = inject(StoreService);
  private readonly cartService = inject(CartService);
  private readonly router = inject(Router);

  protected readonly marketplace = signal<MarketplaceSnapshot | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly pageFeedback = signal<{ type: 'error'; message: string } | null>(null);
  protected readonly cartFeedback = signal<{ type: 'success' | 'error'; message: string } | null>(null);
  protected readonly orderFeedback = signal<{ type: 'success' | 'error'; message: string } | null>(null);

  protected shipmentForm: ShipmentCustomer = this.createDefaultShipment();

  protected readonly currentCartItems = this.cartService.items;
  protected readonly cartCount = this.cartService.count;
  protected readonly cartTotal = this.cartService.total;
  protected readonly cartNotice = this.cartService.notice;
  protected readonly cartPulse = this.cartService.isPulsing;
  protected readonly cartGroups = computed<CartBusinessGroup[]>(() => {
    const businesses = this.marketplace()?.businesses ?? [];
    const grouped = new Map<number, CartBusinessGroup>();

    for (const item of this.currentCartItems()) {
      const business = businesses.find((entry) => entry.businessId === item.businessId);
      const currentGroup =
        grouped.get(item.businessId) ??
        {
          businessId: item.businessId,
          businessName: business?.businessName ?? item.businessName,
          businessSlug: business?.slug ?? '',
          bannerUrl: business?.bannerUrl ?? '/assets/images/banner-localshop-default.jpg',
          minimumOrderAmount: business?.minimumOrderAmount ?? 0,
          city: business?.city ?? '',
          address: business?.address ?? '',
          tagline: business?.tagline ?? '',
          phone: business?.phone ?? '',
          email: business?.email ?? '',
          items: [],
          subtotal: 0,
        };

      currentGroup.items.push(item);
      currentGroup.subtotal += item.quantity * item.price;
      grouped.set(item.businessId, currentGroup);
    }

    return Array.from(grouped.values());
  });
  protected readonly businessCount = computed(() => this.cartGroups().length);
  protected readonly invalidBusinessGroups = computed(() =>
    this.cartGroups().filter((group) => group.subtotal < group.minimumOrderAmount),
  );
  protected readonly invalidBusinessNames = computed(() =>
    this.invalidBusinessGroups()
      .map((group) => group.businessName)
      .join(', '),
  );
  protected readonly cartHeroBackground = computed(() => {
    const bannerUrl = this.cartGroups()[0]?.bannerUrl;
    if (!bannerUrl) {
      return null;
    }

    return `linear-gradient(135deg, rgba(255, 255, 255, 0.92), rgba(237, 248, 255, 0.86)), linear-gradient(110deg, rgba(18, 48, 71, 0.14), rgba(15, 118, 110, 0.12)), url("${bannerUrl}") center / cover no-repeat`;
  });

  ngOnInit(): void {
    void this.loadMarketplace();
  }

  protected async refreshMarketplace(): Promise<void> {
    await this.loadMarketplace();
  }

  protected openBusinessProfile(slug: string): void {
    if (!slug) {
      return;
    }

    void this.router.navigate(['/empresa', slug]);
  }

  protected increaseQuantity(productId: number): void {
    this.orderFeedback.set(null);
    this.cartService.increaseQuantity(productId);
  }

  protected decreaseQuantity(productId: number): void {
    this.orderFeedback.set(null);
    this.cartService.decreaseQuantity(productId);
  }

  protected removeFromCart(productId: number): void {
    this.orderFeedback.set(null);
    this.cartService.removeFromCart(productId);
  }

  protected async submitShipment(form: NgForm): Promise<void> {
    if (form.invalid) {
      return;
    }

    const groups = this.cartGroups();
    if (groups.length === 0) {
      this.orderFeedback.set({
        type: 'error',
        message: 'No hay productos listos para comprar.',
      });
      return;
    }

    if (this.invalidBusinessGroups().length > 0) {
      this.orderFeedback.set({
        type: 'error',
        message: `Aun faltan minimos de compra en: ${this.invalidBusinessNames()}.`,
      });
      return;
    }

    try {
      const response = await this.storeService.checkoutCart({
        fullName: this.shipmentForm.fullName,
        email: this.shipmentForm.email,
        phone: this.shipmentForm.phone,
        city: this.shipmentForm.city,
        address: this.shipmentForm.address,
        notes: this.shipmentForm.notes,
        items: this.currentCartItems().map((item) => ({
          businessId: item.businessId,
          productId: item.productId,
          quantity: item.quantity,
        })),
      });

      this.cartService.clear();
      this.cartFeedback.set(null);
      this.orderFeedback.set({
        type: 'success',
        message: `${response.message} Total procesado: ${new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(response.total)}.`,
      });
      this.shipmentForm = this.createDefaultShipment();
      form.resetForm(this.shipmentForm);
      await this.loadMarketplace();
    } catch (error) {
      this.orderFeedback.set({
        type: 'error',
        message: error instanceof Error ? error.message : 'No se pudo registrar el pedido.',
      });
    }
  }

  private async loadMarketplace(): Promise<void> {
    this.isLoading.set(true);

    try {
      const snapshot = await this.storeService.getMarketplaceSnapshot();
      this.marketplace.set(snapshot);
      this.pageFeedback.set(null);
    } catch (error) {
      this.pageFeedback.set({
        type: 'error',
        message: error instanceof Error ? error.message : 'No se pudo cargar la informacion del carrito.',
      });
    } finally {
      this.isLoading.set(false);
    }
  }

  private createDefaultShipment(): ShipmentCustomer {
    return {
      fullName: '',
      email: '',
      phone: '',
      city: '',
      address: '',
      notes: '',
    };
  }
}
