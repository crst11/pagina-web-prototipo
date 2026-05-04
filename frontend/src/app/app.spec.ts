import { TestBed } from '@angular/core/testing';
import { MarketplacePage } from './features/marketplace/pages/marketplace-page/marketplace-page';

describe('MarketplacePage', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MarketplacePage],
    }).compileComponents();
  });

  it('should create the marketplace page', () => {
    const fixture = TestBed.createComponent(MarketplacePage);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });
});
