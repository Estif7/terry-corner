import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { CartService } from '../../../cart/services/cart.service';
import { OrdersService } from '../../../orders/services/orders.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'tc-checkout-page',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, DecimalPipe],
  templateUrl: './checkout-page.component.html',
  styleUrl: './checkout-page.component.scss',
})
export class CheckoutPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly ordersService = inject(OrdersService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly cart = inject(CartService);
  readonly submitting = signal(false);

  readonly form = this.fb.nonNullable.group({
    contactFullName: ['', [Validators.required, Validators.maxLength(150)]],
    contactPhoneNumber: ['', [Validators.required, Validators.maxLength(30)]],
    orderType: ['Pickup' as 'Pickup' | 'Delivery', Validators.required],
    deliveryAddress: [''],
    orderNotes: [''],
  });

  get isDelivery(): boolean {
    return this.form.controls.orderType.value === 'Delivery';
  }

  submit(): void {
    if (this.cart.items().length === 0) {
      this.toast.error('Your cart is empty.');
      return;
    }

    if (this.isDelivery && !this.form.controls.deliveryAddress.value.trim()) {
      this.form.controls.deliveryAddress.setErrors({ required: true });
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    const value = this.form.getRawValue();

    this.ordersService
      .createOrder({
        contactFullName: value.contactFullName,
        contactPhoneNumber: value.contactPhoneNumber,
        orderType: value.orderType,
        deliveryAddress: value.orderType === 'Delivery' ? value.deliveryAddress : undefined,
        orderNotes: value.orderNotes || undefined,
        items: this.cart.items().map((item) => ({
          productId: item.productId,
          quantity: item.quantity,
          toppingIds: item.toppings.map((t) => t.toppingId),
        })),
      })
      .subscribe({
        next: (order) => {
          this.cart.clear();
          this.router.navigate(['/checkout/payment', order.orderNumber]);
        },
        error: () => {
          this.submitting.set(false);
        },
      });
  }
}
