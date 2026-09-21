export interface CreateOrderItem {
  productId: string;
  quantity: number;
  toppingIds: string[];
}

export interface CreateOrderRequest {
  contactFullName: string;
  contactPhoneNumber: string;
  orderType: 'Pickup' | 'Delivery';
  deliveryAddress?: string;
  orderNotes?: string;
  items: CreateOrderItem[];
}

export interface OrderItemResult {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  toppingNames: string[];
  lineTotal: number;
}

export interface OrderResult {
  id: string;
  orderNumber: string;
  status: string;
  paymentStatus: string;
  orderType: string;
  subtotal: number;
  discountAmount: number;
  total: number;
  items: OrderItemResult[];
  createdAtUtc: string;
}

export interface OrderStatusStep {
  status: string;
  isComplete: boolean;
  isActive: boolean;
}

export interface OrderTracking {
  orderNumber: string;
  status: string;
  paymentStatus: string;
  total: number;
  timeline: OrderStatusStep[];
  createdAtUtc: string;
}

export interface PaymentMethod {
  id: string;
  methodName: string;
  accountName: string;
  accountNumberOrIdentifier: string;
  phoneNumber?: string;
  instructions: string;
}

export interface PaymentReceiptResult {
  id: string;
  orderNumber: string;
  paymentStatus: string;
  submittedAtUtc: string;
}

/** Human-readable labels for each backend status value, in timeline order. */
export const ORDER_STATUS_LABELS: Record<string, string> = {
  OrderReceived: 'Order Received',
  PaymentPending: 'Payment Pending',
  PaymentReceiptSubmitted: 'Payment Receipt Submitted',
  PaymentVerification: 'Payment Verification',
  PreparingFood: 'Preparing Food',
  Ready: 'Ready',
  Completed: 'Completed',
};
