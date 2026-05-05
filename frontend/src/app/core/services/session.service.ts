import { Injectable } from '@angular/core';

// Claves de localStorage usadas para las sesiones.
// Se definen aqui para que cualquier servicio que las necesite las importe de un solo lugar.
const OWNER_TOKEN_KEY = 'marketplace_owner_token';
const CUSTOMER_TOKEN_KEY = 'marketplace_customer_token';

@Injectable({ providedIn: 'root' })
export class SessionService {
  readOwnerToken(): string | null {
    return this.read(OWNER_TOKEN_KEY);
  }

  writeOwnerToken(token: string): void {
    this.write(OWNER_TOKEN_KEY, token);
  }

  clearOwnerToken(): void {
    this.remove(OWNER_TOKEN_KEY);
  }

  readCustomerToken(): string | null {
    return this.read(CUSTOMER_TOKEN_KEY);
  }

  writeCustomerToken(token: string): void {
    this.write(CUSTOMER_TOKEN_KEY, token);
  }

  clearCustomerToken(): void {
    this.remove(CUSTOMER_TOKEN_KEY);
  }

  hasOwnerSession(): boolean {
    return !!this.readOwnerToken();
  }

  hasCustomerSession(): boolean {
    return !!this.readCustomerToken();
  }

  private read(key: string): string | null {
    try {
      return localStorage.getItem(key);
    } catch {
      return null;
    }
  }

  private write(key: string, value: string): void {
    try {
      localStorage.setItem(key, value);
    } catch {
      // localStorage no disponible, se ignora.
    }
  }

  private remove(key: string): void {
    try {
      localStorage.removeItem(key);
    } catch {
      // localStorage no disponible, se ignora.
    }
  }
}
