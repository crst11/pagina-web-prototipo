import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink } from '@angular/router';

import {
  BusinessOrdersFeed,
  BusinessProduct,
  LoginBusinessRequest,
  MarketplaceBusiness,
  RegisterBusinessRequest,
  UpdateBusinessProfileRequest,
  UpsertBusinessProductRequest,
} from '../../../../core/models/store.models';
import { CartService } from '../../../../core/services/cart.service';
import { StoreService } from '../../../../core/services/store.service';

@Component({
  selector: 'app-portal-page',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './portal-page.html',
  styleUrl: './portal-page.css',
})
export class PortalPage implements OnInit {
  private readonly storeService = inject(StoreService);
  private readonly cartService = inject(CartService);

  protected readonly authMode = signal<'register' | 'login'>('register');
  protected readonly isLoading = signal(false);
  protected readonly currentBusiness = signal<MarketplaceBusiness | null>(null);
  protected readonly ordersFeed = signal<BusinessOrdersFeed | null>(null);
  protected readonly editingProductId = signal<number | null>(null);
  protected readonly authFeedback = signal<{ type: 'success' | 'error'; message: string } | null>(null);
  protected readonly businessFeedback = signal<{ type: 'success' | 'error'; message: string } | null>(null);

  protected registerForm: RegisterBusinessRequest = this.createDefaultRegisterForm();
  protected loginForm: LoginBusinessRequest = {
    email: '',
    password: '',
  };
  protected businessProfileForm: UpdateBusinessProfileRequest = this.createDefaultBusinessProfile();
  protected productForm: UpsertBusinessProductRequest = this.createDefaultProduct();

  protected readonly isAuthenticated = computed(() => !!this.currentBusiness());
  protected readonly currentBusinessProductCount = computed(() => this.currentBusiness()?.products.length ?? 0);
  protected readonly currentBusinessFeaturedCount = computed(
    () => this.currentBusiness()?.products.filter((product) => product.isFeatured).length ?? 0,
  );
  protected readonly currentBusinessPublishedCount = computed(
    () => this.currentBusiness()?.products.filter((product) => product.isPublished).length ?? 0,
  );
  protected readonly newOrdersCount = computed(() => this.ordersFeed()?.newOrders ?? 0);
  protected readonly cartCount = this.cartService.count;
  protected readonly portalHeroBackground = computed(() => {
    const bannerUrl = this.currentBusiness()?.bannerUrl;
    if (!bannerUrl) {
      return null;
    }

    return `linear-gradient(135deg, rgba(255, 255, 255, 0.92), rgba(237, 248, 255, 0.86)), linear-gradient(110deg, rgba(18, 48, 71, 0.14), rgba(15, 118, 110, 0.12)), url("${bannerUrl}") center / cover no-repeat`;
  });

  ngOnInit(): void {
    void this.syncCurrentBusiness();
  }

  protected async submitRegister(form: NgForm): Promise<void> {
    if (form.invalid) {
      return;
    }

    try {
      const business = await this.storeService.registerBusiness(this.registerForm);
      this.currentBusiness.set(business);
      this.authFeedback.set({
        type: 'success',
        message: 'Tu perfil quedo creado y ya puedes administrar catalogo, imagenes y productos.',
      });
      this.businessFeedback.set(null);
      this.registerForm = this.createDefaultRegisterForm();
      form.resetForm(this.registerForm);
      this.syncFormsFromBusiness(business);
      await this.loadOrders();
    } catch (error) {
      this.authFeedback.set({
        type: 'error',
        message: error instanceof Error ? error.message : 'No se pudo registrar la empresa.',
      });
    }
  }

  protected async submitLogin(form: NgForm): Promise<void> {
    if (form.invalid) {
      return;
    }

    try {
      const business = await this.storeService.loginBusiness(this.loginForm);
      this.currentBusiness.set(business);
      this.authFeedback.set({
        type: 'success',
        message: `Sesion iniciada como ${business.businessName}.`,
      });
      this.businessFeedback.set(null);
      this.loginForm = {
        email: '',
        password: '',
      };
      form.resetForm(this.loginForm);
      this.syncFormsFromBusiness(business);
      await this.loadOrders();
    } catch (error) {
      this.authFeedback.set({
        type: 'error',
        message: error instanceof Error ? error.message : 'No se pudo iniciar sesion.',
      });
    }
  }

  protected async logout(): Promise<void> {
    try {
      await this.storeService.logoutBusiness();
      this.currentBusiness.set(null);
      this.businessProfileForm = this.createDefaultBusinessProfile();
      this.productForm = this.createDefaultProduct();
      this.ordersFeed.set(null);
      this.editingProductId.set(null);
      this.businessFeedback.set({
        type: 'success',
        message: 'Sesion empresarial cerrada.',
      });
    } catch (error) {
      this.businessFeedback.set({
        type: 'error',
        message: error instanceof Error ? error.message : 'No se pudo cerrar la sesion.',
      });
    }
  }

  protected async saveBusinessProfile(form: NgForm): Promise<void> {
    if (form.invalid) {
      return;
    }

    try {
      const business = await this.storeService.updateCurrentBusiness(this.businessProfileForm);
      this.currentBusiness.set(business);
      this.syncFormsFromBusiness(business);
      await this.loadOrders();
      this.businessFeedback.set({
        type: 'success',
        message: 'Tu perfil publico se actualizo correctamente.',
      });
    } catch (error) {
      this.businessFeedback.set({
        type: 'error',
        message: error instanceof Error ? error.message : 'No se pudo guardar el perfil.',
      });
    }
  }

  protected async saveProduct(form: NgForm): Promise<void> {
    if (form.invalid) {
      return;
    }

    try {
      const business = await this.storeService.saveProduct(this.productForm, this.editingProductId() ?? undefined);
      this.currentBusiness.set(business);
      this.syncFormsFromBusiness(business);
      await this.loadOrders();
      this.businessFeedback.set({
        type: 'success',
        message: this.editingProductId()
          ? 'Producto actualizado correctamente.'
          : 'Producto agregado al catalogo de tu empresa.',
      });
      this.resetProductForm(form);
    } catch (error) {
      this.businessFeedback.set({
        type: 'error',
        message: error instanceof Error ? error.message : 'No se pudo guardar el producto.',
      });
    }
  }

  protected editProduct(product: BusinessProduct): void {
    this.editingProductId.set(product.productId);
    this.productForm = {
      name: product.name,
      category: product.category,
      description: product.description,
      price: product.price,
      minimumOrder: product.minimumOrder,
      stock: product.stock,
      imageUrl: product.imageUrl,
      isFeatured: product.isFeatured,
      isPublished: product.isPublished,
    };
  }

  protected async deleteProduct(productId: number): Promise<void> {
    try {
      const business = await this.storeService.deleteProduct(productId);
      this.currentBusiness.set(business);
      this.syncFormsFromBusiness(business);
      await this.loadOrders();
      this.businessFeedback.set({
        type: 'success',
        message: 'Producto eliminado del catalogo.',
      });
      if (this.editingProductId() === productId) {
        this.resetProductForm();
      }
    } catch (error) {
      this.businessFeedback.set({
        type: 'error',
        message: error instanceof Error ? error.message : 'No se pudo eliminar el producto.',
      });
    }
  }

  protected resetProductForm(form?: NgForm): void {
    this.editingProductId.set(null);
    this.productForm = this.createDefaultProduct();
    form?.resetForm(this.productForm);
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

  private async syncCurrentBusiness(): Promise<void> {
    this.isLoading.set(true);

    try {
      const business = await this.storeService.getCurrentBusiness();
      this.currentBusiness.set(business);

      if (business) {
        this.syncFormsFromBusiness(business);
        await this.loadOrders();
      } else {
        this.ordersFeed.set(null);
      }
    } finally {
      this.isLoading.set(false);
    }
  }

  private async loadOrders(): Promise<void> {
    try {
      this.ordersFeed.set(await this.storeService.getCurrentBusinessOrders());
    } catch {
      this.ordersFeed.set(null);
    }
  }

  private syncFormsFromBusiness(business: MarketplaceBusiness): void {
    this.businessProfileForm = {
      ownerName: business.ownerName,
      businessName: business.businessName,
      email: business.email,
      phone: business.phone,
      city: business.city,
      address: business.address,
      tagline: business.tagline,
      description: business.description,
      shippingLeadTime: business.shippingLeadTime,
      minimumOrderAmount: business.minimumOrderAmount,
      logoUrl: business.logoUrl,
      bannerUrl: business.bannerUrl,
      websiteUrl: business.websiteUrl,
    };
  }

  private createDefaultRegisterForm(): RegisterBusinessRequest {
    return {
      ownerName: '',
      businessName: '',
      email: '',
      password: '',
      phone: '',
      city: 'Bogota',
      address: '',
      tagline: '',
      description: '',
      shippingLeadTime: 'Entregas locales entre 24 y 48 horas y nacionales entre 2 y 5 dias habiles.',
      minimumOrderAmount: 30000,
      websiteUrl: '',
    };
  }

  private createDefaultBusinessProfile(): UpdateBusinessProfileRequest {
    return {
      ownerName: '',
      businessName: '',
      email: '',
      phone: '',
      city: '',
      address: '',
      tagline: '',
      description: '',
      shippingLeadTime: '',
      minimumOrderAmount: 30000,
      logoUrl: '/assets/images/store1.png',
      bannerUrl: '/assets/images/banner-localshop-default.jpg',
      websiteUrl: '',
    };
  }

  private createDefaultProduct(): UpsertBusinessProductRequest {
    return {
      name: '',
      category: '',
      description: '',
      price: 0,
      minimumOrder: 1,
      stock: 1,
      imageUrl: '/assets/images/pla1.png',
      isFeatured: false,
      isPublished: true,
    };
  }
}
