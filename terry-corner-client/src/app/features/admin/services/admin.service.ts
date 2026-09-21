import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  AdminOrderDetail,
  AdminOrderListItem,
  AdminOverview,
  PaymentReceiptReview,
} from '../models/admin.models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/admin`;

  getPendingReceipts(): Observable<PaymentReceiptReview[]> {
    return this.http.get<PaymentReceiptReview[]>(`${this.baseUrl}/payment-receipts/pending`);
  }

  /**
   * Receipt files require an authenticated request (the auth interceptor attaches the bearer
   * token), so we can't just point an <img> tag's src at the URL directly — fetch as a blob and
   * hand back an object URL instead.
   */
  getReceiptFileUrl(receiptId: string): Observable<string> {
    return new Observable((subscriber) => {
      this.http
        .get(`${this.baseUrl}/payment-receipts/${receiptId}/file`, { responseType: 'blob' })
        .subscribe({
          next: (blob) => {
            const objectUrl = URL.createObjectURL(blob);
            subscriber.next(objectUrl);
            subscriber.complete();
          },
          error: (err) => subscriber.error(err),
        });
    });
  }

  getOverview(): Observable<AdminOverview> {
    return this.http.get<AdminOverview>(`${this.baseUrl}/overview`);
  }

  getOrders(status?: string, search?: string): Observable<AdminOrderListItem[]> {
    const params: string[] = [];
    if (status) params.push(`status=${status}`);
    if (search) params.push(`search=${encodeURIComponent(search)}`);
    const query = params.length ? `?${params.join('&')}` : '';
    return this.http.get<AdminOrderListItem[]>(`${this.baseUrl}/orders${query}`);
  }

  getOrderDetail(orderId: string): Observable<AdminOrderDetail> {
    return this.http.get<AdminOrderDetail>(`${this.baseUrl}/orders/${orderId}`);
  }

  updateOrderStatus(orderId: string, newStatus: string): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/orders/${orderId}/status`, { newStatus });
  }

  addOrderNote(orderId: string, note: string): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/orders/${orderId}/notes`, { note });
  }

  approveReceipt(receiptId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/payment-receipts/${receiptId}/approve`, {});
  }

  rejectReceipt(receiptId: string, reason: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/payment-receipts/${receiptId}/reject`, { reason });
  }
}
