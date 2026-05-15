
export class Stack<T> {
  
  private readonly elements: T[] = [];

  push(element: T): void {
    this.elements.push(element);
  }

  pop(): T | undefined {
    return this.elements.pop();
  }

  peek(): T | undefined {
    return this.elements.at(-1);
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
