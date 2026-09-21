import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ProductCardComponent } from '../../../../shared/components/product-card/product-card.component';
import { ProductModalComponent } from '../../../menu/components/product-modal/product-modal.component';
import { MenuService } from '../../../menu/services/menu.service';
import { Product, Promotion } from '../../../menu/models/menu.models';

@Component({
  selector: 'tc-home-page',
  standalone: true,
  imports: [RouterLink, ProductCardComponent, ProductModalComponent],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss',
})
export class HomePageComponent implements OnInit {
  private readonly menuService = inject(MenuService);
  private readonly router = inject(Router);

  readonly featuredProducts = signal<Product[]>([]);
  readonly featuredPromotion = signal<Promotion | null>(null);
  readonly activeProduct = signal<Product | null>(null);

  ngOnInit(): void {
    this.menuService.getProducts({ featuredOnly: true }).subscribe({
      next: (products) => this.featuredProducts.set(products),
      error: () => {},
    });

    this.menuService.getActivePromotions().subscribe({
      next: (promotions) => this.featuredPromotion.set(promotions.find((p) => p.isFeatured) ?? promotions[0] ?? null),
      error: () => {},
    });
  }

  openProduct(product: Product): void {
    this.activeProduct.set(product);
  }

  openDeal(promo: Promotion): void {
    const targetId = promo.productIds?.[0] ?? 'prod-special-burger';
    this.menuService.getProduct(targetId).subscribe({
      next: (product) => {
        if (product) {
          this.activeProduct.set(product);
        }
      },
    });
  }

  closeModal(): void {
    this.activeProduct.set(null);
  }

  goToMenu(): void {
    this.router.navigate(['/menu']);
  }
}
