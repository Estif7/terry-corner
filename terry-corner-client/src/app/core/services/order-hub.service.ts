import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class OrderHubService {
  private connection: signalR.HubConnection | null = null;
  private startPromise: Promise<void> | null = null;
  private currentOrderNumber: string | null = null;

  constructor(private readonly auth: AuthService) {}

  private connect(): { connection: signalR.HubConnection; ready: Promise<void> } {
    if (!this.connection) {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(`${environment.signalRHubUrl}/orders`, {
          accessTokenFactory: () => this.auth.getAccessToken() ?? '',
        })
        .withAutomaticReconnect()
        .build();

      // Rejoin the current order's group after any automatic reconnect — SignalR groups
      // aren't restored automatically when the underlying connection drops and comes back.
      this.connection.onreconnected(() => {
        if (this.currentOrderNumber) {
          this.connection?.invoke('JoinOrderGroup', this.currentOrderNumber).catch(() => {});
        }
      });

      this.startPromise = this.connection.start().catch(() => {
        // Live updates are a nice-to-have; the tracking page still works via manual refresh
        // if the socket can't connect (e.g. a network blocking WebSockets).
      });
    }

    return { connection: this.connection, ready: this.startPromise! };
  }

  /** Joins the group for one order and calls onUpdate whenever staff change its status. */
  watchOrder(orderNumber: string, onUpdate: () => void): void {
    const { connection, ready } = this.connect();

    connection.off('OrderUpdated');
    connection.on('OrderUpdated', (payload: { orderNumber: string }) => {
      if (payload.orderNumber === orderNumber) {
        onUpdate();
      }
    });

    this.currentOrderNumber = orderNumber;
    ready.then(() => connection.invoke('JoinOrderGroup', orderNumber).catch(() => {}));
  }

  stopWatching(): void {
    if (this.connection && this.currentOrderNumber) {
      this.connection.invoke('LeaveOrderGroup', this.currentOrderNumber).catch(() => {});
    }
    this.currentOrderNumber = null;
  }
}
