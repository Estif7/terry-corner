export interface PaymentReceiptReview {
  id: string;
  orderNumber: string;
  customerName: string;
  orderTotal: number;
  originalFileNameForDisplay: string;
  transactionReferenceNumber?: string;
  paymentNote?: string;
  submittedAtUtc: string;
}

export interface PopularProductStat {
  productName: string;
  timesOrdered: number;
}

export interface AdminOverview {
  todaysOrderCount: number;
  todaysRevenue: number;
  pendingPaymentReceipts: number;
  ordersPreparing: number;
  ordersReady: number;
  ordersCompletedToday: number;
  popularProducts: PopularProductStat[];
}

export interface AdminOrderListItem {
  id: string;
  orderNumber: string;
  customerName: string;
  status: string;
  paymentStatus: string;
  total: number;
  createdAtUtc: string;
}

export interface AdminOrderItem {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  toppingNames: string[];
  lineTotal: number;
}

export interface AdminOrderReceipt {
  id: string;
  status: string;
  transactionReferenceNumber?: string;
  submittedAtUtc: string;
}

export interface AdminOrderDetail {
  id: string;
  orderNumber: string;
  customerName: string;
  contactPhoneNumber: string;
  status: string;
  paymentStatus: string;
  orderType: string;
  deliveryAddress?: string;
  orderNotes?: string;
  internalStaffNotes?: string;
  subtotal: number;
  discountAmount: number;
  total: number;
  items: AdminOrderItem[];
  receipts: AdminOrderReceipt[];
  createdAtUtc: string;
}

export interface AdminCategory {
  id: string;
  name: string;
  description?: string;
  imageUrl?: string;
  sortOrder: number;
  isActive: boolean;
}

export interface AdminTopping {
  id: string;
  name: string;
  additionalPrice: number;
  isAvailable: boolean;
  sortOrder: number;
}

export interface AdminProduct {
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
  sortOrder: number;
  toppings: AdminTopping[];
}

export interface AdminPaymentMethod {
  id: string;
  methodName: string;
  accountName: string;
  accountNumberOrIdentifier: string;
  phoneNumber?: string;
  instructions: string;
  sortOrder: number;
  isActive: boolean;
}

export interface AdminPromotion {
  id: string;
  name: string;
  description?: string;
  discountType: 'Percentage' | 'FixedAmount';
  discountValue: number;
  startDateUtc: string;
  endDateUtc: string;
  isFeatured: boolean;
  isActive: boolean;
  productIds: string[];
}

export interface AdminUser {
  id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  roles: string[];
  isLockedOut: boolean;
  hasProfilePicture: boolean;
  createdAtUtc?: string;
}

export interface UserStats {
  totalUsers: number;
  customerCount: number;
  staffCount: number;
  managerCount: number;
  adminCount: number;
  lockedOutCount: number;
  newUsersLast7Days: number;
}
