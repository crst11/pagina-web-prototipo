import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

import { CartBusinessGroup } from '../../../../core/models/cart.models';
import { CustomerProfile } from '../../../../core/models/customer.models';
import { MarketplaceBusiness, MarketplaceSnapshot } from '../../../../core/models/marketplace.models';
import { ShipmentCustomer } from '../../../../core/models/cart.models';
import { CartService } from '../../../../core/services/cart.service';
import { AuthService } from '../../../../core/services/auth.service';
import { CustomerService } from '../../../../core/services/customer.service';
import { MarketplaceService } from '../../../../core/services/marketplace.service';
import { validateEmail, validatePhone, validateFullName, validateCity, validateAddress } from '../../../../core/validators/form.validators';

@Component({
  selector: 'app-cart-page',
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.css',
})
export class CartPage implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly customerService = inject(CustomerService);
  private readonly marketplaceService = inject(MarketplaceService);
  private readonly cartService = inject(CartService);
  private readonly router = inject(Router);

  protected readonly marketplace = signal<MarketplaceSnapshot | null>(null);
  protected readonly currentCustomer = signal<CustomerProfile | null>(null);
  protected readonly currentBusiness = signal<MarketplaceBusiness | null>(null);
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
    if (!bannerUrl) return null;
    return `linear-gradient(135deg, rgba(255, 255, 255, 0.92), rgba(230, 255, 251, 0.88)), url("${bannerUrl}") center / cover no-repeat`;
  });

  ngOnInit(): void {
    void this.loadCustomer();
    void this.loadMarketplace();
  }

  protected async refreshMarketplace(): Promise<void> {
    await this.loadMarketplace();
  }

  protected openBusinessProfile(slug: string): void {
    if (!slug) return;
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
    if (this.currentBusiness()) {
      this.orderFeedback.set({
        type: 'error',
        message: 'Cierra la sesion empresarial e inicia sesion como cliente para comprar.',
      });
      return;
    }

    if (!this.currentCustomer()) {
      this.orderFeedback.set({
        type: 'error',
        message: 'Para proteger tu compra debes registrarte o iniciar sesion como cliente antes de pagar.',
      });
      return;
    }

    const nameError = validateFullName(this.shipmentForm.fullName);
    if (nameError) { this.orderFeedback.set({ type: 'error', message: nameError }); return; }
    const emailError = validateEmail(this.shipmentForm.email);
    if (emailError) { this.orderFeedback.set({ type: 'error', message: emailError }); return; }
    const phoneError = validatePhone(this.shipmentForm.phone);
    if (phoneError) { this.orderFeedback.set({ type: 'error', message: phoneError }); return; }
    const cityError = validateCity(this.shipmentForm.city);
    if (cityError) { this.orderFeedback.set({ type: 'error', message: cityError }); return; }
    const addressError = validateAddress(this.shipmentForm.address);
    if (addressError) { this.orderFeedback.set({ type: 'error', message: addressError }); return; }

    const groups = this.cartGroups();
    if (groups.length === 0) {
      this.orderFeedback.set({ type: 'error', message: 'No hay productos listos para comprar.' });
      return;
    }

    if (this.invalidBusinessGroups().length > 0) {
      this.orderFeedback.set({ type: 'error', message: `Aun faltan minimos de compra en: ${this.invalidBusinessNames()}.` });
      return;
    }

    try {
      this.cartService.savePendingShipment({
        fullName: this.shipmentForm.fullName,
        email: this.shipmentForm.email,
        phone: this.shipmentForm.phone,
        city: this.shipmentForm.city,
        address: this.shipmentForm.address,
        notes: this.shipmentForm.notes,
      });
      void this.router.navigate(['/pago']);
    } catch (error) {
      this.orderFeedback.set({ type: 'error', message: 'No se pudo proceder al pago.' });
    }
  }

  private async loadMarketplace(): Promise<void> {
    this.isLoading.set(true);
    try {
      const snapshot = await this.marketplaceService.getMarketplaceSnapshot();
      this.marketplace.set(snapshot);
      this.pageFeedback.set(null);
    } catch (error) {
      this.pageFeedback.set({ type: 'error', message: error instanceof Error ? error.message : 'No se pudo cargar la informacion del carrito.' });
    } finally {
      this.isLoading.set(false);
    }
  }

  private async loadCustomer(): Promise<void> {
    try {
      const customer = await this.customerService.getCurrentCustomer();
      const business = await this.authService.getCurrentBusiness();
      this.currentCustomer.set(customer);
      this.currentBusiness.set(business);
      if (customer) {
        this.shipmentForm = this.createDefaultShipment(customer);
      }
    } catch {
      this.currentCustomer.set(null);
      this.currentBusiness.set(null);
    }
  }

  private createDefaultShipment(customer?: CustomerProfile | null): ShipmentCustomer {
    return {
      fullName: customer?.fullName ?? '',
      email: customer?.email ?? '',
      phone: customer?.phone ?? '',
      city: customer?.city ?? '',
      address: customer?.address ?? '',
      notes: '',
    };
  }
}
