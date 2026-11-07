import { Navigate, Outlet, useLocation } from "react-router";
import { roleHome, useAuth } from "./AuthProvider";
import { LoadingState } from "../components/ui";
import type { Role } from "../types/api";

export function RouteGuard({ roles }: { roles: Role[] }) {
  const { user, loading } = useAuth();
  const location = useLocation();
  if (loading) return <LoadingState label="Đang kiểm tra phiên đăng nhập…" />;
  if (!user) return <Navigate to="/dang-nhap" state={{ from: location.pathname }} replace />;
  if (!roles.includes(user.role)) return <Navigate to={roleHome(user.role)} replace />;
  return <Outlet />;
}
