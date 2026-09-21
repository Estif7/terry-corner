import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { ResolveImageUrlPipe } from '../../../../shared/pipes/resolve-image-url.pipe';
import { CartService } from '../../services/cart.service';
import { ToastService } from '../../../../core/services/toast.service';
import { CartItem, lineTotal } from '../../models/cart.models';

const TELEGRAM_BOT_TOKEN = '8718265722:AAHqQMCfs8fAUeJiiQUkuopZmEgtDw67_7M';
const TELEGRAM_CHAT_ID = '5079496783';
const TELEGRAM_USERNAME = 'estifc137';
const BOT_USERNAME = 'terry_corner_orders_bot';

@Component({
  selector: 'tc-cart-page',
  standalone: true,
  imports: [RouterLink, FormsModule, DecimalPipe, ResolveImageUrlPipe],
  templateUrl: './cart-page.component.html',
  styleUrl: './cart-page.component.scss',
})
export class CartPageComponent {
  readonly cart = inject(CartService);
  private readonly toast = inject(ToastService);

  // Form State
  readonly orderType = signal<'pickup' | 'delivery'>('pickup');
  readonly customerName = signal('');
  readonly phoneNumber = signal('');
  readonly deliveryAddress = signal('');
  readonly notes = signal('');

  // UI State
  readonly submitting = signal(false);
  readonly orderPlaced = signal(false);
  readonly copied = signal(false);
  readonly lastOrderNumber = signal('');
  readonly lastOrderText = signal('');
  readonly lastOrderTotal = signal(0);
  readonly lastOrderType = signal<'pickup' | 'delivery'>('pickup');

  toppingNames(item: CartItem): string {
    return item.toppings.map((t) => t.name).join(', ');
  }

  lineTotal(item: CartItem): number {
    return lineTotal(item);
  }

  increment(item: CartItem): void {
    this.cart.updateQuantity(item.lineId, item.quantity + 1);
  }

  decrement(item: CartItem): void {
    this.cart.updateQuantity(item.lineId, item.quantity - 1);
  }

  remove(item: CartItem): void {
    this.cart.remove(item.lineId);
  }

  clear(): void {
    this.cart.clear();
    this.toast.info('Your tray has been cleared.');
  }

  selectOrderType(type: 'pickup' | 'delivery'): void {
    this.orderType.set(type);
  }

  copyAccount(account: string, name: string): void {
    navigator.clipboard.writeText(account).then(() => {
      this.toast.success(`Copied ${name} account (${account}) to clipboard!`);
    });
  }

  formatOrderSummary(orderNum: string): string {
    const isDelivery = this.orderType() === 'delivery';
    const lines = this.cart.items().map((item) => {
      const toppings = item.toppings.length ? ` (+ ${this.toppingNames(item)})` : '';
      return `• ${item.quantity}x ${item.name}${toppings} - ETB ${this.lineTotal(item)}`;
    });

    const notePart = this.notes().trim() ? `\n📝 Note: ${this.notes().trim()}` : '';
    const addressPart = isDelivery ? `\n📍 Delivery Address: ${this.deliveryAddress().trim()}` : '';

    return `🍔 *TERRY CORNER NEW ORDER*\n` +
      `────────────────────────\n` +
      `🔢 Order #: *${orderNum}*\n` +
      `🛵 Type: *${isDelivery ? 'Delivery (Advance Payment)' : 'Pickup (Pay at counter)'}*\n` +
      `👤 Name: ${this.customerName().trim()}\n` +
      `📞 Phone: ${this.phoneNumber().trim()}` +
      addressPart +
      notePart + `\n\n` +
      `📋 *Items Ordered:*\n${lines.join('\n')}\n` +
      `────────────────────────\n` +
      `💰 *Total Amount: ETB ${this.cart.subtotal()}*` +
      (isDelivery ? `\n⚠️ *Payment Receipt needed to dispatch.*` : '');
  }

  async submitOrder(): Promise<void> {
    if (!this.customerName().trim()) {
      this.toast.error('Please enter your full name.');
      return;
    }

    if (!this.phoneNumber().trim() || this.phoneNumber().trim().length < 9) {
      this.toast.error('Please enter a valid phone number (e.g. 0911...)');
      return;
    }

    if (this.orderType() === 'delivery' && !this.deliveryAddress().trim()) {
      this.toast.error('Please enter your specific delivery address.');
      return;
    }

    this.submitting.set(true);

    const orderNum = 'TC-' + Math.floor(1000 + Math.random() * 9000);
    const summaryText = this.formatOrderSummary(orderNum);
    const total = this.cart.subtotal();
    const type = this.orderType();

    try {
      // 1. Send via Telegram Bot API
      const response = await fetch(`https://api.telegram.org/bot${TELEGRAM_BOT_TOKEN}/sendMessage`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          chat_id: TELEGRAM_CHAT_ID,
          text: summaryText,
          parse_mode: 'Markdown',
        }),
      });

      const resData = await response.json();

      if (resData.ok) {
        this.toast.success(`Order ${orderNum} sent directly to Terry Corner!`);
      } else {
        // If bot hasn't been started yet by the user, we inform them
        console.warn('Telegram Bot API response:', resData);
      }
    } catch (err) {
      console.error('Bot dispatch error:', err);
    } finally {
      this.lastOrderNumber.set(orderNum);
      this.lastOrderText.set(summaryText);
      this.lastOrderTotal.set(total);
      this.lastOrderType.set(type);
      this.orderPlaced.set(true);
      this.submitting.set(false);
      this.cart.clear();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  openTelegramChat(): void {
    const rawText = encodeURIComponent(
      `Hello Terry Corner! I just placed order #${this.lastOrderNumber()}.\n\n${this.lastOrderText()}`
    );
    window.open(`https://t.me/${TELEGRAM_USERNAME}?text=${rawText}`, '_blank');
  }

  openBotChat(): void {
    window.open(`https://t.me/${BOT_USERNAME}`, '_blank');
  }

  copySummary(): void {
    navigator.clipboard.writeText(this.lastOrderText()).then(() => {
      this.copied.set(true);
      this.toast.success('Order summary copied to clipboard!');
      setTimeout(() => this.copied.set(false), 3000);
    });
  }

  startNewOrder(): void {
    this.orderPlaced.set(false);
    this.customerName.set('');
    this.phoneNumber.set('');
    this.deliveryAddress.set('');
    this.notes.set('');
  }
}
