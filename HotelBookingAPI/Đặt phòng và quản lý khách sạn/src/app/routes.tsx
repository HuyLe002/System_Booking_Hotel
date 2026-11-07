import { createBrowserRouter } from "react-router";
import { RouteGuard } from "../auth/RouteGuard";
import {
  AccountLayout,
  BookingDetailPage,
  BookingsListPage,
  PasswordPage,
} from "../pages/AccountPage";
import { AdminResourcesPage, AuditLogsPage } from "../pages/AdminPages";
import { LoginPage, RegisterPage } from "../pages/AuthPages";
import { BookingPage } from "../pages/BookingPage";
import { HomePage } from "../pages/HomePage";
import {
  CustomersPage,
  OperationsBookingDetail,
  OperationsBookings,
  OperationsDashboard,
  PaymentClaimsPage,
  StaffCreateBookingPage,
} from "../pages/OperationsPages";
import { RoomDetailPage } from "../pages/RoomDetailPage";
import { RoomsPage } from "../pages/RoomsPage";
import { ExperiencePage, NotFoundPage } from "../pages/StaticPage";
import { DashboardLayout } from "../layouts/DashboardLayout";
import { PublicLayout } from "../layouts/PublicLayout";

const routes = [
  {
    Component: PublicLayout,
    children: [
      { index: true, Component: HomePage },
      { path: "phong", Component: RoomsPage },
      { path: "phong/:id", Component: RoomDetailPage },
      { path: "gioi-thieu", Component: ExperiencePage },
      {
        element: <RouteGuard roles={["Customer"]} />,
        children: [
          { path: "dat-phong/:id", Component: BookingPage },
          {
            path: "tai-khoan",
            Component: AccountLayout,
            children: [
              { index: true, Component: BookingsListPage },
              { path: "booking/:id", Component: BookingDetailPage },
              { path: "doi-mat-khau", Component: PasswordPage },
            ],
          },
        ],
      },
      { path: "*", Component: NotFoundPage },
    ],
  },
  { path: "dang-nhap", Component: LoginPage },
  { path: "dang-ky", Component: RegisterPage },
  {
    element: <RouteGuard roles={["Staff", "Admin"]} />,
    children: [
      {
        path: "van-hanh",
        element: <DashboardLayout />,
        children: [
          { index: true, Component: OperationsDashboard },
          { path: "booking", element: <OperationsBookings prefix="/van-hanh" /> },
          { path: "booking/:id", Component: OperationsBookingDetail },
          { path: "booking-moi", Component: StaffCreateBookingPage },
          { path: "khach-hang", Component: CustomersPage },
          { path: "thanh-toan", Component: PaymentClaimsPage },
        ],
      },
    ],
  },
  {
    element: <RouteGuard roles={["Admin"]} />,
    children: [
      {
        path: "admin",
        element: <DashboardLayout admin />,
        children: [
          { index: true, element: <OperationsDashboard admin /> },
          { path: "booking", element: <OperationsBookings prefix="/admin" /> },
          { path: "booking/:id", Component: OperationsBookingDetail },
          { path: "booking-moi", Component: StaffCreateBookingPage },
          { path: "phong", element: <AdminResourcesPage group="rooms" /> },
          { path: "danh-muc", element: <AdminResourcesPage group="catalog" /> },
          { path: "gia", element: <AdminResourcesPage group="pricing" /> },
          { path: "khach-hang", Component: CustomersPage },
          { path: "nhan-vien", element: <AdminResourcesPage group="staff" /> },
          { path: "thanh-toan", Component: PaymentClaimsPage },
          { path: "nhat-ky", Component: AuditLogsPage },
        ],
      },
    ],
  },
];

export const router = createBrowserRouter(routes, { basename: import.meta.env.BASE_URL });
