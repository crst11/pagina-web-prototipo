/*
 * portal-page.ts
 *
 * Componente del portal empresarial. Gestiona el registro e inicio de sesion
 * de propietarios de empresas, la administracion del perfil publico, el catalogo
 * de productos y la bandeja de pedidos entrantes.
 *
 * El portal es independiente de la cuenta de cliente: un mismo correo puede
 * existir en ambas tablas pero representan roles diferentes. Este componente
 * no expone ni gestiona funciones del carrito de compras.
 *
 * Nuevas funciones en esta version:
 *   - Edicion del perfil empresarial desde la tarjeta de vista previa.
 *   - Eliminacion de la cuenta empresarial con modal de confirmacion propio.
 *   - Modal de confirmacion reutilizable (showDeleteModal).
 */

import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { LoginBusinessRequest, RegisterBusinessRequest, UpdateBusinessProfileRequest, UpsertBusinessProductRequest } from '../../../../core/models/auth.models';
import { BusinessProduct, MarketplaceBusiness } from '../../../../core/models/marketplace.models';
import { BusinessOrdersFeed } from '../../../../core/models/order.models';
import { AuthService } from '../../../../core/services/auth.service';
import { OrderService } from '../../../../core/services/order.service';
import { ProductService } from '../../../../core/services/product.service';

@Component({
  selector: 'app-portal-page',
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './portal-page.html',
  styleUrl: './portal-page.css',
})
export class PortalPage implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly productService = inject(ProductService);
  private readonly orderService = inject(OrderService);

  protected readonly authMode = signal<'register' | 'login'>('login');
  protected readonly isLoading = signal(false);
  protected readonly currentBusiness = signal<MarketplaceBusiness | null>(null);
  protected readonly ordersFeed = signal<BusinessOrdersFeed | null>(null);
  protected readonly editingProductId = signal<number | null>(null);
  protected readonly showDeleteModal = signal(false);
  protected readonly authFeedback = signal<{ type: 'success' | 'error'; message: string } | null>(null);
  protected readonly businessFeedback = signal<{ type: 'success' | 'error'; message: string } | null>(null);

  protected registerForm: RegisterBusinessRequest = this.buildEmptyRegisterForm();
  protected loginForm: LoginBusinessRequest = { email: '', password: '' };
  protected businessProfileForm: UpdateBusinessProfileRequest = this.buildEmptyProfileForm();
  protected productForm: UpsertBusinessProductRequest = this.buildEmptyProduct();

  protected readonly isAuthenticated = computed(() => !!this.currentBusiness());
  protected readonly currentBusinessProductCount = computed(() => this.currentBusiness()?.products.length ?? 0);
  protected readonly currentBusinessFeaturedCount = computed(
    () => this.currentBusiness()?.products.filter((p) => p.isFeatured).length ?? 0,
  );
  protected readonly currentBusinessPublishedCount = computed(
    () => this.currentBusiness()?.products.filter((p) => p.isPublished).length ?? 0,
  );
  protected readonly newOrdersCount = computed(() => this.ordersFeed()?.newOrders ?? 0);
  protected readonly portalHeroBackground = computed(() => {
    const url = this.currentBusiness()?.bannerUrl;
    if (!url) return null;
    return `linear-gradient(135deg, rgba(255,255,255,0.92), rgba(237,248,255,0.86)), url("${url}") center / cover no-repeat`;
  });

  ngOnInit(): void {
    void this.syncCurrentBusiness();
  }

  protected async submitRegister(form: NgForm): Promise<void> {
    if (form.invalid) {
      this.authFeedback.set({ type: 'error', message: 'Completa todos los campos obligatorios.' });
      return;
    }

    this.isLoading.set(true);
    try {
      const business = await this.authService.registerBusiness(this.registerForm);
      this.currentBusiness.set(business);
      this.authFeedback.set({ type: 'success', message: 'Empresa registrada correctamente.' });
      this.businessFeedback.set(null);
      this.registerForm = this.buildEmptyRegisterForm();
      form.resetForm(this.registerForm);
      this.syncProfileFormFromBusiness(business);
      await this.loadOrders();
    } catch (error) {
      this.authFeedback.set({
        type: 'error',
        message: error instanceof Error ? error.message : 'No se pudo registrar la empresa.',
      });
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async submitLogin(form: NgForm): Promise<void> {
    if (form.invalid) {
      this.authFeedback.set({ type: 'error', message: 'Ingresa el correo y la contrasena.' });
      return;
    }

    this.isLoading.set(true);
    try {
      const business = await this.authService.loginBusiness(this.loginForm);
      this.currentBusiness.set(business);
      this.authFeedback.set({ type: 'success', message: `Sesion iniciada como ${business.businessName}.` });
      this.businessFeedback.set(null);
      this.loginForm = { email: '', password: '' };
      form.resetForm(this.loginForm);
      this.syncProfileFormFromBusiness(business);
      await this.loadOrders();
    } catch (error) {
      this.authFeedback.set({
        type: 'error',
        message: error instanceof Error ? error.message : 'No se pudo iniciar sesion.',
      });
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async logout(): Promise<void> {
    try {
      if (this.currentBusiness()) await this.authService.logoutBusiness();
      this.currentBusiness.set(null);
      this.businessProfileForm = this.buildEmptyProfileForm();
      this.productForm = this.buildEmptyProduct();
      this.ordersFeed.set(null);
      this.editingProductId.set(null);
      this.businessFeedback.set({ type: 'success', message: 'Sesion empresarial cerrada.' });
    } catch (error) {
      this.businessFeedback.set({
        type: 'error',
        message: error instanceof Error ? error.message : 'No se pudo cerrar la sesion.',
      });
    }
  }

  protected async saveBusinessProfile(form: NgForm): Promise<void> {
    if (form.invalid) return;

    try {
      const business = await this.authService.updateCurrentBusiness(this.businessProfileForm);
      this.currentBusiness.set(business);
      this.syncProfileFormFromBusiness(business);
      await this.loadOrders();
      this.businessFeedback.set({ type: 'success', message: 'Perfil actualizado correctamente.' });
    } catch (error) {
      this.businessFeedback.set({
        type: 'error',
        message: error instanceof Error ? error.message : 'No se pudo guardar el perfil.',
      });
    }
  }

  // Abre el modal de confirmacion para eliminar la cuenta empresarial.
  protected requestDeleteBusiness(): void {
    this.showDeleteModal.set(true);
  }

  protected cancelDeleteBusiness(): void {
    this.showDeleteModal.set(false);
  }

  // Elimina la cuenta tras confirmar en el modal.
  protected async confirmDeleteBusiness(): Promise<void> {
    this.showDeleteModal.set(false);
    this.isLoading.set(true);

    try {
      await this.authService.deleteBusiness();
      this.currentBusiness.set(null);
      this.ordersFeed.set(null);
      this.businessFeedback.set({ type: 'success', message: 'La cuenta empresarial fue eliminada.' });
    } catch (error) {
      this.businessFeedback.set({
        type: 'error',
        message: error instanceof Error ? error.message : 'No fue posible eliminar la cuenta.',
      });
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async saveProduct(form: NgForm): Promise<void> {
    if (form.invalid) return;

    try {
      const business = await this.productService.saveProduct(this.productForm, this.editingProductId() ?? undefined);
      this.currentBusiness.set(business);
      this.syncProfileFormFromBusiness(business);
      await this.loadOrders();
      this.businessFeedback.set({
        type: 'success',
        message: this.editingProductId() ? 'Producto actualizado.' : 'Producto agregado al catalogo.',
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
      const business = await this.productService.deleteProduct(productId);
      this.currentBusiness.set(business);
      this.syncProfileFormFromBusiness(business);
      await this.loadOrders();
      this.businessFeedback.set({ type: 'success', message: 'Producto eliminado del catalogo.' });
      if (this.editingProductId() === productId) this.resetProductForm();
    } catch (error) {
      this.businessFeedback.set({
        type: 'error',
        message: error instanceof Error ? error.message : 'No se pudo eliminar el producto.',
      });
    }
  }

  protected resetProductForm(form?: NgForm): void {
    this.editingProductId.set(null);
    this.productForm = this.buildEmptyProduct();
    form?.resetForm(this.productForm);
  }

  protected initials(value?: string | null): string {
    if (!value?.trim()) return 'LS';
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
      const business = await this.authService.getCurrentBusiness();
      this.currentBusiness.set(business);

      if (business) {
        this.syncProfileFormFromBusiness(business);
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
      this.ordersFeed.set(await this.orderService.getCurrentBusinessOrders());
    } catch {
      this.ordersFeed.set(null);
    }
  }

  private syncProfileFormFromBusiness(business: MarketplaceBusiness): void {
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

  private buildEmptyRegisterForm(): RegisterBusinessRequest {
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

  private buildEmptyProfileForm(): UpdateBusinessProfileRequest {
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

  private buildEmptyProduct(): UpsertBusinessProductRequest {
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
