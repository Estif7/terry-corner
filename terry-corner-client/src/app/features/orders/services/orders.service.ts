import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CreateOrderRequest,
  OrderResult,
  OrderTracking,
  PaymentMethod,
  PaymentReceiptResult,
} from '../models/order.models';

@Injectable({ providedIn: 'root' })
export class OrdersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  createOrder(request: CreateOrderRequest): Observable<OrderResult> {
    return this.http.post<OrderResult>(`${this.baseUrl}/orders`, request);
  }

  trackOrder(orderNumber: string): Observable<OrderTracking> {
    return this.http.get<OrderTracking>(`${this.baseUrl}/orders/${orderNumber}`);
  }

  getActivePaymentMethods(): Observable<PaymentMethod[]> {
    return this.http.get<PaymentMethod[]>(`${this.baseUrl}/payment-methods/active`);
  }

  uploadReceipt(
    orderNumber: string,
    file: File,
    transactionReferenceNumber: string,
    paymentNote: string,
  ): Observable<PaymentReceiptResult> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('TransactionReferenceNumber', transactionReferenceNumber);
    formData.append('PaymentNote', paymentNote);

    return this.http.post<PaymentReceiptResult>(
      `${this.baseUrl}/orders/${orderNumber}/receipts`,
      formData,
    );
  }
}
