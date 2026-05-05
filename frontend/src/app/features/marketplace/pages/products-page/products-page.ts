import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { BusinessProduct } from '../../../../core/models/marketplace.models';
import { ProductsPanel } from '../../panels/products-panel/products-panel';
import { MarketplacePageState } from '../../services/marketplace-page-state.service';

@Component({
  selector: 'app-products-page',
  imports: [CommonModule, RouterLink, RouterLinkActive, ProductsPanel],
  templateUrl: './products-page.html',
  styleUrl: './products-page.css',
})
export class ProductsPage implements OnInit {
  protected readonly state = inject(MarketplacePageState);
  protected readonly currentBusiness = this.state.currentBusiness;
  protected readonly currentCustomer = this.state.currentCustomer;
  protected readonly featuredProducts = this.state.featuredProducts;
  protected readonly pageFeedback = this.state.pageFeedback;
  protected readonly cartCount = this.state.cartCount;
  protected readonly cartNotice = this.state.cartNotice;
  protected readonly cartPulse = this.state.cartPulse;
  protected readonly isAnyAuthenticated = this.state.isAnyAuthenticated;

  ngOnInit(): void {
    void this.state.initialize();
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
