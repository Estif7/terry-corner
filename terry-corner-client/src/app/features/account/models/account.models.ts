export interface MyProfile {
  fullName: string;
  phoneNumber: string;
  email?: string;
  hasProfilePicture: boolean;
}

export interface MyOrderListItem {
  orderNumber: string;
  status: string;
  paymentStatus: string;
  total: number;
  createdAtUtc: string;
}

export interface MyOrderItemForReorder {
  productId: string;
  productName: string;
  quantity: number;
  toppingIds: string[];
}

export interface MyOrderDetail {
  orderNumber: string;
  status: string;
  total: number;
  createdAtUtc: string;
  items: MyOrderItemForReorder[];
}
