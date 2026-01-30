// ============================================
// User & Auth Types
// ============================================
export type UserRole = 'Customer' | 'Admin' | 'ContentManager' | 'SuperAdmin';
export type UserRoleNumber = 0 | 1 | 2 | 3;

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  phoneNumber?: string;
  profilePictureUrl?: string;
  createdAt: string;
  updatedAt: string;
}

export interface UserFromApi {
  id: string;
  email: { value: string };
  firstName: string;
  lastName: string;
  role: UserRoleNumber;
  createdAt: string;
  updatedAt: string;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface LoginApiResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
  user: UserFromApi;
}

// ============================================
// Product Types (matches ProductDto.cs)
// ============================================
export interface ProductImage {
  id: string;
  productId: string;
  url: string;
  altText?: string;
  sortOrder: number;
  isPrimary: boolean;
  thumbnailSmallUrl?: string;
  thumbnailMediumUrl?: string;
  thumbnailLargeUrl?: string;
  webPUrl?: string;
  fileSize: number;
  width: number;
  height: number;
  originalFileName?: string;
}

export interface ProductVariant {
  id: string;
  productId: string;
  name: string;
  sku: string;
  price?: number;
  currency?: string;
  comparePrice?: number;
  stockQuantity: number;
  trackQuantity: boolean;
  isActive: boolean;
  color?: string;
  size?: string;
  material?: string;
  weight?: number;
  imageUrl?: string;
  isInStock: boolean;
}

export interface Product {
  id: string;
  name: string;
  slug: string;
  description?: string;
  shortDescription?: string;
  sku: string;
  price: number;
  currency: string;
  compareAtPrice?: number;
  costPrice?: number;
  stockQuantity: number;
  lowStockThreshold: number;
  trackQuantity: boolean;
  isActive: boolean;
  isFeatured: boolean;
  weight: number;
  weightUnit?: string;
  brand?: string;
  metaTitle?: string;
  metaDescription?: string;
  categoryId: string;
  categoryName: string;
  createdAt: string;
  updatedAt: string;
  images: ProductImage[];
  variants: ProductVariant[];
  tags: string[];
  // Computed properties from backend
  isInStock: boolean;
  isLowStock: boolean;
  discountPercentage?: number;
  mainImageUrl: string;
}

// ============================================
// Category Types (matches CategoryDto.cs)
// ============================================
export interface Category {
  id: string;
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;
  isActive: boolean;
  sortOrder: number;
  parentCategoryId?: string;
  parentCategoryName?: string;
  subCategories: Category[];
  productCount: number;
  createdAt: string;
  updatedAt: string;
}

// ============================================
// Cart Types (matches GetCartQuery.cs)
// ============================================
export interface CartItem {
  id: string;
  productId: string;
  productName: string;
  productSlug: string;
  productImage?: string;
  variantId?: string;
  variantName?: string;
  variantColor?: string;
  variantSize?: string;
  quantity: number;
  unitPrice: number;
  subTotal: number;
  currency: string;
  addedAt: string;
}

export interface Cart {
  id: string;
  userId?: string;
  sessionId?: string;
  items: CartItem[];
  totalItems: number;
  subTotal: number;
  totalAmount: number;
  currency: string;
  lastModified: string;
  expiresAt?: string;
}

export interface CartSummary {
  totalItems: number;
  totalAmount: number;
}

// ============================================
// Order Types (matches GetOrderByIdQuery.cs)
// ============================================
export type OrderStatus =
  | 'Pending'
  | 'Confirmed'
  | 'Processing'
  | 'Shipped'
  | 'OutForDelivery'
  | 'Delivered'
  | 'Cancelled'
  | 'Returned'
  | 'Refunded';

export type PaymentMethod =
  | 'CreditCard'
  | 'DebitCard'
  | 'BankTransfer'
  | 'CashOnDelivery'
  | 'DigitalWallet';

export type PaymentStatus =
  | 'Pending'
  | 'Processing'
  | 'Completed'
  | 'Failed'
  | 'Cancelled'
  | 'Refunded'
  | 'PartialRefund';

export interface OrderAddress {
  firstName: string;
  lastName: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  stateProvince: string;
  postalCode: string;
  country: string;
  phone?: string;
}

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  productSku: string;
  productImage?: string;
  variantId?: string;
  variantName?: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  currency: string;
}

export interface OrderPayment {
  id: string;
  amount: number;
  currency: string;
  method: PaymentMethod;
  status: PaymentStatus;
  reference?: string;
  createdAt: string;
}

export interface Order {
  id: string;
  orderNumber: string;
  userId?: string;
  customerEmail: string;
  customerPhone?: string;
  status: OrderStatus;
  totalAmount: number;
  currency: string;
  createdAt: string;
  shippedAt?: string;
  deliveredAt?: string;
  trackingNumber?: string;
  shippingCarrier?: string;
  notes?: string;
  shippingAddress: OrderAddress;
  billingAddress: OrderAddress;
  items: OrderItem[];
  payments: OrderPayment[];
}

// ============================================
// Review Types (matches GetReviewsByProductResponse.cs)
// ============================================
export type ReviewStatus = 'Published' | 'Pending' | 'Rejected' | 'Flagged';

export interface Review {
  id: string;
  productId: string;
  userId: string;
  userName: string;
  rating: number;
  title: string;
  comment: string;
  isVerifiedPurchase: boolean;
  status: string;
  createdAt: string;
  updatedAt: string;
}

export interface RatingDistribution {
  fiveStar: number;
  fourStar: number;
  threeStar: number;
  twoStar: number;
  oneStar: number;
}

export interface ReviewStats {
  averageRating: number;
  totalReviews: number;
  verifiedPurchaseReviews: number;
  ratingDistribution: RatingDistribution;
}

// ============================================
// Wishlist Types (matches CheckWishlistStatusResponse.cs)
// ============================================
export interface WishlistStatus {
  isInWishlist: boolean;
  wishlistIds: string[];
}

// ============================================
// User Profile Types
// ============================================
export interface UserProfile {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  profilePictureUrl?: string;
  dateOfBirth?: string;
  gender?: string;
  bio?: string;
  addresses: OrderAddress[];
  createdAt: string;
  updatedAt: string;
}

// ============================================
// API Response Types
// ============================================
export interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
  statusCode: number;
}

// ============================================
// Filter & Query Types
// ============================================
export interface ProductFilters {
  searchTerm?: string;
  categoryId?: string;
  minPrice?: number;
  maxPrice?: number;
  brand?: string;
  tags?: string[];
  featuredOnly?: boolean;
  page?: number;
  pageSize?: number;
}

export interface PriceRange {
  minPrice: number;
  maxPrice: number;
}

// ============================================
// Utility Types
// ============================================
export const USER_ROLE_MAP: Record<UserRoleNumber, UserRole> = {
  0: 'Customer',
  1: 'Admin',
  2: 'ContentManager',
  3: 'SuperAdmin',
};

export function mapUserFromApi(apiUser: UserFromApi): User {
  return {
    id: apiUser.id,
    email: apiUser.email.value,
    firstName: apiUser.firstName,
    lastName: apiUser.lastName,
    role: USER_ROLE_MAP[apiUser.role],
    createdAt: apiUser.createdAt,
    updatedAt: apiUser.updatedAt,
  };
}
