import { Component, OnInit, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { CatalogAdminService } from '../../services/catalog-admin.service';
import { ResolveImageUrlPipe } from '../../../../shared/pipes/resolve-image-url.pipe';
import { AdminCategory } from '../../models/admin.models';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'tc-admin-categories-page',
  standalone: true,
  imports: [ReactiveFormsModule, ResolveImageUrlPipe],
  templateUrl: './admin-categories-page.component.html',
  styleUrl: './admin-categories-page.component.scss',
})
export class AdminCategoriesPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly catalog = inject(CatalogAdminService);
  private readonly toast = inject(ToastService);

  readonly categories = signal<AdminCategory[]>([]);
  readonly loading = signal(true);
  readonly editingId = signal<string | null>(null);
  readonly saving = signal(false);
  readonly uploadingImage = signal(false);
  readonly imageError = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: [''],
    imageUrl: [''],
    sortOrder: [0],
    isActive: [true],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.catalog.getCategories().subscribe({
      next: (categories) => {
        this.categories.set(categories);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.uploadingImage.set(true);
    this.imageError.set(null);

    this.catalog.uploadImage(file).subscribe({
      next: ({ url }) => {
        this.form.controls.imageUrl.setValue(url);
        this.uploadingImage.set(false);
      },
      error: () => {
        this.imageError.set("Couldn't upload that image. Try a JPG, PNG, or WEBP under 5 MB.");
        this.uploadingImage.set(false);
      },
    });

    input.value = '';
  }

  startCreate(): void {
    this.editingId.set('new');
    this.form.reset({ name: '', description: '', imageUrl: '', sortOrder: 0, isActive: true });
  }

  startEdit(category: AdminCategory): void {
    this.editingId.set(category.id);
    this.form.reset({
      name: category.name,
      description: category.description ?? '',
      imageUrl: category.imageUrl ?? '',
      sortOrder: category.sortOrder,
      isActive: category.isActive,
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
      ? this.catalog.createCategory(value)
      : this.catalog.updateCategory(editingId!, value);

    request$.subscribe({
      next: () => {
        this.toast.success(editingId === 'new' ? 'Category created.' : 'Category updated.');
        this.saving.set(false);
        this.editingId.set(null);
        this.load();
      },
      error: () => this.saving.set(false),
    });
  }

  remove(category: AdminCategory): void {
    if (!confirm(`Delete category "${category.name}"?`)) return;

    this.catalog.deleteCategory(category.id).subscribe({
      next: () => {
        this.toast.success('Category deleted.');
        this.load();
      },
      error: () => {},
    });
  }
}
