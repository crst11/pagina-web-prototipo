import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import {
  AuthResponseApi,
  BusinessOrdersFeed,
  CheckoutCartRequest,
  CheckoutCartResponse,
  CreateShipmentRequest,
  MarketplaceBusiness,
  MarketplaceSnapshot,
  OrderCreatedResponse,
  RegisterBusinessRequest,
  LoginBusinessRequest,
  UpdateBusinessProfileRequest,
  UpsertBusinessProductRequest,
} from '../models/store.models';

@Injectable({
  providedIn: 'root',
})
export class StoreService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = '/api';
  private readonly sessionKey = 'marketplace_owner_token';

  async getMarketplaceSnapshot(): Promise<MarketplaceSnapshot> {
    try {
      return await firstValueFrom(this.http.get<MarketplaceSnapshot>(`${this.apiBaseUrl}/store/overview`));
    } catch (error) {
      throw this.toError(error, 'No se pudo cargar la vitrina empresarial.');
    }
  }

  async getCurrentBusiness(): Promise<MarketplaceBusiness | null> {
    const token = this.readToken();
    if (!token) {
      return null;
    }

    try {
      return await firstValueFrom(
        this.http.get<MarketplaceBusiness>(`${this.apiBaseUrl}/auth/me`, {
          headers: this.buildAuthHeaders(token),
        }),
      );
    } catch (error) {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        this.clearToken();
        return null;
      }

      throw this.toError(error, 'No se pudo recuperar la sesion empresarial.');
    }
  }

  async registerBusiness(payload: RegisterBusinessRequest): Promise<MarketplaceBusiness> {
    try {
      const response = await firstValueFrom(
        this.http.post<AuthResponseApi>(`${this.apiBaseUrl}/auth/register`, payload),
      );
      this.writeToken(response.token);
      return response.business;
    } catch (error) {
      throw this.toError(error, 'No se pudo registrar la empresa.');
    }
  }

  async loginBusiness(payload: LoginBusinessRequest): Promise<MarketplaceBusiness> {
    try {
      const response = await firstValueFrom(this.http.post<AuthResponseApi>(`${this.apiBaseUrl}/auth/login`, payload));
      this.writeToken(response.token);
      return response.business;
    } catch (error) {
      throw this.toError(error, 'No se pudo iniciar sesion.');
    }
  }

  async logoutBusiness(): Promise<void> {
    const token = this.readToken();

    try {
      if (token) {
        await firstValueFrom(
          this.http.post<void>(
            `${this.apiBaseUrl}/auth/logout`,
            {},
            {
              headers: this.buildAuthHeaders(token),
            },
          ),
        );
      }
    } catch (error) {
      throw this.toError(error, 'No se pudo cerrar la sesion empresarial.');
    } finally {
      this.clearToken();
    }
  }

  async updateCurrentBusiness(payload: UpdateBusinessProfileRequest): Promise<MarketplaceBusiness> {
    const token = this.requireToken();

    try {
      return await firstValueFrom(
        this.http.put<MarketplaceBusiness>(`${this.apiBaseUrl}/auth/me`, payload, {
          headers: this.buildAuthHeaders(token),
        }),
      );
    } catch (error) {
      throw this.toError(error, 'No se pudo guardar el perfil empresarial.');
    }
  }

  async saveProduct(payload: UpsertBusinessProductRequest, productId?: number): Promise<MarketplaceBusiness> {
    const token = this.requireToken();
    const request = productId
      ? this.http.put(`${this.apiBaseUrl}/admin/products/${productId}`, payload, {
          headers: this.buildAuthHeaders(token),
        })
      : this.http.post(`${this.apiBaseUrl}/admin/products`, payload, {
          headers: this.buildAuthHeaders(token),
        });

    try {
      await firstValueFrom(request);
      return await this.requireCurrentBusiness();
    } catch (error) {
      throw this.toError(error, 'No se pudo guardar el producto.');
    }
  }

  async deleteProduct(productId: number): Promise<MarketplaceBusiness> {
    const token = this.requireToken();

    try {
      await firstValueFrom(
        this.http.delete<void>(`${this.apiBaseUrl}/admin/products/${productId}`, {
          headers: this.buildAuthHeaders(token),
        }),
      );
      return await this.requireCurrentBusiness();
    } catch (error) {
      throw this.toError(error, 'No se pudo eliminar el producto.');
    }
  }

  async createOrder(payload: CreateShipmentRequest): Promise<OrderCreatedResponse> {
    try {
      return await firstValueFrom(
        this.http.post<OrderCreatedResponse>(`${this.apiBaseUrl}/orders`, payload),
      );
    } catch (error) {
      throw this.toError(error, 'No se pudo crear el pedido.');
    }
  }

  async checkoutCart(payload: CheckoutCartRequest): Promise<CheckoutCartResponse> {
    try {
      return await firstValueFrom(
        this.http.post<CheckoutCartResponse>(`${this.apiBaseUrl}/orders/checkout`, payload),
      );
    } catch (error) {
      throw this.toError(error, 'No se pudo registrar la compra del carrito.');
    }
  }

  async getCurrentBusinessOrders(): Promise<BusinessOrdersFeed> {
    const token = this.requireToken();

    try {
      return await firstValueFrom(
        this.http.get<BusinessOrdersFeed>(`${this.apiBaseUrl}/admin/orders`, {
          headers: this.buildAuthHeaders(token),
        }),
      );
    } catch (error) {
      throw this.toError(error, 'No se pudieron cargar los pedidos de la empresa.');
    }
  }

  private buildAuthHeaders(token: string): HttpHeaders {
    return new HttpHeaders({
      'X-Owner-Token': token,
    });
  }

  private readToken(): string | null {
    if (typeof localStorage === 'undefined') {
      return null;
    }

    return localStorage.getItem(this.sessionKey);
  }

  private writeToken(token: string): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(this.sessionKey, token);
    }
  }

  private clearToken(): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem(this.sessionKey);
    }
  }

  private requireToken(): string {
    const token = this.readToken();
    if (!token) {
      throw new Error('Debes iniciar sesion como empresa para continuar.');
    }

    return token;
  }

  private async requireCurrentBusiness(): Promise<MarketplaceBusiness> {
    const business = await this.getCurrentBusiness();
    if (!business) {
      throw new Error('La sesion empresarial no esta disponible. Inicia sesion nuevamente.');
    }

    return business;
  }

  private toError(error: unknown, fallback: string): Error {
    if (error instanceof Error && !(error instanceof HttpErrorResponse)) {
      return error;
    }

    if (error instanceof HttpErrorResponse) {
      const message =
        (typeof error.error === 'object' && error.error && 'message' in error.error
          ? String((error.error as { message?: unknown }).message)
          : undefined) ?? fallback;

      if (error.status === 401) {
        this.clearToken();
      }

      return new Error(message);
    }

    return new Error(fallback);
  }
}
