export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: 'Admin' | 'Customer' | 'ContentManager';
  createdAt: string;
  updatedAt: string;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface LoginResponse {
  user: User;
  tokens: AuthTokens;
}

export interface Category {
  id: string;
  name: string;
  slug: string;
  description?: string;
  parentId?: string;
  imageUrl?: string;
  isActive: boolean;
  sortOrder: number;
  productsCount: number;
  children?: Category[];
  createdAt: string;
  updatedAt: string;
}

export interface ProductImage {
  id: string;
  url: string;
  altText?: string;
  sortOrder: number;
  isPrimary: boolean;
}

export interface ProductVariant {
  id: string;
  sku: string;
  name: string;
  price: number;
  compareAtPrice?: number;
  stockQuantity: number;
  attributes: Record<string, string>;
  isActive: boolean;
}

export interface Product {
  id: string;
  name: string;
  slug: string;
  description?: string;
  shortDescription?: string;
  sku: string;
  price: number;
  compareAtPrice?: number;
  costPrice?: number;
  stockQuantity: number;
  lowStockThreshold: number;
  isActive: boolean;
  isFeatured: boolean;
  categoryId?: string;
  category?: Category;
  brandId?: string;
  images: ProductImage[];
  variants: ProductVariant[];
  tags: string[];
  metaTitle?: string;
  metaDescription?: string;
  averageRating: number;
  reviewCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  productImage?: string;
  variantId?: string;
  variantName?: string;
  sku: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface Address {
  firstName: string;
  lastName: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  phone: string;
}

export type OrderStatus =
  | 'Pending'
  | 'Confirmed'
  | 'Processing'
  | 'Shipped'
  | 'Delivered'
  | 'Cancelled'
  | 'Refunded';

export type PaymentStatus = 'Pending' | 'Paid' | 'Failed' | 'Refunded';

export interface Order {
  id: string;
  orderNumber: string;
  userId: string;
  customerName: string;
  customerEmail: string;
  status: OrderStatus;
  paymentStatus: PaymentStatus;
  paymentMethod: string;
  shippingAddress: Address;
  billingAddress: Address;
  items: OrderItem[];
  subtotal: number;
  shippingCost: number;
  taxAmount: number;
  discountAmount: number;
  totalAmount: number;
  notes?: string;
  trackingNumber?: string;
  shippingCarrier?: string;
  createdAt: string;
  updatedAt: string;
}

export interface Customer {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phone?: string;
  isEmailVerified: boolean;
  ordersCount: number;
  totalSpent: number;
  createdAt: string;
  lastLoginAt?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
  statusCode: number;
}

export interface DashboardStats {
  totalOrders: number;
  totalRevenue: number;
  totalProducts: number;
  totalCustomers: number;
  lowStockCount: number;
  pendingOrdersCount: number;
  recentOrders: Order[];
  salesByMonth: { month: string; revenue: number; orders: number }[];
}
