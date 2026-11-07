export type Role = "Customer" | "Staff" | "Admin";

export interface User {
  id?: number | string;
  userId?: number | string;
  username: string;
  fullName?: string;
  email?: string;
  role: Role;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface RoomType {
  id: number;
  name: string;
  description?: string;
  capacity?: number;
  basePrice?: number;
}

export interface Amenity {
  id: number;
  name: string;
  icon?: string;
}

export interface Room {
  id: number;
  roomNumber: string;
  floor?: number;
  capacity?: number;
  description?: string;
  imageUrl?: string;
  images?: string[];
  roomTypeId?: number;
  roomType?: RoomType;
  amenities?: Amenity[];
  amenityIds?: number[];
  price?: number;
  basePrice?: number;
  status?: string;
}

export interface QuoteNight {
  date: string;
  price: number;
  basePrice?: number;
  adjustment?: number;
}

export interface PriceQuote {
  roomId: number;
  nights?: QuoteNight[];
  numberOfNights?: number;
  subtotal?: number;
  total?: number;
  totalAmount?: number;
}

export interface ReviewPage extends PagedResult<Review> {
  averageRating: number;
  totalReviews: number;
}

export interface ExtraService {
  id: number;
  name: string;
  description?: string;
  price: number;
  isActive?: boolean;
}

export interface Review {
  id: number;
  rating: number;
  comment?: string;
  customerName?: string;
  createdAt?: string;
}

export interface Booking {
  id: number;
  bookingCode?: string;
  roomId?: number;
  room?: Room;
  customerId?: number;
  customer?: { id: number; fullName?: string; email?: string; phone?: string };
  checkIn: string;
  checkOut: string;
  status: string;
  paymentStatus?: string;
  totalAmount?: number;
  totalPaid?: number;
  balance?: number;
  services?: Array<{ id?: number; name?: string; quantity?: number; price?: number }>;
  createdAt?: string;
}

export interface PaymentClaim {
  id: number;
  bookingId: number;
  booking?: Booking;
  amount: number;
  method: string;
  note?: string;
  status: string;
  createdAt?: string;
}

export interface ApiErrorShape {
  message?: string;
  title?: string;
  errors?: Record<string, string[]>;
}
