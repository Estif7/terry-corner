import { Component, OnInit, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { CatalogAdminService } from '../../services/catalog-admin.service';
import { AdminPaymentMethod } from '../../models/admin.models';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'tc-admin-payment-settings-page',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './admin-payment-settings-page.component.html',
  styleUrl: './admin-payment-settings-page.component.scss',
})
export class AdminPaymentSettingsPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly catalog = inject(CatalogAdminService);
  private readonly toast = inject(ToastService);

  readonly methods = signal<AdminPaymentMethod[]>([]);
  readonly loading = signal(true);
  readonly editingId = signal<string | null>(null);
  readonly saving = signal(false);

  readonly form = this.fb.nonNullable.group({
    methodName: ['', [Validators.required, Validators.maxLength(100)]],
    accountName: ['', [Validators.required, Validators.maxLength(150)]],
    accountNumberOrIdentifier: ['', [Validators.required, Validators.maxLength(100)]],
    phoneNumber: [''],
    instructions: ['', Validators.required],
    sortOrder: [0],
    isActive: [true],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.catalog.getPaymentMethods().subscribe({
      next: (methods) => {
        this.methods.set(methods);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  startCreate(): void {
    this.editingId.set('new');
    this.form.reset({
      methodName: '', accountName: '', accountNumberOrIdentifier: '',
      phoneNumber: '', instructions: '', sortOrder: 0, isActive: true,
    });
  }

  startEdit(method: AdminPaymentMethod): void {
    this.editingId.set(method.id);
    this.form.reset({
      methodName: method.methodName,
      accountName: method.accountName,
      accountNumberOrIdentifier: method.accountNumberOrIdentifier,
      phoneNumber: method.phoneNumber ?? '',
      instructions: method.instructions,
      sortOrder: method.sortOrder,
      isActive: method.isActive,
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
      ? this.catalog.createPaymentMethod(value)
      : this.catalog.updatePaymentMethod(editingId!, value);

    request$.subscribe({
      next: () => {
        this.toast.success(editingId === 'new' ? 'Payment method created.' : 'Payment method updated.');
        this.saving.set(false);
        this.editingId.set(null);
        this.load();
      },
      error: () => this.saving.set(false),
    });
  }

  remove(method: AdminPaymentMethod): void {
    if (!confirm(`Delete payment method "${method.methodName}"?`)) return;

    this.catalog.deletePaymentMethod(method.id).subscribe({
      next: () => {
        this.toast.success('Payment method deleted.');
        this.load();
      },
      error: () => {},
    });
  }
}
