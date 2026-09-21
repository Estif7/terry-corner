import { Component, OnInit, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { CatalogAdminService } from '../../services/catalog-admin.service';
import { AdminTopping } from '../../models/admin.models';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'tc-admin-toppings-page',
  standalone: true,
  imports: [ReactiveFormsModule, DecimalPipe],
  templateUrl: './admin-toppings-page.component.html',
  styleUrl: './admin-toppings-page.component.scss',
})
export class AdminToppingsPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly catalog = inject(CatalogAdminService);
  private readonly toast = inject(ToastService);

  readonly toppings = signal<AdminTopping[]>([]);
  readonly loading = signal(true);
  readonly editingId = signal<string | null>(null);
  readonly saving = signal(false);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    additionalPrice: [0, [Validators.required, Validators.min(0)]],
    sortOrder: [0],
    isAvailable: [true],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.catalog.getToppings().subscribe({
      next: (toppings) => {
        this.toppings.set(toppings);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  startCreate(): void {
    this.editingId.set('new');
    this.form.reset({ name: '', additionalPrice: 0, sortOrder: 0, isAvailable: true });
  }

  startEdit(topping: AdminTopping): void {
    this.editingId.set(topping.id);
    this.form.reset({
      name: topping.name,
      additionalPrice: topping.additionalPrice,
      sortOrder: topping.sortOrder,
      isAvailable: topping.isAvailable,
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

    const value = this.form.getRawValue();
    this.saving.set(true);
    const editingId = this.editingId();

    const request$: Observable<unknown> = editingId === 'new'
      ? this.catalog.createTopping(value)
      : this.catalog.updateTopping(editingId!, value);

    request$.subscribe({
      next: () => {
        this.toast.success(editingId === 'new' ? 'Topping created.' : 'Topping updated.');
        this.saving.set(false);
        this.editingId.set(null);
        this.load();
      },
      error: () => this.saving.set(false),
    });
  }

  remove(topping: AdminTopping): void {
    if (!confirm(`Delete topping "${topping.name}"?`)) return;

    this.catalog.deleteTopping(topping.id).subscribe({
      next: () => {
        this.toast.success('Topping deleted.');
        this.load();
      },
      error: () => {},
    });
  }
}
