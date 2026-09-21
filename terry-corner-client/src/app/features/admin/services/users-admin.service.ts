import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AdminUser, UserStats } from '../models/admin.models';

@Injectable({ providedIn: 'root' })
export class UsersAdminService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/admin/users`;

  getUsers(): Observable<AdminUser[]> {
    return this.http.get<AdminUser[]>(this.baseUrl);
  }

  getStats(): Observable<UserStats> {
    return this.http.get<UserStats>(`${this.baseUrl}/stats`);
  }

  updateUser(id: string, fullName: string, email: string, phoneNumber: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, { fullName, email, phoneNumber });
  }

  createManager(fullName: string, email: string, password: string): Observable<AdminUser> {
    return this.http.post<AdminUser>(`${this.baseUrl}/managers`, { fullName, email, password });
  }

  changeRole(id: string, newRole: string): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/role`, { newRole });
  }

  restrictUser(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/restrict`, {});
  }

  unrestrictUser(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/unrestrict`, {});
  }

  getProfilePictureUrl(userId: string): Observable<string> {
    return new Observable((subscriber) => {
      this.http.get(`${this.baseUrl}/${userId}/profile-picture`, { responseType: 'blob' }).subscribe({
        next: (blob) => {
          subscriber.next(URL.createObjectURL(blob));
          subscriber.complete();
        },
        error: (err) => subscriber.error(err),
      });
    });
  }
}
