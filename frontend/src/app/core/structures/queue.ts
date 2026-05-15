
export class Queue<T> {
  
  private readonly elements: T[] = [];

  enqueue(element: T): void {
    this.elements.push(element);
  }

  dequeue(): T | undefined {
    return this.elements.shift();
  }

  peek(): T | undefined {
    return this.elements[0];
  }

  clear(): void {
    this.elements.length = 0;
  }

  isEmpty(): boolean {
    return this.elements.length === 0;
  }

  get size(): number {
    return this.elements.length;
  }
}
