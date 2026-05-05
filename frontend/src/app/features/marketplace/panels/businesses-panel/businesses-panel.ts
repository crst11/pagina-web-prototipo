import { Component, EventEmitter, Input, Output } from '@angular/core';

import { MarketplaceBusiness } from '../../../../core/models/marketplace.models';

@Component({
  selector: 'app-businesses-panel',
  templateUrl: './businesses-panel.html',
  styleUrl: './businesses-panel.css',
})
export class BusinessesPanel {
  @Input() businesses: MarketplaceBusiness[] = [];
  @Input() selectedBusiness: MarketplaceBusiness | null = null;

  @Output() previewBusiness = new EventEmitter<number>();
  @Output() openBusiness = new EventEmitter<number>();

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
