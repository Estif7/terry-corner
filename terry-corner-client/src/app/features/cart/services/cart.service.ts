import { Injectable, computed, signal } from '@angular/core';
import { CartItem, lineTotal } from '../models/cart.models';

const CART_STORAGE_KEY = 'tc_cart';

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly itemsSignal = signal<CartItem[]>(this.readStoredCart());

  readonly items = this.itemsSignal.asReadonly();
  readonly itemCount = computed(() =>
    this.itemsSignal().reduce((sum, item) => sum + item.quantity, 0),
  );
  readonly subtotal = computed(() =>
    this.itemsSignal().reduce((sum, item) => sum + lineTotal(item), 0),
  );

  add(item: CartItem): void {
    this.itemsSignal.update((items) => [...items, item]);
    this.persist();
  }

  updateQuantity(lineId: string, quantity: number): void {
    if (quantity < 1) {
      this.remove(lineId);
      return;
    }
    this.itemsSignal.update((items) =>
      items.map((i) => (i.lineId === lineId ? { ...i, quantity } : i)),
    );
    this.persist();
  }

  remove(lineId: string): void {
    this.itemsSignal.update((items) => items.filter((i) => i.lineId !== lineId));
    this.persist();
  }

  clear(): void {
    this.itemsSignal.set([]);
    this.persist();
  }

  private persist(): void {
    localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(this.itemsSignal()));
  }

  private readStoredCart(): CartItem[] {
    const raw = localStorage.getItem(CART_STORAGE_KEY);
    return raw ? (JSON.parse(raw) as CartItem[]) : [];
  }
}
