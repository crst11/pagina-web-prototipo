import { CurrencyPipe } from '@angular/common';
import { Component, EventEmitter, Input, Output, signal } from '@angular/core';

import { BusinessProduct } from '../../../../core/models/marketplace.models';

export type CatalogPanelItem = BusinessProduct & {
  businessName: string;
  businessSlug?: string;
  city: string;
};

@Component({
  selector: 'app-catalog-panel',
  imports: [CurrencyPipe],
  templateUrl: './catalog-panel.html',
  styleUrl: './catalog-panel.css',
})
export class CatalogPanel {
  @Input() filteredProducts: CatalogPanelItem[] = [];
  @Input() availableCategories: string[] = [];
  @Input() activeCategory = 'Todas';

  @Output() categoryChange = new EventEmitter<string>();
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
