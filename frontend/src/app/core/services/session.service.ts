import { Injectable } from '@angular/core';

/**
 * Claves de `localStorage` usadas para persistir las sesiones de usuario.
 *
 * Se definen como constantes de módulo para que cualquier servicio que las
 * necesite las importe desde un único lugar y no haya cadenas duplicadas.
 */
const OWNER_TOKEN_KEY = 'marketplace_owner_token';
const CUSTOMER_TOKEN_KEY = 'marketplace_customer_token';

/**
 * Servicio de gestión de sesiones mediante `localStorage`.
 *
 * Abstrae la lectura y escritura de los tokens de sesión tanto para
 * empresarios (owners) como para clientes (customers). Centraliza las
 * claves de storage y maneja errores de acceso de forma silenciosa.
 *
 * ## Arquitectura MVC
 * Este servicio actúa como capa de **Modelo** para el estado de sesión:
 * - No contiene lógica de negocio ni navegación.
 * - Es consumido por `AuthService` y `CustomerService` (capa de servicios).
 * - Los componentes nunca acceden directamente al `localStorage`.
 *
 * @injectable Registrado globalmente (`providedIn: 'root'`).
 */
@Injectable({ providedIn: 'root' })
export class SessionService {

  // ─── Sesión Empresarial (Owner) ───────────────────────────────────────────

  /**
   * Lee el token de sesión del empresario desde `localStorage`.
   * @returns El token si existe, o `null` si no hay sesión activa.
   */
  readOwnerToken(): string | null {
    return this.read(OWNER_TOKEN_KEY);
  }

  /**
   * Persiste el token de sesión del empresario en `localStorage`.
   * @param token Token recibido del backend tras autenticarse.
   */
  writeOwnerToken(token: string): void {
    this.write(OWNER_TOKEN_KEY, token);
  }

  /**
   * Elimina el token del empresario de `localStorage` (cierra sesión).
   */
  clearOwnerToken(): void {
    this.remove(OWNER_TOKEN_KEY);
  }

  /**
   * Indica si hay una sesión de empresario activa.
   * @returns `true` si el token existe, `false` en caso contrario.
   */
  hasOwnerSession(): boolean {
    return !!this.readOwnerToken();
  }

  // ─── Sesión de Cliente (Customer) ─────────────────────────────────────────

  /**
   * Lee el token de sesión del cliente desde `localStorage`.
   * @returns El token si existe, o `null` si no hay sesión activa.
   */
  readCustomerToken(): string | null {
    return this.read(CUSTOMER_TOKEN_KEY);
  }

  /**
   * Persiste el token de sesión del cliente en `localStorage`.
   * @param token Token recibido del backend tras autenticarse.
   */
  writeCustomerToken(token: string): void {
    this.write(CUSTOMER_TOKEN_KEY, token);
  }

  /**
   * Elimina el token del cliente de `localStorage` (cierra sesión).
   */
  clearCustomerToken(): void {
    this.remove(CUSTOMER_TOKEN_KEY);
  }

  /**
   * Indica si hay una sesión de cliente activa.
   * @returns `true` si el token existe, `false` en caso contrario.
   */
  hasCustomerSession(): boolean {
    return !!this.readCustomerToken();
  }

  // ─── Métodos Privados ─────────────────────────────────────────────────────

  /** Lee un valor de `localStorage`. Retorna `null` si no está disponible. */
  private read(key: string): string | null {
    try {
      return localStorage.getItem(key);
    } catch {
      return null;
    }
  }

  /** Escribe un valor en `localStorage`. Silencia errores de acceso. */
  private write(key: string, value: string): void {
    try {
      localStorage.setItem(key, value);
    } catch {
      // localStorage no disponible (navegadores en modo privado o bloqueado).
    }
  }

  /** Elimina un valor de `localStorage`. Silencia errores de acceso. */
  private remove(key: string): void {
    try {
      localStorage.removeItem(key);
    } catch {
      // localStorage no disponible, se ignora.
    }
  }
}
