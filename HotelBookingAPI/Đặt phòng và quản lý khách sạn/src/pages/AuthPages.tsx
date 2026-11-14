import { ArrowRight, LockKeyhole, ShieldCheck } from "lucide-react";
import { FormEvent, useState } from "react";
import { Link, useNavigate } from "react-router";
import { roleHome, useAuth } from "../auth/AuthProvider";
import { Brand } from "../components/Brand";
import { Button, Field, useToast } from "../components/ui";
import { api } from "../lib/api";

const AUTH_IMAGE =
  "https://images.unsplash.com/photo-1565503187147-6b0012bab4da?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=85&w=1400";

function AuthShell({
  children,
  title,
  description,
}: {
  children: React.ReactNode;
  title: string;
  description: string;
}) {
  return (
    <div className="grid min-h-screen bg-white lg:grid-cols-2">
      <div className="flex min-h-screen flex-col p-5 sm:p-10 lg:p-14">
        <Brand />
        <div className="mx-auto flex w-full max-w-md flex-1 flex-col justify-center py-10">
          <p className="eyebrow">Chào mừng đến Aurora</p>
          <h1 className="font-display text-4xl font-semibold text-navy">{title}</h1>
          <p className="mt-3 text-sm leading-6 text-slate-500">{description}</p>
          {children}
        </div>
        <p className="text-xs text-slate-400">
          Thông tin của bạn được bảo vệ và chỉ dùng để phục vụ kỳ lưu trú.
        </p>
      </div>
      <div className="relative hidden overflow-hidden lg:block">
        <img
          src={AUTH_IMAGE}
          alt="Bãi biển bên khu nghỉ dưỡng"
          className="absolute inset-0 size-full object-cover"
        />
        <div className="absolute inset-0 bg-gradient-to-t from-navy/90 via-navy/10 to-transparent" />
        <div className="absolute bottom-14 left-14 right-14 text-white">
          <ShieldCheck className="mb-5 text-gold" size={32} />
          <blockquote className="font-display text-3xl leading-snug">
            “Mọi hành trình tuyệt vời đều bắt đầu bằng một nơi chốn khiến ta mong được trở về.”
          </blockquote>
        </div>
      </div>
    </div>
  );
}

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const notify = useToast();
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    setBusy(true);
    setError("");
    try {
      const user = await login(String(data.get("username")), String(data.get("password")));
      notify(`Xin chào ${user.fullName || user.username}`);
      navigate(roleHome(user.role), { replace: true });
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Đăng nhập không thành công");
    } finally {
      setBusy(false);
    }
  };
  return (
    <AuthShell
      title="Đăng nhập"
      description="Tiếp tục quản lý kỳ nghỉ và những trải nghiệm đã chọn."
    >
      <form onSubmit={submit} className="mt-8 grid gap-5">
        <Field
          name="username"
          label="Tên đăng nhập"
          autoComplete="username"
          required
          placeholder="Nhập tên đăng nhập"
        />
        <Field
          name="password"
          label="Mật khẩu"
          type="password"
          autoComplete="current-password"
          required
          placeholder="Nhập mật khẩu"
        />
        {error && (
          <p role="alert" className="rounded-xl bg-red-50 p-3 text-sm text-red-700">
            {error}
          </p>
        )}
        <Button disabled={busy} className="w-full">
          {busy ? "Đang đăng nhập…" : "Đăng nhập"}
          <ArrowRight size={17} />
        </Button>
      </form>
      <p className="mt-6 text-center text-sm text-slate-500">
        Chưa có tài khoản?{" "}
        <Link className="font-semibold text-gold-dark hover:underline" to="/dang-ky">
          Đăng ký ngay
        </Link>
      </p>
    </AuthShell>
  );
}

export function RegisterPage() {
  const navigate = useNavigate();
  const notify = useToast();
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    setError("");
    const password = String(form.get("password"));
    if (password !== form.get("confirmPassword"))
      return setError("Mật khẩu xác nhận chưa khớp.");
    const payload = {
      username: String(form.get("username") ?? "").trim(),
      password,
      fullName: String(form.get("fullName") ?? "").trim(),
      email: String(form.get("email") ?? "").trim(),
      phone: String(form.get("phone") ?? "").trim(),
    };
    if (payload.username.length < 3)
      return setError("Tên đăng nhập phải có ít nhất 3 ký tự sau khi bỏ khoảng trắng.");
    if (!payload.fullName) return setError("Họ và tên không được để trống.");
    setBusy(true);
    try {
      await api("/api/Auth/register", { method: "POST", body: JSON.stringify(payload) });
      notify("Đăng ký thành công. Vui lòng đăng nhập để tiếp tục.");
      navigate("/dang-nhap");
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Đăng ký không thành công");
    } finally {
      setBusy(false);
    }
  };
  return (
    <AuthShell
      title="Tạo tài khoản"
      description="Chỉ vài bước để bắt đầu hành trình nghỉ dưỡng của riêng bạn."
    >
      <form onSubmit={submit} className="mt-8 grid gap-4 sm:grid-cols-2">
        <Field
          name="fullName"
          label="Họ và tên"
          autoComplete="name"
          required
          maxLength={100}
          placeholder="Nguyễn Minh Anh"
          className="sm:col-span-2"
        />
        <Field
          name="username"
          label="Tên đăng nhập"
          autoComplete="username"
          required
          minLength={3}
          maxLength={50}
        />
        <Field
          name="phone"
          label="Số điện thoại"
          type="tel"
          autoComplete="tel"
          required
          pattern="(0|\+84)[0-9]{9}"
          title="Nhập số điện thoại dạng 0900000001 hoặc +84900000001."
          placeholder="0900000001"
        />
        <Field
          name="email"
          label="Email"
          type="email"
          autoComplete="email"
          required
          maxLength={100}
          className="sm:col-span-2"
        />
        <Field
          name="password"
          label="Mật khẩu"
          type="password"
          autoComplete="new-password"
          required
          minLength={6}
          maxLength={100}
        />
        <Field
          name="confirmPassword"
          label="Xác nhận mật khẩu"
          type="password"
          autoComplete="new-password"
          required
          minLength={6}
          maxLength={100}
        />
        {error && (
          <p role="alert" className="rounded-xl bg-red-50 p-3 text-sm text-red-700 sm:col-span-2">
            {error}
          </p>
        )}
        <Button disabled={busy} className="w-full sm:col-span-2">
          {busy ? "Đang tạo tài khoản…" : "Tạo tài khoản"}
          <ArrowRight size={17} />
        </Button>
      </form>
      <p className="mt-6 text-center text-sm text-slate-500">
        Đã có tài khoản?{" "}
        <Link className="font-semibold text-gold-dark hover:underline" to="/dang-nhap">
          Đăng nhập
        </Link>
      </p>
    </AuthShell>
  );
}

export function ChangePasswordForm() {
  const notify = useToast();
  const [busy, setBusy] = useState(false);
  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    if (form.get("newPassword") !== form.get("confirm"))
      return notify("Mật khẩu xác nhận chưa khớp.", "error");
    setBusy(true);
    try {
      await api("/api/Auth/change-password", {
        method: "POST",
        body: JSON.stringify({
          currentPassword: form.get("currentPassword"),
          newPassword: form.get("newPassword"),
        }),
      });
      notify("Đã đổi mật khẩu thành công.");
      event.currentTarget.reset();
    } catch (reason) {
      notify(reason instanceof Error ? reason.message : "Không thể đổi mật khẩu", "error");
    } finally {
      setBusy(false);
    }
  };
  return (
    <form onSubmit={submit} className="grid max-w-xl gap-4">
      <Field label="Mật khẩu hiện tại" name="currentPassword" type="password" required />
      <Field
        label="Mật khẩu mới"
        name="newPassword"
        type="password"
        minLength={8}
        pattern="(?=.*[A-Za-z])(?=.*\d).+"
        title="Mật khẩu mới phải có ít nhất 8 ký tự, gồm cả chữ và số."
        required
      />
      <Field label="Xác nhận mật khẩu mới" name="confirm" type="password" required />
      <Button disabled={busy} className="w-fit">
        <LockKeyhole size={17} />
        Cập nhật mật khẩu
      </Button>
    </form>
  );
}
