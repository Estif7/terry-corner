import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { DecimalPipe, DatePipe } from '@angular/common';
import { AccountService } from '../../services/account.service';
import { MyOrderListItem, MyProfile } from '../../models/account.models';
import { AuthService } from '../../../../core/services/auth.service';
import { CartService } from '../../../cart/services/cart.service';
import { MenuService } from '../../../menu/services/menu.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'tc-account-page',
  standalone: true,
  imports: [ReactiveFormsModule, DecimalPipe, DatePipe],
  templateUrl: './account-page.component.html',
  styleUrl: './account-page.component.scss',
})
export class AccountPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly accountService = inject(AccountService);
  private readonly auth = inject(AuthService);
  private readonly cart = inject(CartService);
  private readonly menuService = inject(MenuService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly profile = signal<MyProfile | null>(null);
  readonly orders = signal<MyOrderListItem[]>([]);
  readonly loading = signal(true);
  readonly editingProfile = signal(false);
  readonly savingProfile = signal(false);
  readonly reorderingNumber = signal<string | null>(null);
  readonly profilePictureUrl = signal<string | null>(null);
  readonly uploadingPicture = signal(false);

  readonly profileForm = this.fb.nonNullable.group({
    fullName: ['', Validators.required],
    phoneNumber: ['', Validators.required],
  });

  ngOnInit(): void {
    this.accountService.getProfile().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.profileForm.reset({ fullName: profile.fullName, phoneNumber: profile.phoneNumber });
        this.loading.set(false);

        if (profile.hasProfilePicture) {
          this.accountService.getProfilePictureUrl().subscribe({
            next: (url) => this.profilePictureUrl.set(url),
            error: () => {},
          });
        }
      },
      error: () => this.loading.set(false),
    });

    this.accountService.getOrders().subscribe({
      next: (orders) => this.orders.set(orders),
      error: () => {},
    });
  }

  onPictureSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.uploadingPicture.set(true);
    this.accountService.uploadProfilePicture(file).subscribe({
      next: () => {
        this.accountService.getProfilePictureUrl().subscribe({
          next: (url) => {
            this.profilePictureUrl.set(url);
            this.uploadingPicture.set(false);
            this.toast.success('Profile picture updated.');
          },
          error: () => this.uploadingPicture.set(false),
        });
      },
      error: () => this.uploadingPicture.set(false),
    });

    input.value = '';
  }

  removePicture(): void {
    if (!confirm('Remove your profile picture?')) return;

    this.accountService.deleteProfilePicture().subscribe({
      next: () => {
        this.profilePictureUrl.set(null);
        this.toast.success('Profile picture removed.');
      },
      error: () => {},
    });
  }

  startEditProfile(): void {
    this.editingProfile.set(true);
  }

  saveProfile(): void {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    const { fullName, phoneNumber } = this.profileForm.getRawValue();
    this.savingProfile.set(true);

    this.accountService.updateProfile(fullName, phoneNumber).subscribe({
      next: () => {
        this.profile.update((p) => (p ? { ...p, fullName, phoneNumber } : p));
        this.toast.success('Profile updated.');
        this.savingProfile.set(false);
        this.editingProfile.set(false);
      },
      error: () => this.savingProfile.set(false),
    });
  }

  reorder(orderNumber: string): void {
    this.reorderingNumber.set(orderNumber);

    this.accountService.getOrderDetail(orderNumber).subscribe({
      next: (detail) => {
        let remaining = detail.items.length;
        if (remaining === 0) {
          this.reorderingNumber.set(null);
          return;
        }

        detail.items.forEach((item) => {
          this.menuService.getProduct(item.productId).subscribe({
            next: (product) => {
              this.cart.add({
                lineId: crypto.randomUUID(),
                productId: product.id,
                name: product.name,
                imageUrl: product.imageUrl ?? '',
                basePrice: product.price,
                quantity: item.quantity,
                toppings: product.toppings
                  .filter((t) => item.toppingIds.includes(t.id))
                  .map((t) => ({ toppingId: t.id, name: t.name, price: t.additionalPrice })),
              });
              remaining -= 1;
              if (remaining === 0) {
                this.reorderingNumber.set(null);
                this.toast.success('Added your previous order to the cart.');
                this.router.navigate(['/cart']);
              }
            },
            error: () => {
              remaining -= 1;
              if (remaining === 0) this.reorderingNumber.set(null);
            },
          });
        });
      },
      error: () => this.reorderingNumber.set(null),
    });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/']);
  }
}
