import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

import { BusinessProduct, MarketplaceBusiness, MarketplaceSnapshot } from '../../../core/models/marketplace.models';
import { CustomerProfile } from '../../../core/models/customer.models';
import { CartService } from '../../../core/services/cart.service';
import { AuthService } from '../../../core/services/auth.service';
import { CustomerService } from '../../../core/services/customer.service';
import { MarketplaceService } from '../../../core/services/marketplace.service';

@Injectable({
  providedIn: 'root',
})
export class MarketplacePageState {
  private readonly authService = inject(AuthService);
  private readonly customerService = inject(CustomerService);
  private readonly marketplaceService = inject(MarketplaceService);
  private readonly cartService = inject(CartService);
  private readonly router = inject(Router);

  readonly marketplace = signal<MarketplaceSnapshot | null>(null);
  readonly currentBusiness = signal<MarketplaceBusiness | null>(null);
  readonly currentCustomer = signal<CustomerProfile | null>(null);
  readonly selectedBusinessId = signal<number | null>(null);
  readonly activeCategory = signal('Todas');
  readonly searchTerm = signal('');
  readonly isLoading = signal(false);
  readonly pageFeedback = signal<{ type: 'error'; message: string } | null>(null);
  readonly cartFeedback = signal<{ type: 'success' | 'error'; message: string } | null>(null);

  readonly businesses = computed(() => this.marketplace()?.businesses ?? []);
  readonly featuredProducts = computed(() => this.marketplace()?.featuredProducts ?? []);
  readonly totalBusinesses = computed(() => this.marketplace()?.totalBusinesses ?? 0);
  readonly totalProducts = computed(() => this.marketplace()?.totalProducts ?? 0);
  readonly availableCategories = computed(() => this.marketplace()?.categories ?? []);
  readonly cartCount = this.cartService.count;
  readonly cartNotice = this.cartService.notice;
  readonly cartPulse = this.cartService.isPulsing;
  readonly isAuthenticated = computed(() => !!this.currentBusiness());
  readonly isCustomerAuthenticated = computed(() => !!this.currentCustomer());
  readonly isAnyAuthenticated = computed(() => this.isAuthenticated() || this.isCustomerAuthenticated());
  readonly selectedBusiness = computed(() => {
    const businesses = this.businesses();
    if (businesses.length === 0) return null;
    return businesses.find((business) => business.businessId === this.selectedBusinessId()) ?? businesses[0];
  });
  readonly filteredProducts = computed(() => {
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

  async initialize(): Promise<void> {
    if (this.marketplace()) {
      await this.syncCurrentBusiness();
      return;
    }

    this.isLoading.set(true);
    await this.reloadMarketplace();
    this.isLoading.set(false);
  }

  setSearchTerm(value: string): void { this.searchTerm.set(value); }
  setCategory(category: string): void { this.activeCategory.set(category); }
  previewBusiness(businessId: number): void { this.selectedBusinessId.set(businessId); }

  openBusinessProfileById(businessId: number): void {
    const business = this.businesses().find((item) => item.businessId === businessId);
    if (!business) return;
    void this.router.navigate(['/empresa', business.slug]);
  }

  openProductProfile(product: BusinessProduct): void { this.openBusinessProfileById(product.businessId); }
  openCurrentCartBusiness(): void { void this.router.navigate(['/carrito']); }
  openPortalPage(): void { void this.router.navigate(['/portal']); }

  openAccountPage(): void {
    void this.router.navigate([this.currentCustomer() ? '/cliente' : '/portal']);
  }

  addToCart(product: BusinessProduct): void {
    const business = this.businesses().find((item) => item.businessId === product.businessId);
    if (!business) return;
    this.cartFeedback.set(null);
    this.cartService.addProduct(product, business.businessName);
  }

  async reloadMarketplace(): Promise<void> {
    try {
      const snapshot = await this.marketplaceService.getMarketplaceSnapshot();
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
      const business = await this.authService.getCurrentBusiness();
      this.currentBusiness.set(business);
    } catch {
      this.currentBusiness.set(null);
    }

    try {
      const customer = await this.customerService.getCurrentCustomer();
      this.currentCustomer.set(customer);
    } catch {
      this.currentCustomer.set(null);
    }
  }
}
