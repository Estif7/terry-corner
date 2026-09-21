import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Category, Product, Promotion } from '../models/menu.models';
import { MOCK_CATEGORIES, MOCK_PRODUCTS, MOCK_PROMOTIONS } from '../data/mock-menu.data';

@Injectable({ providedIn: 'root' })
export class MenuService {
  getCategories(): Observable<Category[]> {
    return of([...MOCK_CATEGORIES].sort((a, b) => a.sortOrder - b.sortOrder));
  }

  getProducts(params?: { categoryId?: string; search?: string; featuredOnly?: boolean }): Observable<Product[]> {
    let list = [...MOCK_PRODUCTS];

    if (params) {
      if (params.categoryId) {
        list = list.filter((p) => p.categoryId === params.categoryId);
      }
      if (params.featuredOnly) {
        list = list.filter((p) => p.isFeatured);
      }
      if (params.search) {
        const term = params.search.trim().toLowerCase();
        list = list.filter(
          (p) =>
            p.name.toLowerCase().includes(term) ||
            p.description.toLowerCase().includes(term) ||
            p.categoryName.toLowerCase().includes(term),
        );
      }
    }

    return of(list);
  }

  getProduct(productId: string): Observable<Product> {
    const product = MOCK_PRODUCTS.find((p) => p.id === productId) ?? MOCK_PRODUCTS[0];
    return of(product);
  }

  getActivePromotions(): Observable<Promotion[]> {
    return of(MOCK_PROMOTIONS);
  }
}
