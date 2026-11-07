import type { ApiErrorShape, PagedResult } from "../types/api";

export const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || "").replace(/\/$/, "");

const ACCESS_KEY = "aurora_access_token";
const REFRESH_KEY = "aurora_refresh_token";

export class ApiError extends Error {
  status: number;
  validation?: Record<string, string[]>;
  constructor(message: string, status: number, validation?: Record<string, string[]>) {
    super(message);
    this.status = status;
    this.validation = validation;
  }
}

export const tokenStore = {
  getAccess: () => localStorage.getItem(ACCESS_KEY),
  getRefresh: () => localStorage.getItem(REFRESH_KEY),
  set: (access?: string, refresh?: string) => {
    if (access) localStorage.setItem(ACCESS_KEY, access);
    if (refresh) localStorage.setItem(REFRESH_KEY, refresh);
  },
  clear: () => {
    localStorage.removeItem(ACCESS_KEY);
    localStorage.removeItem(REFRESH_KEY);
  },
};

async function parseError(response: Response) {
  let body: ApiErrorShape | undefined;
  try {
    body = await response.json();
  } catch {
    // The API can return an empty body for authorization failures.
  }
  const validation = body?.errors;
  const firstValidation = validation && Object.values(validation).flat()[0];
  return new ApiError(
    firstValidation || body?.message || body?.title || `Yêu cầu thất bại (${response.status})`,
    response.status,
    validation,
  );
}

let refreshPromise: Promise<boolean> | null = null;

async function refreshSession() {
  const refreshToken = tokenStore.getRefresh();
  if (!refreshToken) return false;
  const response = await fetch(`${API_BASE_URL}/api/Auth/refresh`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ refreshToken }),
  });
  if (!response.ok) {
    tokenStore.clear();
    return false;
  }
  const data = (await response.json()) as {
    accessToken?: string;
    token?: string;
    refreshToken?: string;
  };
  const accessToken = data.accessToken || data.token;
  if (!accessToken || !data.refreshToken) {
    tokenStore.clear();
    throw new ApiError("API không trả về đủ token để làm mới phiên đăng nhập.", response.status);
  }
  tokenStore.set(accessToken, data.refreshToken);
  return true;
}

type ApiRecord = Record<string, unknown>;

function asRecord(value: unknown): ApiRecord | undefined {
  return value !== null && typeof value === "object" && !Array.isArray(value)
    ? (value as ApiRecord)
    : undefined;
}

function mapPagedItems(value: unknown, mapper: (item: unknown) => unknown): unknown {
  if (Array.isArray(value)) return value.map(mapper);
  const source = asRecord(value);
  if (!source) return value;

  const key = ["items", "data", "results"].find((name) => Array.isArray(source[name]));
  return key ? { ...source, [key]: (source[key] as unknown[]).map(mapper) } : value;
}

function mapRoom(value: unknown): unknown {
  const room = asRecord(value);
  if (!room) return value;
  const roomType = asRecord(room.roomType);
  const amenities = Array.isArray(room.amenities)
    ? room.amenities.map((amenity, index) =>
        typeof amenity === "string" ? { id: index, name: amenity } : amenity,
      )
    : [];
  const roomTypeName = room.roomTypeName ?? roomType?.name;
  const price = room.pricePerNight ?? room.price ?? room.basePrice;

  return {
    ...room,
    amenityIds: room.amenityIds ?? [],
    capacity: room.capacity ?? roomType?.capacity,
    price,
    basePrice: room.basePrice ?? price,
    roomType:
      roomType ??
      (roomTypeName
        ? { name: roomTypeName, capacity: room.capacity, basePrice: price, description: room.description }
        : undefined),
    amenities,
  };
}

function mapRoomType(value: unknown): unknown {
  const roomType = asRecord(value);
  return roomType
    ? {
        ...roomType,
        basePrice: roomType.basePrice ?? roomType.pricePerNight,
      }
    : value;
}

function mapBooking(value: unknown): unknown {
  const booking = asRecord(value);
  if (!booking) return value;
  const room = asRecord(booking.room);
  const customer = asRecord(booking.customer);
  const roomTypeName = booking.roomTypeName ?? asRecord(room?.roomType)?.name;
  const services = Array.isArray(booking.services)
    ? booking.services.map((serviceValue) => {
        const service = asRecord(serviceValue);
        if (!service) return serviceValue;
        return {
          ...service,
          id: service.id ?? service.extraServiceId,
          price: service.price ?? service.unitPrice,
        };
      })
    : [];

  return {
    ...booking,
    checkIn: booking.checkIn ?? booking.checkInDate,
    checkOut: booking.checkOut ?? booking.checkOutDate,
    totalAmount: booking.totalAmount ?? booking.totalPrice,
    balance: booking.balance ?? booking.balanceDue,
    bookingCode: booking.bookingCode ?? booking.id,
    customer:
      customer ??
      (booking.customerId !== undefined || booking.customerName !== undefined
        ? { id: booking.customerId, fullName: booking.customerName }
        : undefined),
    room:
      room ??
      (booking.roomId !== undefined || booking.roomNumber !== undefined
        ? { id: booking.roomId, roomNumber: booking.roomNumber, roomType: { name: roomTypeName } }
        : undefined),
    services,
  };
}

function mapPaymentClaim(value: unknown): unknown {
  const claim = asRecord(value);
  if (!claim) return value;
  const booking = asRecord(claim.booking);
  return {
    ...claim,
    createdAt: claim.createdAt ?? claim.customerConfirmedAt,
    booking:
      booking ??
      (claim.bookingId !== undefined
        ? {
            id: claim.bookingId,
            bookingCode: claim.bookingId,
            customer: { fullName: claim.customerName },
            room: { roomNumber: claim.roomNumber },
            checkIn: claim.checkInDate,
            checkOut: claim.checkOutDate,
          }
        : undefined),
  };
}

function adaptResponse(path: string, value: unknown): unknown {
  const pathname = path.split("?")[0].replace(/\/$/, "").toLowerCase();
  if (pathname === "/api/rooms") return mapPagedItems(value, mapRoom);
  if (/^\/api\/rooms\/\d+$/.test(pathname)) return mapRoom(value);
  if (pathname === "/api/bookings/available-rooms") return mapPagedItems(value, mapRoom);
  if (pathname === "/api/roomtypes") return mapPagedItems(value, mapRoomType);
  if (/^\/api\/roomtypes\/\d+$/.test(pathname)) return mapRoomType(value);
  if (pathname === "/api/bookings") return mapPagedItems(value, mapBooking);
  if (/^\/api\/bookings\/\d+$/.test(pathname)) return mapBooking(value);
  if (pathname === "/api/paymentclaims/pending")
    return mapPagedItems(value, mapPaymentClaim);
  if (pathname === "/api/pricingrules/quote") {
    const quote = asRecord(value);
    if (!quote) return value;
    const quoteItems = Array.isArray(quote.items) ? quote.items : [];
    return {
      ...quote,
      nights: quote.nights ?? quoteItems,
      total: quote.total ?? quote.roomCharge,
      totalAmount: quote.totalAmount ?? quote.roomCharge,
    };
  }
  if (/^\/api\/reviews\/room\/\d+$/.test(pathname)) {
    const result = asRecord(value);
    const page = asRecord(result?.reviews);
    return page
      ? {
          ...page,
          pageNumber: page.pageNumber ?? page.page,
          totalCount: page.totalCount ?? page.totalItems,
          averageRating: result?.averageRating,
          totalReviews: result?.totalReviews,
        }
      : value;
  }
  return value;
}

export async function api<T>(path: string, init: RequestInit = {}, retry = true): Promise<T> {
  const token = tokenStore.getAccess();
  const headers = new Headers(init.headers);
  headers.set("Accept", "application/json");
  if (
    init.body &&
    typeof FormData !== "undefined" &&
    !(init.body instanceof FormData) &&
    !headers.has("Content-Type")
  ) {
    headers.set("Content-Type", "application/json");
  }
  if (token && !headers.has("Authorization")) headers.set("Authorization", `Bearer ${token}`);

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers,
  });
  if (response.status === 401 && retry && tokenStore.getRefresh()) {
    refreshPromise ||= refreshSession().finally(() => (refreshPromise = null));
    if (await refreshPromise) return api<T>(path, init, false);
  }
  if (!response.ok) throw await parseError(response);
  if (response.status === 204 || response.status === 205) return undefined as T;
  const body = await response.text();
  if (!body) return undefined as T;
  try {
    return adaptResponse(path, JSON.parse(body)) as T;
  } catch (reason) {
    if (reason instanceof SyntaxError)
      throw new ApiError("API trả về dữ liệu không đúng định dạng JSON.", response.status);
    throw reason;
  }
}

export const queryString = (params: Record<string, string | number | undefined | null>) => {
  const search = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "")
      search.set(key === "pageNumber" ? "page" : key, String(value));
  });
  const value = search.toString();
  return value ? `?${value}` : "";
};

export function normalizePaged<T>(value: unknown): PagedResult<T> {
  if (Array.isArray(value)) {
    return {
      items: value,
      pageNumber: 1,
      pageSize: value.length,
      totalCount: value.length,
      totalPages: 1,
    };
  }
  const source = (value || {}) as Record<string, unknown>;
  const items = (source.items || source.data || source.results || []) as T[];
  const pageSize = Number(source.pageSize || items.length || 10);
  const totalCount = Number(source.totalCount ?? source.totalItems ?? source.total ?? items.length);
  return {
    items,
    pageNumber: Number(source.pageNumber || source.page || 1),
    pageSize,
    totalCount,
    totalPages: Number(source.totalPages || Math.max(1, Math.ceil(totalCount / pageSize))),
  };
}
