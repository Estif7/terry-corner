import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DecimalPipe, DatePipe } from '@angular/common';
import { AdminService } from '../../services/admin.service';
import { AdminOrderDetail } from '../../models/admin.models';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'tc-admin-order-detail-page',
  standalone: true,
  imports: [RouterLink, DecimalPipe, DatePipe],
  templateUrl: './admin-order-detail-page.component.html',
  styleUrl: './admin-order-detail-page.component.scss',
})
export class AdminOrderDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly adminService = inject(AdminService);
  private readonly toast = inject(ToastService);

  readonly orderId = this.route.snapshot.paramMap.get('id') ?? '';
  readonly order = signal<AdminOrderDetail | null>(null);
  readonly loading = signal(true);
  readonly noteDraft = signal('');
  readonly savingNote = signal(false);
  readonly updatingStatus = signal(false);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.adminService.getOrderDetail(this.orderId).subscribe({
      next: (order) => {
        this.order.set(order);
        this.noteDraft.set(order.internalStaffNotes ?? '');
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  setStatus(status: 'Ready' | 'Completed' | 'Cancelled'): void {
    this.updatingStatus.set(true);
    this.adminService.updateOrderStatus(this.orderId, status).subscribe({
      next: () => {
        this.toast.success(`Order marked as ${status}.`);
        this.load();
        this.updatingStatus.set(false);
      },
      error: () => this.updatingStatus.set(false),
    });
  }

  saveNote(): void {
    this.savingNote.set(true);
    this.adminService.addOrderNote(this.orderId, this.noteDraft()).subscribe({
      next: () => {
        this.toast.success('Note saved.');
        this.savingNote.set(false);
      },
      error: () => this.savingNote.set(false),
    });
  }
}
