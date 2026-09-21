export interface CartToppingSelection {
  toppingId: string;
  name: string;
  price: number;
}

export interface CartItem {
  /** Client-generated id for this specific line (product + toppings combo), used for add/remove/update in the cart list. */
  lineId: string;
  productId: string;
  name: string;
  imageUrl: string;
  basePrice: number;
  quantity: number;
  toppings: CartToppingSelection[];
}

export function lineTotal(item: CartItem): number {
  const toppingsTotal = item.toppings.reduce((sum, t) => sum + t.price, 0);
  return (item.basePrice + toppingsTotal) * item.quantity;
}
