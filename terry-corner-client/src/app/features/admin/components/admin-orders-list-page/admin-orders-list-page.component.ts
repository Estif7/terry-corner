import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DecimalPipe, DatePipe } from '@angular/common';
import { AdminService } from '../../services/admin.service';
import { AdminOrderListItem } from '../../models/admin.models';

const STATUS_OPTIONS = [
  'OrderReceived', 'PaymentPending', 'PaymentReceiptSubmitted', 'PaymentVerification',
  'PreparingFood', 'Ready', 'Completed', 'Cancelled',
];

@Component({
  selector: 'tc-admin-orders-list-page',
  standalone: true,
  imports: [FormsModule, DecimalPipe, DatePipe],
  templateUrl: './admin-orders-list-page.component.html',
  styleUrl: './admin-orders-list-page.component.scss',
})
export class AdminOrdersListPageComponent implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly router = inject(Router);

  readonly orders = signal<AdminOrderListItem[]>([]);
  readonly loading = signal(true);
  readonly statusFilter = signal('');
  readonly search = signal('');
  readonly statusOptions = STATUS_OPTIONS;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.adminService.getOrders(this.statusFilter() || undefined, this.search() || undefined).subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  openOrder(order: AdminOrderListItem): void {
    this.router.navigate(['/admin/orders', order.id]);
  }
}
