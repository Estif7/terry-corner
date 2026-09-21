import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { MyOrderDetail, MyOrderListItem, MyProfile } from '../models/account.models';

@Injectable({ providedIn: 'root' })
export class AccountService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/account`;

  getProfile(): Observable<MyProfile> {
    return this.http.get<MyProfile>(`${this.baseUrl}/profile`);
  }

  updateProfile(fullName: string, phoneNumber: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/profile`, { fullName, phoneNumber });
  }

  getOrders(): Observable<MyOrderListItem[]> {
    return this.http.get<MyOrderListItem[]>(`${this.baseUrl}/orders`);
  }

  getOrderDetail(orderNumber: string): Observable<MyOrderDetail> {
    return this.http.get<MyOrderDetail>(`${this.baseUrl}/orders/${orderNumber}`);
  }

  uploadProfilePicture(file: File): Observable<void> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<void>(`${this.baseUrl}/profile-picture`, formData);
  }

  deleteProfilePicture(): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/profile-picture`);
  }

  /** Profile pictures need an auth header, so fetch as a blob and hand back an object URL. */
  getProfilePictureUrl(): Observable<string> {
    return new Observable((subscriber) => {
      this.http.get(`${this.baseUrl}/profile-picture`, { responseType: 'blob' }).subscribe({
        next: (blob) => {
          subscriber.next(URL.createObjectURL(blob));
          subscriber.complete();
        },
        error: (err) => subscriber.error(err),
      });
    });
  }
}
