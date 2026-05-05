import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { BusinessesPanel } from '../../panels/businesses-panel/businesses-panel';
import { MarketplacePageState } from '../../services/marketplace-page-state.service';

@Component({
  selector: 'app-businesses-page',
  imports: [CommonModule, RouterLink, RouterLinkActive, BusinessesPanel],
  templateUrl: './businesses-page.html',
  styleUrl: './businesses-page.css',
})
export class BusinessesPage implements OnInit {
  protected readonly state = inject(MarketplacePageState);
  protected readonly currentBusiness = this.state.currentBusiness;
  protected readonly currentCustomer = this.state.currentCustomer;
  protected readonly businesses = this.state.businesses;
  protected readonly selectedBusiness = this.state.selectedBusiness;
  protected readonly pageFeedback = this.state.pageFeedback;
  protected readonly cartCount = this.state.cartCount;
  protected readonly cartPulse = this.state.cartPulse;
  protected readonly isAnyAuthenticated = this.state.isAnyAuthenticated;

  ngOnInit(): void {
    void this.state.initialize();
  }

  protected previewBusiness(businessId: number): void {
    this.state.previewBusiness(businessId);
  }

  protected openBusinessProfileById(businessId: number): void {
    this.state.openBusinessProfileById(businessId);
  }

  protected openCurrentCartBusiness(): void {
    this.state.openCurrentCartBusiness();
  }
}
