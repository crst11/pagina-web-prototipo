import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { MarketplaceBusiness, MarketplaceSnapshot } from '../../../../core/models/store.models';
import { CartService } from '../../../../core/services/cart.service';
import { StoreService } from '../../../../core/services/store.service';

@Component({
  selector: 'app-business-profile-page',
  imports: [CommonModule, RouterLink],
  templateUrl: './business-profile-page.html',
  styleUrl: './business-profile-page.css',
})
export class BusinessProfilePage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly storeService = inject(StoreService);
  private readonly cartService = inject(CartService);

  protected readonly marketplace = signal<MarketplaceSnapshot | null>(null);
  protected readonly business = signal<MarketplaceBusiness | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly pageFeedback = signal<{ type: 'error'; message: string } | null>(null);
  protected readonly cartFeedback = signal<{ type: 'success' | 'error'; message: string } | null>(null);
  protected readonly orderFeedback = signal<{ type: 'success' | 'error'; message: string } | null>(null);
  protected readonly profileHeroBackground = computed(() => {
    const bannerUrl = this.business()?.bannerUrl;
    if (!bannerUrl) {
      return null;
    }

    return `linear-gradient(135deg, rgba(255, 255, 255, 0.9), rgba(237, 248, 255, 0.82)), linear-gradient(110deg, rgba(18, 48, 71, 0.16), rgba(15, 118, 110, 0.14)), url("${bannerUrl}") center / cover no-repeat`;
  });
  protected readonly currentBusinessProducts = computed(
    () => this.business()?.products.filter((product) => product.isPublished) ?? [],
  );
  protected readonly cartNotice = this.cartService.notice;
  protected readonly cartPulse = this.cartService.isPulsing;
  protected readonly currentBusinessCategories = computed(() =>
    Array.from(new Set(this.currentBusinessProducts().map((product) => product.category))),
  );
  protected readonly currentCartItems = computed(() => {
    const business = this.business();
    if (!business) {
      return [];
    }

    return this.cartService.items().filter((item) => item.businessId === business.businessId);
  });
  protected readonly cartCount = this.cartService.count;
  protected readonly currentBusinessCartCount = computed(() =>
    this.currentCartItems().reduce((total, item) => total + item.quantity, 0),
  );
  protected readonly cartTotal = computed(() =>
    this.currentCartItems().reduce((total, item) => total + item.quantity * item.price, 0),
  );
  protected readonly meetsMinimumOrder = computed(() => {
    const business = this.business();
    if (!business) {
      return false;
    }

    return this.currentCartItems().length > 0 && this.cartTotal() >= business.minimumOrderAmount;
  });
  protected readonly externalCartItemsCount = computed(
    () => this.cartService.items().reduce((total, item) => total + item.quantity, 0) - this.currentBusinessCartCount(),
  );
  protected readonly hasExternalCart = computed(() => this.externalCartItemsCount() > 0);

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      const slug = params.get('slug');
      if (slug) {
        void this.loadBusiness(slug);
      }
    });
  }

  protected initials(value?: string | null): string {
    if (!value?.trim()) {
      return 'LS';
    }

    return value
      .trim()
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part.charAt(0).toUpperCase())
      .join('');
  }

  protected openCurrentCartBusiness(): void {
    void this.router.navigate(['/carrito']);
  }

  protected addToCart(productId: number): void {
    const business = this.business();
    const product = business?.products.find((item) => item.productId === productId);

    if (!business || !product) {
      return;
    }

    this.orderFeedback.set(null);
    this.cartFeedback.set(null);
    this.cartService.addProduct(product, business.businessName);
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

  protected openCheckout(): void {
    if (this.cartCount() === 0) {
      this.cartFeedback.set({
        type: 'error',
        message: 'Agrega al menos un producto al carrito antes de abrir la pagina del pedido.',
      });
      return;
    }

    this.cartFeedback.set(null);
    void this.router.navigate(['/carrito']);
  }

  private async loadBusiness(slug: string): Promise<void> {
    this.isLoading.set(true);

    try {
      const snapshot = await this.storeService.getMarketplaceSnapshot();
      const business = snapshot.businesses.find((item) => item.slug === slug) ?? null;

      this.marketplace.set(snapshot);
      this.business.set(business);
      this.cartFeedback.set(null);

      if (!business) {
        this.pageFeedback.set({
          type: 'error',
          message: 'La empresa que buscas no existe o ya no esta publicada en la vitrina.',
        });
      } else {
        this.pageFeedback.set(null);
      }
    } catch (error) {
      this.pageFeedback.set({
        type: 'error',
        message: error instanceof Error ? error.message : 'No se pudo cargar el perfil empresarial.',
      });
    } finally {
      this.isLoading.set(false);
    }
  }
}
