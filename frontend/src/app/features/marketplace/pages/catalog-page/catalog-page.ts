import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { BusinessProduct } from '../../../../core/models/marketplace.models';
import { CatalogPanel } from '../../panels/catalog-panel/catalog-panel';
import { MarketplacePageState } from '../../services/marketplace-page-state.service';

@Component({
  selector: 'app-catalog-page',
  imports: [CommonModule, RouterLink, RouterLinkActive, CatalogPanel],
  templateUrl: './catalog-page.html',
  styleUrl: './catalog-page.css',
})
export class CatalogPage implements OnInit {
  protected readonly state = inject(MarketplacePageState);
  protected readonly currentBusiness = this.state.currentBusiness;
  protected readonly currentCustomer = this.state.currentCustomer;
  protected readonly filteredProducts = this.state.filteredProducts;
  protected readonly availableCategories = this.state.availableCategories;
  protected readonly activeCategory = this.state.activeCategory;
  protected readonly pageFeedback = this.state.pageFeedback;
  protected readonly cartCount = this.state.cartCount;
  protected readonly cartNotice = this.state.cartNotice;
  protected readonly cartPulse = this.state.cartPulse;
  protected readonly isAnyAuthenticated = this.state.isAnyAuthenticated;

  ngOnInit(): void {
    void this.state.initialize();
  }

  protected setCategory(category: string): void {
    this.state.setCategory(category);
  }

  protected openProductProfile(product: BusinessProduct): void {
    this.state.openProductProfile(product);
  }

  protected addToCart(product: BusinessProduct): void {
    this.state.addToCart(product);
  }

  protected openCurrentCartBusiness(): void {
    this.state.openCurrentCartBusiness();
  }
}
