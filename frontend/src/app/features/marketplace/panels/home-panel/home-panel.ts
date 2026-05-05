import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { CustomerProfile } from '../../../../core/models/customer.models';
import { MarketplaceBusiness } from '../../../../core/models/marketplace.models';

@Component({
  selector: 'app-home-panel',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './home-panel.html',
  styleUrl: './home-panel.css',
})
export class HomePanel {
  @Input() selectedBusiness: MarketplaceBusiness | null = null;
  @Input() currentBusiness: MarketplaceBusiness | null = null;
  @Input() currentCustomer: CustomerProfile | null = null;
  @Input() searchTerm = '';
  @Input() totalBusinesses = 0;
  @Input() totalProducts = 0;
  @Input() isAuthenticated = false;
  @Input() isAnyAuthenticated = false;
  @Input() cartFeedback: { type: 'success' | 'error'; message: string } | null = null;

  @Output() searchTermChange = new EventEmitter<string>();
  @Output() openBusiness = new EventEmitter<number>();
  @Output() openPortal = new EventEmitter<void>();

  protected initials(value?: string | null): string {
    if (!value?.trim()) {
      return 'LS';
    }

    return value
      .trim()
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part.charAt(0).toUpperCase())
      .join('');
  }
}
