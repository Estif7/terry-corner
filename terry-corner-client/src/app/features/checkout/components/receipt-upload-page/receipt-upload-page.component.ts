import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { OrdersService } from '../../../orders/services/orders.service';
import { ToastService } from '../../../../core/services/toast.service';

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'application/pdf'];
const MAX_SIZE_BYTES = 5 * 1024 * 1024;

@Component({
  selector: 'tc-receipt-upload-page',
  standalone: true,
  templateUrl: './receipt-upload-page.component.html',
  styleUrl: './receipt-upload-page.component.scss',
})
export class ReceiptUploadPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly ordersService = inject(OrdersService);
  private readonly toast = inject(ToastService);

  readonly orderNumber = this.route.snapshot.paramMap.get('orderNumber') ?? '';
  readonly selectedFile = signal<File | null>(null);
  readonly fileError = signal<string | null>(null);
  readonly transactionReferenceNumber = signal('');
  readonly paymentNote = signal('');
  readonly submitting = signal(false);
  readonly submitted = signal(false);

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    if (!file) {
      this.selectedFile.set(null);
      return;
    }

    if (!ALLOWED_TYPES.includes(file.type)) {
      this.fileError.set('Unsupported file type. Please upload a JPG, PNG, or PDF.');
      this.selectedFile.set(null);
      return;
    }

    if (file.size > MAX_SIZE_BYTES) {
      this.fileError.set('File is too large. Maximum size is 5 MB.');
      this.selectedFile.set(null);
      return;
    }

    this.fileError.set(null);
    this.selectedFile.set(file);
  }

  submit(): void {
    const file = this.selectedFile();
    if (!file) {
      this.fileError.set('Please select a receipt image or PDF.');
      return;
    }

    this.submitting.set(true);

    this.ordersService
      .uploadReceipt(this.orderNumber, file, this.transactionReferenceNumber(), this.paymentNote())
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.submitted.set(true);
        },
        error: () => {
          this.submitting.set(false);
        },
      });
  }

  goToTracking(): void {
    this.router.navigate(['/orders/track', this.orderNumber]);
  }
}
