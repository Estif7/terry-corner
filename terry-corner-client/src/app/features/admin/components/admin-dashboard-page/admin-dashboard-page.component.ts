import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe, DatePipe } from '@angular/common';
import { AdminService } from '../../services/admin.service';
import { PaymentReceiptReview } from '../../models/admin.models';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'tc-admin-dashboard-page',
  standalone: true,
  imports: [DecimalPipe, DatePipe],
  templateUrl: './admin-dashboard-page.component.html',
  styleUrl: './admin-dashboard-page.component.scss',
})
export class AdminDashboardPageComponent implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly toast = inject(ToastService);

  readonly receipts = signal<PaymentReceiptReview[]>([]);
  readonly loading = signal(true);
  readonly previewUrls = signal<Record<string, string>>({});
  readonly rejectingId = signal<string | null>(null);
  readonly rejectReason = signal('');
  readonly busyId = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.adminService.getPendingReceipts().subscribe({
      next: (receipts) => {
        this.receipts.set(receipts);
        this.loading.set(false);
        receipts.forEach((r) => this.loadPreview(r.id));
      },
      error: () => this.loading.set(false),
    });
  }

  private loadPreview(receiptId: string): void {
    this.adminService.getReceiptFileUrl(receiptId).subscribe({
      next: (url) => this.previewUrls.update((map) => ({ ...map, [receiptId]: url })),
      error: () => {},
    });
  }

  approve(receipt: PaymentReceiptReview): void {
    this.busyId.set(receipt.id);
    this.adminService.approveReceipt(receipt.id).subscribe({
      next: () => {
        this.toast.success(`Order ${receipt.orderNumber} approved — now preparing food.`);
        this.removeFromList(receipt.id);
      },
      error: () => this.busyId.set(null),
    });
  }

  startReject(receiptId: string): void {
    this.rejectingId.set(receiptId);
    this.rejectReason.set('');
  }

  cancelReject(): void {
    this.rejectingId.set(null);
  }

  confirmReject(receipt: PaymentReceiptReview): void {
    const reason = this.rejectReason().trim();
    if (!reason) {
      this.toast.error('Please provide a reason for rejecting this receipt.');
      return;
    }

    this.busyId.set(receipt.id);
    this.adminService.rejectReceipt(receipt.id, reason).subscribe({
      next: () => {
        this.toast.info(`Order ${receipt.orderNumber} receipt rejected.`);
        this.rejectingId.set(null);
        this.removeFromList(receipt.id);
      },
      error: () => this.busyId.set(null),
    });
  }

  private removeFromList(receiptId: string): void {
    this.receipts.update((list) => list.filter((r) => r.id !== receiptId));
    this.busyId.set(null);
  }
}
