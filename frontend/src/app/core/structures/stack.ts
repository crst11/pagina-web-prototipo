/**
 * Estructura de datos Pila (Stack) — LIFO (Last In, First Out).
 *
 * Usada en el carrito de compras para registrar el historial de acciones
 * del usuario (agregar / quitar productos) y permitir deshacer operaciones.
 *
 * Principio LIFO: el último elemento insertado es el primero en salir.
 *
 * @template T Tipo de los elementos almacenados en la pila.
 *
 * @example
 * const historial = new Stack<string>();
 * historial.push('producto-agregado');
 * historial.push('producto-eliminado');
 * historial.pop();  // 'producto-eliminado'
 * historial.peek(); // 'producto-agregado'
 */
export class Stack<T> {
  /** Arreglo interno que contiene los elementos de la pila. */
  private readonly elements: T[] = [];

  /**
   * Inserta un elemento en la cima de la pila.
   * @param element Elemento a insertar.
   */
  push(element: T): void {
    this.elements.push(element);
  }

  /**
   * Extrae y retorna el elemento en la cima de la pila.
   * @returns El elemento extraído, o `undefined` si la pila está vacía.
   */
  pop(): T | undefined {
    return this.elements.pop();
  }

  /**
   * Consulta el elemento en la cima sin extraerlo.
   * @returns El elemento en la cima, o `undefined` si la pila está vacía.
   */
  peek(): T | undefined {
    return this.elements.at(-1);
  }

  /** Elimina todos los elementos de la pila. */
  clear(): void {
    this.elements.length = 0;
  }

  /**
   * Indica si la pila no contiene elementos.
   * @returns `true` si está vacía, `false` en caso contrario.
   */
  isEmpty(): boolean {
    return this.elements.length === 0;
  }

  /** Número de elementos actualmente en la pila. */
  get size(): number {
    return this.elements.length;
  }
}
