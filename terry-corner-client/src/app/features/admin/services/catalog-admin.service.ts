import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AdminCategory, AdminPaymentMethod, AdminProduct, AdminPromotion, AdminTopping } from '../models/admin.models';

@Injectable({ providedIn: 'root' })
export class CatalogAdminService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/admin`;

  // ---- Images ----------------------------------------------------------------------------
  uploadImage(file: File): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ url: string }>(`${this.baseUrl}/images`, formData);
  }

  // ---- Categories ----------------------------------------------------------------
  getCategories(): Observable<AdminCategory[]> {
    return this.http.get<AdminCategory[]>(`${this.baseUrl}/categories`);
  }
  createCategory(payload: Omit<AdminCategory, 'id' | 'isActive'>): Observable<AdminCategory> {
    return this.http.post<AdminCategory>(`${this.baseUrl}/categories`, payload);
  }
  updateCategory(id: string, payload: Omit<AdminCategory, 'id'>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/categories/${id}`, payload);
  }
  deleteCategory(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/categories/${id}`);
  }

  // ---- Toppings --------------------------------------------------------------------
  getToppings(): Observable<AdminTopping[]> {
    return this.http.get<AdminTopping[]>(`${this.baseUrl}/toppings`);
  }
  createTopping(payload: Omit<AdminTopping, 'id' | 'isAvailable'>): Observable<AdminTopping> {
    return this.http.post<AdminTopping>(`${this.baseUrl}/toppings`, payload);
  }
  updateTopping(id: string, payload: Omit<AdminTopping, 'id'>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/toppings/${id}`, payload);
  }
  deleteTopping(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/toppings/${id}`);
  }

  // ---- Products ----------------------------------------------------------------------
  createProduct(payload: {
    name: string; description: string; price: number; imageUrl?: string; categoryId: string;
    isFeatured: boolean; isPopular: boolean; sortOrder: number; toppingIds: string[];
  }): Observable<AdminProduct> {
    return this.http.post<AdminProduct>(`${this.baseUrl}/products`, payload);
  }
  updateProduct(id: string, payload: {
    name: string; description: string; price: number; imageUrl?: string; categoryId: string;
    isAvailable: boolean; isFeatured: boolean; isPopular: boolean; sortOrder: number; toppingIds: string[];
  }): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/products/${id}`, payload);
  }
  deleteProduct(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/products/${id}`);
  }

  // ---- Payment Methods --------------------------------------------------------------
  getPaymentMethods(): Observable<AdminPaymentMethod[]> {
    return this.http.get<AdminPaymentMethod[]>(`${this.baseUrl}/payment-methods`);
  }
  createPaymentMethod(payload: Omit<AdminPaymentMethod, 'id' | 'isActive'>): Observable<AdminPaymentMethod> {
    return this.http.post<AdminPaymentMethod>(`${this.baseUrl}/payment-methods`, payload);
  }
  updatePaymentMethod(id: string, payload: Omit<AdminPaymentMethod, 'id'>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/payment-methods/${id}`, payload);
  }
  deletePaymentMethod(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/payment-methods/${id}`);
  }

  // ---- Promotions ----------------------------------------------------------------------
  getPromotions(): Observable<AdminPromotion[]> {
    return this.http.get<AdminPromotion[]>(`${this.baseUrl}/promotions`);
  }
  createPromotion(payload: Omit<AdminPromotion, 'id' | 'isActive'>): Observable<AdminPromotion> {
    return this.http.post<AdminPromotion>(`${this.baseUrl}/promotions`, payload);
  }
  updatePromotion(id: string, payload: Omit<AdminPromotion, 'id'>): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/promotions/${id}`, payload);
  }
  deletePromotion(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/promotions/${id}`);
  }
}
