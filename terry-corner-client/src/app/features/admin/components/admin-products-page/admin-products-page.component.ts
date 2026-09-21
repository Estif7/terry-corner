import { Component, OnInit, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { ResolveImageUrlPipe } from '../../../../shared/pipes/resolve-image-url.pipe';
import { CatalogAdminService } from '../../services/catalog-admin.service';
import { AdminCategory, AdminTopping } from '../../models/admin.models';
import { MenuService } from '../../../menu/services/menu.service';
import { Product } from '../../../menu/models/menu.models';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'tc-admin-products-page',
  standalone: true,
  imports: [ReactiveFormsModule, DecimalPipe, ResolveImageUrlPipe],
  templateUrl: './admin-products-page.component.html',
  styleUrl: './admin-products-page.component.scss',
})
export class AdminProductsPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly catalog = inject(CatalogAdminService);
  private readonly menuService = inject(MenuService);
  private readonly toast = inject(ToastService);

  readonly products = signal<Product[]>([]);
  readonly categories = signal<AdminCategory[]>([]);
  readonly allToppings = signal<AdminTopping[]>([]);
  readonly loading = signal(true);
  readonly editingId = signal<string | null>(null);
  readonly saving = signal(false);
  readonly selectedToppingIds = signal<Set<string>>(new Set());
  readonly uploadingImage = signal(false);
  readonly imageError = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    description: ['', Validators.required],
    price: [0, [Validators.required, Validators.min(0.01)]],
    imageUrl: [''],
    categoryId: ['', Validators.required],
    isAvailable: [true],
    isFeatured: [false],
    isPopular: [false],
    sortOrder: [0],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.menuService.getProducts().subscribe({
      next: (products) => {
        this.products.set(products);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
    this.catalog.getCategories().subscribe({ next: (c) => this.categories.set(c), error: () => {} });
    this.catalog.getToppings().subscribe({ next: (t) => this.allToppings.set(t), error: () => {} });
  }

  toggleTopping(toppingId: string): void {
    this.selectedToppingIds.update((ids) => {
      const next = new Set(ids);
      next.has(toppingId) ? next.delete(toppingId) : next.add(toppingId);
      return next;
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
    this.selectedToppingIds.set(new Set());
    this.form.reset({
      name: '', description: '', price: 0, imageUrl: '', categoryId: '',
      isAvailable: true, isFeatured: false, isPopular: false, sortOrder: 0,
    });
  }

  startEdit(product: Product): void {
    this.editingId.set(product.id);
    this.selectedToppingIds.set(new Set(product.toppings.map((t) => t.id)));
    this.form.reset({
      name: product.name,
      description: product.description,
      price: product.price,
      imageUrl: product.imageUrl ?? '',
      categoryId: product.categoryId,
      isAvailable: product.isAvailable,
      isFeatured: product.isFeatured,
      isPopular: product.isPopular,
      sortOrder: 0,
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
    const toppingIds = Array.from(this.selectedToppingIds());
    this.saving.set(true);
    const editingId = this.editingId();

    const request$: Observable<unknown> = editingId === 'new'
      ? this.catalog.createProduct({ ...value, toppingIds })
      : this.catalog.updateProduct(editingId!, { ...value, toppingIds });

    request$.subscribe({
      next: () => {
        this.toast.success(editingId === 'new' ? 'Product created.' : 'Product updated.');
        this.saving.set(false);
        this.editingId.set(null);
        this.load();
      },
      error: () => this.saving.set(false),
    });
  }

  remove(product: Product): void {
    if (!confirm(`Delete product "${product.name}"?`)) return;

    this.catalog.deleteProduct(product.id).subscribe({
      next: () => {
        this.toast.success('Product deleted.');
        this.load();
      },
      error: () => {},
    });
  }
}
