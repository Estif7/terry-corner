import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { OrdersService } from '../../../orders/services/orders.service';
import { PaymentMethod } from '../../../orders/models/order.models';

@Component({
  selector: 'tc-payment-instructions-page',
  standalone: true,
  imports: [DecimalPipe],
  templateUrl: './payment-instructions-page.component.html',
  styleUrl: './payment-instructions-page.component.scss',
})
export class PaymentInstructionsPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly ordersService = inject(OrdersService);

  readonly orderNumber = signal('');
  readonly total = signal<number | null>(null);
  readonly paymentMethods = signal<PaymentMethod[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    const orderNumber = this.route.snapshot.paramMap.get('orderNumber') ?? '';
    this.orderNumber.set(orderNumber);

    this.ordersService.trackOrder(orderNumber).subscribe({
      next: (tracking) => this.total.set(tracking.total),
      error: () => {},
    });

    this.ordersService.getActivePaymentMethods().subscribe({
      next: (methods) => {
        this.paymentMethods.set(methods);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  goToReceiptUpload(): void {
    this.router.navigate(['/checkout/receipt', this.orderNumber()]);
  }
}
