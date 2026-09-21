import { Component, EventEmitter, Input, Output, computed, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ResolveImageUrlPipe } from '../../../../shared/pipes/resolve-image-url.pipe';
import { Product, Topping } from '../../models/menu.models';
import { CartService } from '../../../cart/services/cart.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'tc-product-modal',
  standalone: true,
  imports: [DecimalPipe, ResolveImageUrlPipe],
  templateUrl: './product-modal.component.html',
  styleUrl: './product-modal.component.scss',
})
export class ProductModalComponent {
  @Input({ required: true }) product!: Product;
  @Output() closed = new EventEmitter<void>();

  readonly quantity = signal(1);
  readonly selectedToppingIds = signal<Set<string>>(new Set());

  readonly selectedToppings = computed<Topping[]>(() =>
    (this.product?.toppings ?? []).filter((t) => this.selectedToppingIds().has(t.id)),
  );

  readonly toppingsTotal = computed(() =>
    this.selectedToppings().reduce((sum, t) => sum + t.additionalPrice, 0),
  );

  readonly unitPrice = computed(() => this.product.price + this.toppingsTotal());

  readonly total = computed(() => this.unitPrice() * this.quantity());

  constructor(
    private readonly cart: CartService,
    private readonly toast: ToastService,
  ) {}

  toggleTopping(topping: Topping): void {
    if (!topping.isAvailable) return;
    this.selectedToppingIds.update((ids) => {
      const next = new Set(ids);
      if (next.has(topping.id)) {
        next.delete(topping.id);
      } else {
        next.add(topping.id);
      }
      return next;
    });
  }

  increment(): void {
    this.quantity.update((q) => q + 1);
  }

  decrement(): void {
    this.quantity.update((q) => Math.max(1, q - 1));
  }

  addToOrder(): void {
    this.cart.add({
      lineId: crypto.randomUUID(),
      productId: this.product.id,
      name: this.product.name,
      imageUrl: this.product.imageUrl ?? '',
      basePrice: this.product.price,
      quantity: this.quantity(),
      toppings: this.selectedToppings().map((t) => ({
        toppingId: t.id,
        name: t.name,
        price: t.additionalPrice,
      })),
    });
    this.toast.success(`Added ${this.quantity()} × ${this.product.name} to your order`);
    this.close();
  }

  close(): void {
    this.closed.emit();
  }
}
