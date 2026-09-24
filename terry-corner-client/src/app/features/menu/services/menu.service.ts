import { Injectable, inject } from '@angular/core';
import { Observable, catchError, from, map, of } from 'rxjs';
import { Category, Product, Promotion, Topping } from '../models/menu.models';
import { MOCK_CATEGORIES, MOCK_PRODUCTS, MOCK_PROMOTIONS } from '../data/mock-menu.data';
import { SupabaseService } from '../../../core/services/supabase.service';

@Injectable({ providedIn: 'root' })
export class MenuService {
  private readonly supabase = inject(SupabaseService);

  getCategories(): Observable<Category[]> {
    const client = this.supabase.client;
    if (!client) {
      return of([...MOCK_CATEGORIES].sort((a, b) => a.sortOrder - b.sortOrder));
    }

    return from(
      client
        .from('categories')
        .select('*')
        .eq('is_active', true)
        .order('sort_order', { ascending: true }),
    ).pipe(
      map(({ data, error }) => {
        if (error || !data || data.length === 0) {
          return [...MOCK_CATEGORIES].sort((a, b) => a.sortOrder - b.sortOrder);
        }
        return data.map((c: any) => ({
          id: c.id,
          name: c.name,
          description: c.description || '',
          imageUrl: c.image_url || '',
          sortOrder: c.sort_order || 0,
        }));
      }),
      catchError(() => of([...MOCK_CATEGORIES].sort((a, b) => a.sortOrder - b.sortOrder))),
    );
  }

  getProducts(params?: { categoryId?: string; search?: string; featuredOnly?: boolean }): Observable<Product[]> {
    const client = this.supabase.client;
    if (!client) {
      return of(this.filterMockProducts(params));
    }

    return from(
      client
        .from('products')
        .select(`
          id, name, description, price, image_url, category_id, is_available, is_featured, is_popular, sort_order,
          categories(name),
          product_toppings(toppings(id, name, additional_price, is_available))
        `)
        .order('sort_order', { ascending: true }),
    ).pipe(
      map(({ data, error }) => {
        if (error || !data || data.length === 0) {
          return this.filterMockProducts(params);
        }

        const list: Product[] = data.map((p: any) => {
          const toppings: Topping[] = (p.product_toppings || [])
            .map((pt: any) => {
              const t = pt.toppings;
              if (!t) return null;
              return {
                id: t.id,
                name: t.name,
                additionalPrice: Number(t.additional_price || 0),
                isAvailable: t.is_available ?? true,
              };
            })
            .filter((t: any): t is Topping => t !== null);

          return {
            id: p.id,
            name: p.name,
            description: p.description || '',
            price: Number(p.price || 0),
            imageUrl: p.image_url || '',
            categoryId: p.category_id || '',
            categoryName: p.categories?.name || '',
            isAvailable: p.is_available ?? true,
            isFeatured: p.is_featured ?? false,
            isPopular: p.is_popular ?? false,
            toppings,
          };
        });

        return this.filterList(list, params);
      }),
      catchError(() => of(this.filterMockProducts(params))),
    );
  }

  getProduct(productId: string): Observable<Product> {
    return this.getProducts().pipe(
      map((products) => products.find((p) => p.id === productId) ?? MOCK_PRODUCTS[0]),
    );
  }

  getActivePromotions(): Observable<Promotion[]> {
    const client = this.supabase.client;
    if (!client) {
      return of(MOCK_PROMOTIONS);
    }

    return from(
      client
        .from('promotions')
        .select('*')
        .eq('is_active', true),
    ).pipe(
      map(({ data, error }) => {
        if (error || !data || data.length === 0) {
          return MOCK_PROMOTIONS;
        }
        return data.map((pr: any) => ({
          id: pr.id,
          name: pr.name,
          description: pr.description || '',
          discountType: pr.discount_type || 'FixedAmount',
          discountValue: Number(pr.discount_value || 0),
          isFeatured: pr.is_featured ?? true,
          productIds: [],
        }));
      }),
      catchError(() => of(MOCK_PROMOTIONS)),
    );
  }

  private filterMockProducts(params?: { categoryId?: string; search?: string; featuredOnly?: boolean }): Product[] {
    return this.filterList([...MOCK_PRODUCTS], params);
  }

  private filterList(list: Product[], params?: { categoryId?: string; search?: string; featuredOnly?: boolean }): Product[] {
    if (!params) return list;

    let result = list;
    if (params.categoryId) {
      result = result.filter((p) => p.categoryId === params.categoryId);
    }
    if (params.featuredOnly) {
      result = result.filter((p) => p.isFeatured);
    }
    if (params.search) {
      const term = params.search.trim().toLowerCase();
      result = result.filter(
        (p) =>
          p.name.toLowerCase().includes(term) ||
          p.description.toLowerCase().includes(term) ||
          p.categoryName.toLowerCase().includes(term),
      );
    }
    return result;
  }
}
