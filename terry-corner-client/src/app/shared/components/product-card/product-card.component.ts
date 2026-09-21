import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ResolveImageUrlPipe } from '../../pipes/resolve-image-url.pipe';
import { Product } from '../../../features/menu/models/menu.models';

@Component({
  selector: 'tc-product-card',
  standalone: true,
  imports: [DecimalPipe, ResolveImageUrlPipe],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.scss',
})
export class ProductCardComponent {
  @Input({ required: true }) product!: Product;
  @Output() select = new EventEmitter<Product>();

  onSelect(): void {
    if (this.product.isAvailable) {
      this.select.emit(this.product);
    }
  }
}
