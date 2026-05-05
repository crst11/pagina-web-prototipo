import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { BusinessProduct } from '../../../../core/models/marketplace.models';
import { HomePanel } from '../../panels/home-panel/home-panel';
import { MarketplacePageState } from '../../services/marketplace-page-state.service';

/**
 * Vitrina publica principal de LocalShop.
 * Centraliza:
 * - carga del marketplace,
 * - filtros de busqueda y categorias,
 * - acceso a perfiles empresariales,
 * - estado visual del carrito global.
 */
@Component({
  selector: 'app-marketplace-page',
  imports: [CommonModule, RouterLink, RouterLinkActive, HomePanel],
  templateUrl: './marketplace-page.html',
  styleUrl: './marketplace-page.css',
})
export class MarketplacePage implements OnInit {
  protected readonly state = inject(MarketplacePageState);
  protected readonly currentBusiness = this.state.currentBusiness;
  protected readonly currentCustomer = this.state.currentCustomer;
  protected readonly selectedBusiness = this.state.selectedBusiness;
  protected readonly totalBusinesses = this.state.totalBusinesses;
  protected readonly totalProducts = this.state.totalProducts;
  protected readonly searchTerm = this.state.searchTerm;
  protected readonly pageFeedback = this.state.pageFeedback;
  protected readonly cartFeedback = this.state.cartFeedback;
  protected readonly cartCount = this.state.cartCount;
  protected readonly cartNotice = this.state.cartNotice;
  protected readonly cartPulse = this.state.cartPulse;
  protected readonly isAuthenticated = this.state.isAuthenticated;
  protected readonly isAnyAuthenticated = this.state.isAnyAuthenticated;
  protected readonly currentYear = new Date().getFullYear();

  ngOnInit(): void {
    void this.state.initialize();
  }

  protected setSearchTerm(value: string): void {
    this.state.setSearchTerm(value);
  }

  protected openBusinessProfileById(businessId: number): void {
    this.state.openBusinessProfileById(businessId);
  }

  protected openProductProfile(product: BusinessProduct): void {
    this.state.openProductProfile(product);
  }

  protected openCurrentCartBusiness(): void {
    this.state.openCurrentCartBusiness();
  }

  protected openPortalPage(): void {
    this.state.openPortalPage();
  }

  protected openAccountPage(): void {
    this.state.openAccountPage();
  }

  protected addToCart(product: BusinessProduct): void {
    this.state.addToCart(product);
  }
}
