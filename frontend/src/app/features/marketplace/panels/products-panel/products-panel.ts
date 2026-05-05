import { CurrencyPipe } from '@angular/common';
import { Component, EventEmitter, Input, Output, signal } from '@angular/core';

import { BusinessProduct, FeaturedProduct } from '../../../../core/models/marketplace.models';

@Component({
  selector: 'app-products-panel',
  imports: [CurrencyPipe],
  templateUrl: './products-panel.html',
  styleUrl: './products-panel.css',
})
export class ProductsPanel {
  @Input() featuredProducts: FeaturedProduct[] = [];

  @Output() openProductProfile = new EventEmitter<BusinessProduct>();
  @Output() addToCart = new EventEmitter<BusinessProduct>();

  protected readonly expandedProductId = signal<number | null>(null);

  protected isProductDescriptionExpanded(productId: number): boolean {
    return this.expandedProductId() === productId;
  }

  protected hasLongDescription(description: string): boolean {
    return description.length > 120;
  }

  protected productSummary(description: string, productId: number): string {
    if (this.isProductDescriptionExpanded(productId) || description.length <= 86) {
      return description;
    }

    return `${description.slice(0, 86).trimEnd()}...`;
  }

  protected toggleProductDescription(productId: number): void {
    this.expandedProductId.update((currentId) => (currentId === productId ? null : productId));
  }
}
