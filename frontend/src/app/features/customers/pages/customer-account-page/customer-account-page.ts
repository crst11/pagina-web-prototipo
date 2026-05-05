/*
 * customer-account-page.ts
 *
 * Componente que gestiona la sesion del comprador: registro, inicio de sesion,
 * visualizacion del historial de pedidos, edicion del perfil y eliminacion de cuenta.
 */

import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';

import { CustomerOrder, CustomerOrdersHistory, CustomerProfile, LoginCustomerRequest, RegisterCustomerRequest, UpdateCustomerProfileRequest } from '../../../../core/models/customer.models';
import { MarketplaceBusiness } from '../../../../core/models/marketplace.models';
import { validateRegistrationForm, validateProfileEditForm } from '../../../../core/validators/form.validators';
import { CartService } from '../../../../core/services/cart.service';
import { AuthService } from '../../../../core/services/auth.service';
import { CustomerService } from '../../../../core/services/customer.service';

@Component({
  selector: 'app-customer-account-page',
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './customer-account-page.html',
  styleUrl: './customer-account-page.css',
})
export class CustomerAccountPage implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly customerService = inject(CustomerService);
  private readonly cartService = inject(CartService);
  private readonly router = inject(Router);

  protected readonly completedOrders = computed(() =>
    (this.history()?.orders ?? []).filter((order) => !!order.paymentMethod),
  );
  protected readonly pendingItems = this.cartService.items;
  protected readonly pendingTotal = this.cartService.total;

  protected readonly authMode = signal<'register' | 'login'>('login');
  protected readonly currentCustomer = signal<CustomerProfile | null>(null);
  protected readonly currentBusiness = signal<MarketplaceBusiness | null>(null);
  protected readonly history = signal<CustomerOrdersHistory | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly feedback = signal<{ type: 'success' | 'error'; message: string } | null>(null);
  protected readonly anySessionActive = signal(false);
  protected readonly isEditingProfile = signal(false);
  protected readonly showDeleteModal = signal(false);

  protected registerForm: RegisterCustomerRequest = this.buildEmptyRegisterForm();
  protected loginForm: LoginCustomerRequest = { email: '', password: '' };
  protected editProfileForm: UpdateCustomerProfileRequest = this.buildEmptyEditForm();

  ngOnInit(): void {
    void this.loadSession();
  }

  protected startEditingProfile(): void {
    const customer = this.currentCustomer();
    if (!customer) return;
    this.editProfileForm = { fullName: customer.fullName, phone: customer.phone, city: customer.city, address: customer.address };
    this.isEditingProfile.set(true);
    this.feedback.set(null);
  }

  protected cancelEditingProfile(): void {
    this.isEditingProfile.set(false);
    this.feedback.set(null);
  }

  protected requestDeleteProfile(): void { this.showDeleteModal.set(true); }
  protected cancelDeleteProfile(): void { this.showDeleteModal.set(false); }
  protected removeFromPending(productId: number): void { this.cartService.removeFromCart(productId); }
  protected clearAllPending(): void { this.cartService.clear(); }

  protected async submitRegister(form: NgForm): Promise<void> {
    const validationError = validateRegistrationForm(this.registerForm);
    if (validationError) { this.feedback.set({ type: 'error', message: validationError }); return; }
    this.isLoading.set(true);
    this.feedback.set(null);
    try {
      const { token, profile: customer } = await this.customerService.registerCustomer(this.registerForm);
      this.currentCustomer.set(customer);
      this.anySessionActive.set(true);
      this.cartService.bindToCustomerSession(token);
      this.feedback.set({ type: 'success', message: 'Cuenta creada. Ya puedes comprar en LocalShop.' });
      this.registerForm = this.buildEmptyRegisterForm();
      form.resetForm(this.registerForm);
      await this.loadHistory();
    } catch (error) {
      this.feedback.set({ type: 'error', message: error instanceof Error ? error.message : 'No fue posible crear la cuenta.' });
    } finally { this.isLoading.set(false); }
  }

  protected async submitLogin(form: NgForm): Promise<void> {
    if (!this.loginForm.email.trim() || !this.loginForm.password) {
      this.feedback.set({ type: 'error', message: 'Ingresa el correo y la contrasena.' }); return;
    }
    this.isLoading.set(true);
    this.feedback.set(null);
    try {
      const { token, profile: customer } = await this.customerService.loginCustomer(this.loginForm);
      this.currentCustomer.set(customer);
      this.anySessionActive.set(true);
      this.cartService.bindToCustomerSession(token);
      this.feedback.set({ type: 'success', message: 'Sesion iniciada correctamente.' });
      this.loginForm = { email: '', password: '' };
      form.resetForm(this.loginForm);
      await this.loadHistory();
    } catch (error) {
      this.feedback.set({ type: 'error', message: error instanceof Error ? error.message : 'Correo o contrasena incorrectos.' });
    } finally { this.isLoading.set(false); }
  }

  protected async submitEditProfile(form: NgForm): Promise<void> {
    const validationError = validateProfileEditForm(this.editProfileForm);
    if (validationError) { this.feedback.set({ type: 'error', message: validationError }); return; }
    this.isLoading.set(true);
    this.feedback.set(null);
    try {
      const updatedCustomer = await this.customerService.updateCustomerProfile(this.editProfileForm);
      this.currentCustomer.set(updatedCustomer);
      this.isEditingProfile.set(false);
      this.feedback.set({ type: 'success', message: 'Perfil actualizado correctamente.' });
    } catch (error) {
      this.feedback.set({ type: 'error', message: error instanceof Error ? error.message : 'No fue posible guardar los cambios.' });
    } finally { this.isLoading.set(false); }
  }

  protected async confirmDeleteProfile(): Promise<void> {
    this.showDeleteModal.set(false);
    this.isLoading.set(true);
    try {
      await this.customerService.deleteCustomerProfile();
      this.cartService.unbindFromSession();
      this.currentCustomer.set(null);
      this.anySessionActive.set(!!this.currentBusiness());
      this.history.set(null);
      this.feedback.set({ type: 'success', message: 'La cuenta fue eliminada. Hasta pronto.' });
    } catch (error) {
      this.feedback.set({ type: 'error', message: error instanceof Error ? error.message : 'No fue posible eliminar la cuenta.' });
    } finally { this.isLoading.set(false); }
  }

  protected async logout(): Promise<void> {
    this.isLoading.set(true);
    try {
      await this.customerService.logoutCustomer();
      this.cartService.unbindFromSession();
      this.currentCustomer.set(null);
      this.anySessionActive.set(!!this.currentBusiness());
      this.history.set(null);
      this.feedback.set({ type: 'success', message: 'Sesion de cliente cerrada.' });
    } catch (error) {
      this.feedback.set({ type: 'error', message: error instanceof Error ? error.message : 'No fue posible cerrar la sesion.' });
    } finally { this.isLoading.set(false); }
  }

  protected async continuePurchase(order: CustomerOrder): Promise<void> {
    void this.router.navigate(['/carrito']);
  }

  protected showGoogleNotice(): void {
    this.feedback.set({ type: 'error', message: 'El inicio con Google se activa configurando el Client ID de OAuth en el proyecto.' });
  }

  private async loadSession(): Promise<void> {
    this.isLoading.set(true);
    try {
      const [customer, business] = await Promise.all([
        this.customerService.getCurrentCustomer(),
        this.authService.getCurrentBusiness(),
      ]);
      this.currentCustomer.set(customer);
      this.currentBusiness.set(business);
      this.anySessionActive.set(!!customer || !!business);
      if (customer) {
        const token = this.customerService.readCurrentCustomerToken();
        if (token) { this.cartService.bindToCustomerSession(token); }
        await this.loadHistory();
      }
    } catch (error) {
      this.feedback.set({ type: 'error', message: error instanceof Error ? error.message : 'No fue posible cargar la sesion.' });
    } finally { this.isLoading.set(false); }
  }

  private async loadHistory(): Promise<void> {
    try { this.history.set(await this.customerService.getCurrentCustomerOrders()); } catch { /* vacio */ }
  }

  private buildEmptyRegisterForm(): RegisterCustomerRequest {
    return { fullName: '', email: '', password: '', phone: '', city: '', address: '' };
  }

  private buildEmptyEditForm(): UpdateCustomerProfileRequest {
    return { fullName: '', phone: '', city: '', address: '' };
  }
}
