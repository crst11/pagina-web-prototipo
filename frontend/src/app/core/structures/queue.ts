/**
 * Estructura de datos Cola (Queue) — FIFO (First In, First Out).
 *
 * Usada en el sistema de notificaciones del marketplace para gestionar
 * mensajes de retroalimentación al usuario (éxito, error, información)
 * en orden de llegada.
 *
 * Principio FIFO: el primer elemento insertado es el primero en salir.
 *
 * @template T Tipo de los elementos almacenados en la cola.
 *
 * @example
 * const notificaciones = new Queue<string>();
 * notificaciones.enqueue('Producto agregado al carrito');
 * notificaciones.enqueue('Stock actualizado');
 * notificaciones.dequeue(); // 'Producto agregado al carrito'
 * notificaciones.peek();    // 'Stock actualizado'
 */
export class Queue<T> {
  /** Arreglo interno que contiene los elementos de la cola. */
  private readonly elements: T[] = [];

  /**
   * Agrega un elemento al final de la cola.
   * @param element Elemento a encolar.
   */
  enqueue(element: T): void {
    this.elements.push(element);
  }

  /**
   * Extrae y retorna el elemento al frente de la cola.
   * @returns El elemento extraído, o `undefined` si la cola está vacía.
   */
  dequeue(): T | undefined {
    return this.elements.shift();
  }

  /**
   * Consulta el elemento al frente sin extraerlo.
   * @returns El primer elemento, o `undefined` si la cola está vacía.
   */
  peek(): T | undefined {
    return this.elements[0];
  }

  /** Elimina todos los elementos de la cola. */
  clear(): void {
    this.elements.length = 0;
  }

  /**
   * Indica si la cola no contiene elementos.
   * @returns `true` si está vacía, `false` en caso contrario.
   */
  isEmpty(): boolean {
    return this.elements.length === 0;
  }

  /** Número de elementos actualmente en la cola. */
  get size(): number {
    return this.elements.length;
  }
}
