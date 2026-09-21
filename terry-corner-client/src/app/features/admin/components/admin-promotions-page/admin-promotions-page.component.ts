import { Component, OnInit, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { DecimalPipe, DatePipe } from '@angular/common';
import { Observable } from 'rxjs';
import { CatalogAdminService } from '../../services/catalog-admin.service';
import { AdminPromotion } from '../../models/admin.models';
import { MenuService } from '../../../menu/services/menu.service';
import { Product } from '../../../menu/models/menu.models';
import { ToastService } from '../../../../core/services/toast.service';

function toDateTimeLocal(iso: string): string {
  return new Date(iso).toISOString().slice(0, 16);
}

@Component({
  selector: 'tc-admin-promotions-page',
  standalone: true,
  imports: [ReactiveFormsModule, DecimalPipe, DatePipe],
  templateUrl: './admin-promotions-page.component.html',
  styleUrl: './admin-promotions-page.component.scss',
})
export class AdminPromotionsPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly catalog = inject(CatalogAdminService);
  private readonly menuService = inject(MenuService);
  private readonly toast = inject(ToastService);

  readonly promotions = signal<AdminPromotion[]>([]);
  readonly products = signal<Product[]>([]);
  readonly loading = signal(true);
  readonly editingId = signal<string | null>(null);
  readonly saving = signal(false);
  readonly selectedProductIds = signal<Set<string>>(new Set());

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    description: [''],
    discountType: ['Percentage' as 'Percentage' | 'FixedAmount', Validators.required],
    discountValue: [0, [Validators.required, Validators.min(0.01)]],
    startDateUtc: ['', Validators.required],
    endDateUtc: ['', Validators.required],
    isFeatured: [false],
    isActive: [true],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.catalog.getPromotions().subscribe({
      next: (promotions) => {
        this.promotions.set(promotions);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
    this.menuService.getProducts().subscribe({ next: (p) => this.products.set(p), error: () => {} });
  }

  toggleProduct(productId: string): void {
    this.selectedProductIds.update((ids) => {
      const next = new Set(ids);
      next.has(productId) ? next.delete(productId) : next.add(productId);
      return next;
    });
  }

  startCreate(): void {
    this.editingId.set('new');
    this.selectedProductIds.set(new Set());
    this.form.reset({
      name: '', description: '', discountType: 'Percentage', discountValue: 0,
      startDateUtc: '', endDateUtc: '', isFeatured: false, isActive: true,
    });
  }

  startEdit(promotion: AdminPromotion): void {
    this.editingId.set(promotion.id);
    this.selectedProductIds.set(new Set(promotion.productIds));
    this.form.reset({
      name: promotion.name,
      description: promotion.description ?? '',
      discountType: promotion.discountType,
      discountValue: promotion.discountValue,
      startDateUtc: toDateTimeLocal(promotion.startDateUtc),
      endDateUtc: toDateTimeLocal(promotion.endDateUtc),
      isFeatured: promotion.isFeatured,
      isActive: promotion.isActive,
    });
  }

  cancel(): void {
    this.editingId.set(null);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const payload = {
      ...raw,
      startDateUtc: new Date(raw.startDateUtc).toISOString(),
      endDateUtc: new Date(raw.endDateUtc).toISOString(),
      productIds: Array.from(this.selectedProductIds()),
    };

    this.saving.set(true);
    const editingId = this.editingId();

    const request$: Observable<unknown> = editingId === 'new'
      ? this.catalog.createPromotion(payload)
      : this.catalog.updatePromotion(editingId!, payload);

    request$.subscribe({
      next: () => {
        this.toast.success(editingId === 'new' ? 'Promotion created.' : 'Promotion updated.');
        this.saving.set(false);
        this.editingId.set(null);
        this.load();
      },
      error: () => this.saving.set(false),
    });
  }

  remove(promotion: AdminPromotion): void {
    if (!confirm(`Delete promotion "${promotion.name}"?`)) return;

    this.catalog.deletePromotion(promotion.id).subscribe({
      next: () => {
        this.toast.success('Promotion deleted.');
        this.load();
      },
      error: () => {},
    });
  }
}
