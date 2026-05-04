import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { BusinessProduct, MarketplaceBusiness, MarketplaceSnapshot } from '../../../../core/models/store.models';
import { CartService } from '../../../../core/services/cart.service';
import { StoreService } from '../../../../core/services/store.service';

/**
 * Vitrina publica principal de LocalShop.
 * Centraliza:
 * - carga del marketplace,
 * - filtros de busqueda y categorias,
 * - acceso a perfiles empresariales,
 * - estado visual del carrito global.
 */
@Component({
  selector: 'app-marketplace-page',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './marketplace-page.html',
})
export class MarketplacePage implements OnInit {
  private readonly storeService = inject(StoreService);
  private readonly cartService = inject(CartService);
  private readonly router = inject(Router);

  protected readonly marketplace = signal<MarketplaceSnapshot | null>(null);
  protected readonly currentBusiness = signal<MarketplaceBusiness | null>(null);
  protected readonly selectedBusinessId = signal<number | null>(null);
  protected readonly activeCategory = signal('Todas');
  protected readonly searchTerm = signal('');
  protected readonly isLoading = signal(false);
  protected readonly pageFeedback = signal<{ type: 'error'; message: string } | null>(null);
  protected readonly cartFeedback = signal<{ type: 'success' | 'error'; message: string } | null>(null);

  protected readonly businesses = computed(() => this.marketplace()?.businesses ?? []);
  protected readonly featuredProducts = computed(() => this.marketplace()?.featuredProducts ?? []);
  protected readonly totalBusinesses = computed(() => this.marketplace()?.totalBusinesses ?? 0);
  protected readonly totalProducts = computed(() => this.marketplace()?.totalProducts ?? 0);
  protected readonly availableCategories = computed(() => this.marketplace()?.categories ?? []);
  protected readonly cartCount = this.cartService.count;
  protected readonly cartNotice = this.cartService.notice;
  protected readonly cartPulse = this.cartService.isPulsing;
  protected readonly selectedBusiness = computed(() => {
    const businesses = this.businesses();
    if (businesses.length === 0) {
      return null;
    }

    return businesses.find((business) => business.businessId === this.selectedBusinessId()) ?? businesses[0];
  });
  protected readonly filteredProducts = computed(() => {
    const query = this.searchTerm().trim().toLowerCase();
    const category = this.activeCategory();

    return this.businesses()
      .flatMap((business) =>
        business.products
          .filter((product) => product.isPublished)
          .map((product) => ({
            ...product,
            businessName: business.businessName,
            businessSlug: business.slug,
            city: business.city,
          })),
      )
      .filter((product) => {
        const matchesCategory = category === 'Todas' || product.category === category;
        const matchesQuery =
          query.length === 0 ||
          [product.name, product.category, product.description, product.businessName, product.city]
            .join(' ')
            .toLowerCase()
            .includes(query);

        return matchesCategory && matchesQuery;
      });
  });
  protected readonly isAuthenticated = computed(() => !!this.currentBusiness());
  protected readonly currentYear = new Date().getFullYear();

  ngOnInit(): void {
    void this.initializeMarketplace();
  }

  protected setSearchTerm(value: string): void {
    this.searchTerm.set(value);
  }

  protected setCategory(category: string): void {
    this.activeCategory.set(category);
  }

  protected previewBusiness(businessId: number): void {
    this.selectedBusinessId.set(businessId);
  }

  protected openBusinessProfileById(businessId: number): void {
    const business = this.businesses().find((item) => item.businessId === businessId);
    if (!business) {
      return;
    }

    void this.router.navigate(['/empresa', business.slug]);
  }

  protected openProductProfile(product: BusinessProduct): void {
    this.openBusinessProfileById(product.businessId);
  }

  protected openCurrentCartBusiness(): void {
    void this.router.navigate(['/carrito']);
  }

  protected openPortalPage(): void {
    void this.router.navigate(['/portal']);
  }

  protected addToCart(product: BusinessProduct): void {
    const business = this.businesses().find((item) => item.businessId === product.businessId);
    if (!business) {
      return;
    }

    this.cartFeedback.set(null);
    this.cartService.addProduct(product, business.businessName);
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

  private async initializeMarketplace(): Promise<void> {
    this.isLoading.set(true);
    await this.reloadMarketplace();
    this.isLoading.set(false);
  }

  private async reloadMarketplace(): Promise<void> {
    try {
      const snapshot = await this.storeService.getMarketplaceSnapshot();
      this.marketplace.set(snapshot);
      this.pageFeedback.set(null);

      const selectedBusinessId = this.selectedBusinessId();
      const hasSelectedBusiness = snapshot.businesses.some((business) => business.businessId === selectedBusinessId);

      if (!selectedBusinessId || !hasSelectedBusiness) {
        this.selectedBusinessId.set(snapshot.businesses[0]?.businessId ?? null);
      }

      await this.syncCurrentBusiness();
    } catch (error) {
      this.pageFeedback.set({
        type: 'error',
        message: error instanceof Error ? error.message : 'No se pudo cargar el marketplace.',
      });
    }
  }

  private async syncCurrentBusiness(): Promise<void> {
    try {
      const business = await this.storeService.getCurrentBusiness();
      this.currentBusiness.set(business);
    } catch {
      this.currentBusiness.set(null);
    }
  }
}
