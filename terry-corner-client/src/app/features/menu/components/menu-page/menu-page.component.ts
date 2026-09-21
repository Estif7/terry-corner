import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ProductCardComponent } from '../../../../shared/components/product-card/product-card.component';
import { ProductModalComponent } from '../product-modal/product-modal.component';
import { MenuService } from '../../services/menu.service';
import { Category, Product } from '../../models/menu.models';

type SortOption = 'default' | 'price-asc' | 'price-desc' | 'name-asc';

@Component({
  selector: 'tc-menu-page',
  standalone: true,
  imports: [FormsModule, ProductCardComponent, ProductModalComponent],
  templateUrl: './menu-page.component.html',
  styleUrl: './menu-page.component.scss',
})
export class MenuPageComponent implements OnInit {
  private readonly menuService = inject(MenuService);

  readonly categories = signal<Category[]>([]);
  readonly products = signal<Product[]>([]);
  readonly loading = signal(true);
  readonly loadError = signal(false);

  readonly selectedCategoryId = signal<string | null>(null);
  readonly searchTerm = signal('');
  readonly sortOption = signal<SortOption>('default');
  readonly activeProduct = signal<Product | null>(null);

  readonly filteredProducts = computed(() => {
    let list = this.products();

    const categoryId = this.selectedCategoryId();
    if (categoryId) {
      list = list.filter((p) => p.categoryId === categoryId);
    }

    const term = this.searchTerm().trim().toLowerCase();
    if (term) {
      list = list.filter(
        (p) =>
          p.name.toLowerCase().includes(term) ||
          p.description.toLowerCase().includes(term) ||
          p.categoryName.toLowerCase().includes(term),
      );
    }

    switch (this.sortOption()) {
      case 'price-asc':
        return [...list].sort((a, b) => a.price - b.price);
      case 'price-desc':
        return [...list].sort((a, b) => b.price - a.price);
      case 'name-asc':
        return [...list].sort((a, b) => a.name.localeCompare(b.name));
      default:
        return list;
    }
  });

  readonly categoryCounts = computed(() => {
    const counts = new Map<string, number>();
    for (const p of this.products()) {
      counts.set(p.categoryId, (counts.get(p.categoryId) ?? 0) + 1);
    }
    return counts;
  });

  ngOnInit(): void {
    this.menuService.getCategories().subscribe({
      next: (categories) => this.categories.set(categories),
      error: () => {},
    });

    this.menuService.getProducts().subscribe({
      next: (products) => {
        this.products.set(products);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.loadError.set(true);
      },
    });
  }

  selectCategory(categoryId: string | null): void {
    this.selectedCategoryId.set(categoryId);
  }

  clearSearch(): void {
    this.searchTerm.set('');
  }

  openProduct(product: Product): void {
    this.activeProduct.set(product);
  }

  closeModal(): void {
    this.activeProduct.set(null);
  }
}
