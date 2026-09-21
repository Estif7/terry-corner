import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { OrdersService } from '../../services/orders.service';
import { ORDER_STATUS_LABELS, OrderTracking } from '../../models/order.models';
import { OrderHubService } from '../../../../core/services/order-hub.service';

@Component({
  selector: 'tc-order-tracking-page',
  standalone: true,
  imports: [FormsModule, DecimalPipe],
  templateUrl: './order-tracking-page.component.html',
  styleUrl: './order-tracking-page.component.scss',
})
export class OrderTrackingPageComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly ordersService = inject(OrdersService);
  private readonly orderHub = inject(OrderHubService);

  readonly searchValue = signal('');
  readonly tracking = signal<OrderTracking | null>(null);
  readonly loading = signal(false);
  readonly notFound = signal(false);
  readonly statusLabels = ORDER_STATUS_LABELS;

  ngOnInit(): void {
    const fromRoute = this.route.snapshot.paramMap.get('orderNumber');
    if (fromRoute) {
      this.searchValue.set(fromRoute);
      this.search();
    }
  }

  ngOnDestroy(): void {
    this.orderHub.stopWatching();
  }

  search(): void {
    const orderNumber = this.searchValue().trim();
    if (!orderNumber) return;

    this.loading.set(true);
    this.notFound.set(false);
    this.tracking.set(null);

    this.ordersService.trackOrder(orderNumber).subscribe({
      next: (result) => {
        this.tracking.set(result);
        this.loading.set(false);
        // Live updates: re-fetch automatically the moment staff approve/reject this order,
        // instead of requiring a manual refresh.
        this.orderHub.watchOrder(orderNumber, () => this.refresh(orderNumber));
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  private refresh(orderNumber: string): void {
    this.ordersService.trackOrder(orderNumber).subscribe({
      next: (result) => this.tracking.set(result),
      error: () => {},
    });
  }
}
