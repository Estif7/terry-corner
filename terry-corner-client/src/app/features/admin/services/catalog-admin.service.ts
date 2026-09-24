import { Injectable, inject } from '@angular/core';
import { Observable, from, map, of, throwError } from 'rxjs';
import { SupabaseService } from '../../../core/services/supabase.service';
import { ToastService } from '../../../core/services/toast.service';
import { AdminCategory, AdminPaymentMethod, AdminProduct, AdminPromotion, AdminTopping } from '../models/admin.models';
import { MOCK_CATEGORIES, MOCK_TOPPINGS, MOCK_PROMOTIONS } from '../../menu/data/mock-menu.data';

@Injectable({ providedIn: 'root' })
export class CatalogAdminService {
  private readonly supabase = inject(SupabaseService);
  private readonly toast = inject(ToastService);

  private get client() {
    return this.supabase.client;
  }

  // ---- Images ----------------------------------------------------------------------------
  uploadImage(file: File): Observable<{ url: string }> {
    if (!this.client) {
      this.toast.error('Supabase is not configured yet.');
      return throwError(() => new Error('Supabase not configured'));
    }

    const ext = file.name.split('.').pop() || 'png';
    const cleanName = file.name.replace(/[^a-zA-Z0-9]/g, '_').toLowerCase();
    const fileName = `${Date.now()}_${cleanName}.${ext}`;

    return from(
      this.client.storage.from('meals').upload(fileName, file, {
        cacheControl: '3600',
        upsert: true,
      }),
    ).pipe(
      map(({ data, error }) => {
        if (error || !data) {
          const msg = error?.message || 'Failed to upload image to Supabase Storage.';
          this.toast.error(msg);
          throw new Error(msg);
        }

        const { data: urlData } = this.client!.storage.from('meals').getPublicUrl(fileName);
        this.toast.success('Image uploaded successfully!');
        return { url: urlData.publicUrl };
      }),
    );
  }

  // ---- Categories ----------------------------------------------------------------
  getCategories(): Observable<AdminCategory[]> {
    if (!this.client) {
      return of(
        MOCK_CATEGORIES.map((c) => ({
          id: c.id,
          name: c.name,
          description: c.description,
          imageUrl: c.imageUrl,
          sortOrder: c.sortOrder,
          isActive: true,
        })),
      );
    }

    return from(
      this.client.from('categories').select('*').order('sort_order', { ascending: true }),
    ).pipe(
      map(({ data, error }) => {
        if (error || !data || data.length === 0) {
          return MOCK_CATEGORIES.map((c) => ({
            id: c.id,
            name: c.name,
            description: c.description,
            imageUrl: c.imageUrl,
            sortOrder: c.sortOrder,
            isActive: true,
          }));
        }
        return data.map((c: any) => ({
          id: c.id,
          name: c.name,
          description: c.description || '',
          imageUrl: c.image_url || '',
          sortOrder: c.sort_order || 0,
          isActive: c.is_active ?? true,
        }));
      }),
    );
  }

  createCategory(payload: Omit<AdminCategory, 'id' | 'isActive'>): Observable<AdminCategory> {
    if (!this.client) return throwError(() => new Error('Supabase not configured'));

    const id = 'cat-' + Date.now();
    return from(
      this.client
        .from('categories')
        .insert({
          id,
          name: payload.name.trim(),
          description: payload.description || '',
          image_url: payload.imageUrl || '',
          sort_order: payload.sortOrder || 0,
          is_active: true,
        })
        .select()
        .single(),
    ).pipe(
      map(({ data, error }) => {
        if (error) {
          this.toast.error(error.message);
          throw error;
        }
        this.toast.success(`Category "${payload.name}" created!`);
        return {
          id: data.id,
          name: data.name,
          description: data.description,
          imageUrl: data.image_url,
          sortOrder: data.sort_order,
          isActive: data.is_active,
        };
      }),
    );
  }

  updateCategory(id: string, payload: Omit<AdminCategory, 'id'>): Observable<void> {
    if (!this.client) return throwError(() => new Error('Supabase not configured'));

    return from(
      this.client
        .from('categories')
        .update({
          name: payload.name.trim(),
          description: payload.description || '',
          image_url: payload.imageUrl || '',
          sort_order: payload.sortOrder || 0,
          is_active: payload.isActive,
        })
        .eq('id', id),
    ).pipe(
      map(({ error }) => {
        if (error) {
          this.toast.error(error.message);
          throw error;
        }
        this.toast.success(`Category updated!`);
      }),
    );
  }

  deleteCategory(id: string): Observable<void> {
    if (!this.client) return throwError(() => new Error('Supabase not configured'));

    return from(this.client.from('categories').delete().eq('id', id)).pipe(
      map(({ error }) => {
        if (error) {
          this.toast.error(error.message);
          throw error;
        }
        this.toast.success('Category deleted.');
      }),
    );
  }

  // ---- Toppings --------------------------------------------------------------------
  getToppings(): Observable<AdminTopping[]> {
    if (!this.client) {
      return of(
        MOCK_TOPPINGS.map((t, idx) => ({
          id: t.id,
          name: t.name,
          additionalPrice: t.additionalPrice,
          isAvailable: t.isAvailable,
          sortOrder: idx + 1,
        })),
      );
    }

    return from(
      this.client.from('toppings').select('*').order('sort_order', { ascending: true }),
    ).pipe(
      map(({ data, error }) => {
        if (error || !data || data.length === 0) {
          return MOCK_TOPPINGS.map((t, idx) => ({
            id: t.id,
            name: t.name,
            additionalPrice: t.additionalPrice,
            isAvailable: t.isAvailable,
            sortOrder: idx + 1,
          }));
        }
        return data.map((t: any) => ({
          id: t.id,
          name: t.name,
          additionalPrice: Number(t.additional_price || 0),
          isAvailable: t.is_available ?? true,
          sortOrder: t.sort_order || 0,
        }));
      }),
    );
  }

  createTopping(payload: Omit<AdminTopping, 'id' | 'isAvailable'>): Observable<AdminTopping> {
    if (!this.client) return throwError(() => new Error('Supabase not configured'));

    const id = 'top-' + Date.now();
    return from(
      this.client
        .from('toppings')
        .insert({
          id,
          name: payload.name.trim(),
          additional_price: payload.additionalPrice,
          is_available: true,
          sort_order: payload.sortOrder || 0,
        })
        .select()
        .single(),
    ).pipe(
      map(({ data, error }) => {
        if (error) {
          this.toast.error(error.message);
          throw error;
        }
        this.toast.success(`Topping "${payload.name}" created!`);
        return {
          id: data.id,
          name: data.name,
          additionalPrice: Number(data.additional_price),
          isAvailable: data.is_available,
          sortOrder: data.sort_order,
        };
      }),
    );
  }

  updateTopping(id: string, payload: Omit<AdminTopping, 'id'>): Observable<void> {
    if (!this.client) return throwError(() => new Error('Supabase not configured'));

    return from(
      this.client
        .from('toppings')
        .update({
          name: payload.name.trim(),
          additional_price: payload.additionalPrice,
          is_available: payload.isAvailable,
          sort_order: payload.sortOrder || 0,
        })
        .eq('id', id),
    ).pipe(
      map(({ error }) => {
        if (error) {
          this.toast.error(error.message);
          throw error;
        }
        this.toast.success('Topping updated!');
      }),
    );
  }

  deleteTopping(id: string): Observable<void> {
    if (!this.client) return throwError(() => new Error('Supabase not configured'));

    return from(this.client.from('toppings').delete().eq('id', id)).pipe(
      map(({ error }) => {
        if (error) {
          this.toast.error(error.message);
          throw error;
        }
        this.toast.success('Topping deleted.');
      }),
    );
  }

  // ---- Products ----------------------------------------------------------------------
  createProduct(payload: {
    name: string;
    description: string;
    price: number;
    imageUrl?: string;
    categoryId: string;
    isFeatured: boolean;
    isPopular: boolean;
    sortOrder: number;
    toppingIds: string[];
  }): Observable<AdminProduct> {
    if (!this.client) return throwError(() => new Error('Supabase not configured'));

    const id = 'prod-' + Date.now();
    return from(
      (async () => {
        const { data, error } = await this.client!
          .from('products')
          .insert({
            id,
            name: payload.name.trim(),
            description: payload.description.trim(),
            price: payload.price,
            image_url: payload.imageUrl || '',
            category_id: payload.categoryId,
            is_available: true,
            is_featured: payload.isFeatured,
            is_popular: payload.isPopular,
            sort_order: payload.sortOrder || 0,
          })
          .select('*, categories(name)')
          .single();

        if (error) throw error;

        if (payload.toppingIds && payload.toppingIds.length > 0) {
          const links = payload.toppingIds.map((tId) => ({
            product_id: id,
            topping_id: tId,
          }));
          await this.client!.from('product_toppings').insert(links);
        }

        return data;
      })(),
    ).pipe(
      map((data: any) => {
        this.toast.success(`Meal "${payload.name}" added to menu!`);
        return {
          id: data.id,
          name: data.name,
          description: data.description,
          price: Number(data.price),
          imageUrl: data.image_url,
          isAvailable: data.is_available,
          isFeatured: data.is_featured,
          isPopular: data.is_popular,
          categoryId: data.category_id,
          categoryName: data.categories?.name || '',
          sortOrder: data.sort_order,
          toppings: [],
        };
      }),
    );
  }

  updateProduct(
    id: string,
    payload: {
      name: string;
      description: string;
      price: number;
      imageUrl?: string;
      categoryId: string;
      isAvailable: boolean;
      isFeatured: boolean;
      isPopular: boolean;
      sortOrder: number;
      toppingIds: string[];
    },
  ): Observable<void> {
    if (!this.client) return throwError(() => new Error('Supabase not configured'));

    return from(
      (async () => {
        const { error } = await this.client!
          .from('products')
          .update({
            name: payload.name.trim(),
            description: payload.description.trim(),
            price: payload.price,
            image_url: payload.imageUrl || '',
            category_id: payload.categoryId,
            is_available: payload.isAvailable,
            is_featured: payload.isFeatured,
            is_popular: payload.isPopular,
            sortOrder: payload.sortOrder || 0,
          })
          .eq('id', id);

        if (error) throw error;

        // Sync toppings
        await this.client!.from('product_toppings').delete().eq('product_id', id);
        if (payload.toppingIds && payload.toppingIds.length > 0) {
          const links = payload.toppingIds.map((tId) => ({
            product_id: id,
            topping_id: tId,
          }));
          await this.client!.from('product_toppings').insert(links);
        }
      })(),
    ).pipe(
      map(() => {
        this.toast.success(`Updated "${payload.name}" successfully!`);
      }),
    );
  }

  deleteProduct(id: string): Observable<void> {
    if (!this.client) return throwError(() => new Error('Supabase not configured'));

    return from(this.client.from('products').delete().eq('id', id)).pipe(
      map(({ error }) => {
        if (error) {
          this.toast.error(error.message);
          throw error;
        }
        this.toast.success('Meal deleted from menu.');
      }),
    );
  }

  // ---- Payment Methods --------------------------------------------------------------
  getPaymentMethods(): Observable<AdminPaymentMethod[]> {
    return of([]);
  }
  createPaymentMethod(_payload: any): Observable<AdminPaymentMethod> {
    return of({} as AdminPaymentMethod);
  }
  updatePaymentMethod(_id: string, _payload: any): Observable<void> {
    return of(undefined);
  }
  deletePaymentMethod(_id: string): Observable<void> {
    return of(undefined);
  }

  // ---- Promotions ----------------------------------------------------------------------
  getPromotions(): Observable<AdminPromotion[]> {
    if (!this.client) {
      return of(
        MOCK_PROMOTIONS.map((p) => ({
          id: p.id,
          name: p.name,
          description: p.description,
          discountType: p.discountType as any,
          discountValue: p.discountValue,
          startDateUtc: new Date().toISOString(),
          endDateUtc: new Date(Date.now() + 30 * 86400 * 1000).toISOString(),
          isFeatured: p.isFeatured,
          isActive: true,
          productIds: p.productIds || [],
        })),
      );
    }

    return from(this.client.from('promotions').select('*')).pipe(
      map(({ data, error }) => {
        if (error || !data || data.length === 0) {
          return MOCK_PROMOTIONS.map((p) => ({
            id: p.id,
            name: p.name,
            description: p.description,
            discountType: p.discountType as any,
            discountValue: p.discountValue,
            startDateUtc: new Date().toISOString(),
            endDateUtc: new Date(Date.now() + 30 * 86400 * 1000).toISOString(),
            isFeatured: p.isFeatured,
            isActive: true,
            productIds: p.productIds || [],
          }));
        }
        return data.map((pr: any) => ({
          id: pr.id,
          name: pr.name,
          description: pr.description || '',
          discountType: pr.discount_type || 'FixedAmount',
          discountValue: Number(pr.discount_value || 0),
          startDateUtc: pr.created_at || new Date().toISOString(),
          endDateUtc: new Date(Date.now() + 30 * 86400 * 1000).toISOString(),
          isFeatured: pr.is_featured ?? true,
          isActive: pr.is_active ?? true,
          productIds: [],
        }));
      }),
    );
  }

  createPromotion(payload: Omit<AdminPromotion, 'id' | 'isActive'>): Observable<AdminPromotion> {
    if (!this.client) return throwError(() => new Error('Supabase not configured'));
    const id = 'promo-' + Date.now();

    return from(
      this.client
        .from('promotions')
        .insert({
          id,
          name: payload.name.trim(),
          description: payload.description || '',
          discount_type: payload.discountType,
          discount_value: payload.discountValue,
          is_featured: payload.isFeatured,
          is_active: true,
        })
        .select()
        .single(),
    ).pipe(
      map(({ data, error }) => {
        if (error) throw error;
        this.toast.success('Promotion created!');
        return {
          id: data.id,
          name: data.name,
          description: data.description,
          discountType: data.discount_type,
          discountValue: Number(data.discount_value),
          startDateUtc: data.created_at,
          endDateUtc: new Date(Date.now() + 30 * 86400 * 1000).toISOString(),
          isFeatured: data.is_featured,
          isActive: data.is_active,
          productIds: [],
        };
      }),
    );
  }

  updatePromotion(id: string, payload: Omit<AdminPromotion, 'id'>): Observable<void> {
    if (!this.client) return throwError(() => new Error('Supabase not configured'));

    return from(
      this.client
        .from('promotions')
        .update({
          name: payload.name.trim(),
          description: payload.description || '',
          discount_type: payload.discountType,
          discount_value: payload.discountValue,
          is_featured: payload.isFeatured,
          is_active: payload.isActive,
        })
        .eq('id', id),
    ).pipe(
      map(({ error }) => {
        if (error) throw error;
        this.toast.success('Promotion updated!');
      }),
    );
  }

  deletePromotion(id: string): Observable<void> {
    if (!this.client) return throwError(() => new Error('Supabase not configured'));

    return from(this.client.from('promotions').delete().eq('id', id)).pipe(
      map(({ error }) => {
        if (error) throw error;
        this.toast.success('Promotion deleted.');
      }),
    );
  }
}
