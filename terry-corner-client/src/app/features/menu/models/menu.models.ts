export interface Category {
  id: string;
  name: string;
  description?: string;
  imageUrl?: string;
  sortOrder: number;
}

export interface Topping {
  id: string;
  name: string;
  additionalPrice: number;
  isAvailable: boolean;
}

export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  imageUrl?: string;
  isAvailable: boolean;
  isFeatured: boolean;
  isPopular: boolean;
  categoryId: string;
  categoryName: string;
  toppings: Topping[];
}

export interface Promotion {
  id: string;
  name: string;
  description?: string;
  discountType: 'Percentage' | 'FixedAmount';
  discountValue: number;
  isFeatured: boolean;
  productIds: string[];
}
