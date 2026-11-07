import {
  AlertCircle,
  CheckCircle2,
  ChevronLeft,
  ChevronRight,
  Inbox,
  LoaderCircle,
  X,
} from "lucide-react";
import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ButtonHTMLAttributes,
  type InputHTMLAttributes,
  type ReactNode,
} from "react";
import { statusLabels } from "../lib/format";

export function Button({
  variant = "primary",
  className = "",
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: "primary" | "secondary" | "ghost" | "danger";
}) {
  const styles = {
    primary: "bg-navy text-white hover:bg-navy-light shadow-sm",
    secondary: "border border-gold/70 bg-white text-navy hover:bg-cream",
    ghost: "text-slate-600 hover:bg-slate-100",
    danger: "bg-red-600 text-white hover:bg-red-700",
  };
  return (
    <button
      className={`inline-flex min-h-11 items-center justify-center gap-2 rounded-xl px-4 py-2.5 text-sm font-semibold transition disabled:cursor-not-allowed disabled:opacity-50 ${styles[variant]} ${className}`}
      {...props}
    />
  );
}

export function Field({
  label,
  error,
  className = "",
  ...props
}: InputHTMLAttributes<HTMLInputElement> & { label: string; error?: string }) {
  const id = props.id || props.name;
  return (
    <label className={`grid gap-2 text-sm font-medium text-slate-700 ${className}`} htmlFor={id}>
      {label}
      <input
        id={id}
        className={`h-12 rounded-xl border bg-white px-3.5 text-slate-900 outline-none transition placeholder:text-slate-400 focus:border-gold focus:ring-4 focus:ring-gold/15 ${
          error ? "border-red-400" : "border-slate-200"
        }`}
        {...props}
      />
      {error && <span className="text-xs font-normal text-red-600">{error}</span>}
    </label>
  );
}

const badgeStyle: Record<string, string> = {
  Pending: "bg-amber-50 text-amber-800 border-amber-200",
  Confirmed: "bg-blue-50 text-blue-800 border-blue-200",
  CheckedIn: "bg-violet-50 text-violet-800 border-violet-200",
  CheckedOut: "bg-emerald-50 text-emerald-800 border-emerald-200",
  Paid: "bg-emerald-50 text-emerald-800 border-emerald-200",
  PartiallyPaid: "bg-sky-50 text-sky-800 border-sky-200",
  Rejected: "bg-red-50 text-red-700 border-red-200",
  Cancelled: "bg-slate-100 text-slate-600 border-slate-200",
  Unpaid: "bg-red-50 text-red-700 border-red-200",
};

export function Badge({ status }: { status?: string }) {
  if (!status) return null;
  return (
    <span
      className={`inline-flex rounded-full border px-2.5 py-1 text-xs font-semibold ${badgeStyle[status] || "bg-slate-50 text-slate-700 border-slate-200"}`}
    >
      {statusLabels[status] || status}
    </span>
  );
}

export function LoadingState({ label = "Đang tải dữ liệu…" }: { label?: string }) {
  return (
    <div className="flex min-h-44 flex-col items-center justify-center gap-3 text-sm text-slate-500">
      <LoaderCircle className="animate-spin text-gold" size={28} />
      {label}
    </div>
  );
}

export function ErrorState({ message, retry }: { message: string; retry?: () => void }) {
  return (
    <div className="flex min-h-44 flex-col items-center justify-center gap-3 rounded-2xl border border-red-100 bg-red-50/60 p-6 text-center">
      <AlertCircle className="text-red-500" />
      <div>
        <p className="font-semibold text-slate-800">Không thể tải dữ liệu</p>
        <p className="mt-1 text-sm text-slate-600">{message}</p>
      </div>
      {retry && (
        <Button variant="secondary" onClick={retry}>
          Thử lại
        </Button>
      )}
    </div>
  );
}

export function EmptyState({ title, description }: { title: string; description: string }) {
  return (
    <div className="flex min-h-52 flex-col items-center justify-center rounded-2xl border border-dashed border-slate-300 bg-white p-8 text-center">
      <span className="mb-4 grid size-12 place-items-center rounded-full bg-cream text-gold-dark">
        <Inbox />
      </span>
      <p className="font-semibold text-navy">{title}</p>
      <p className="mt-1 max-w-md text-sm text-slate-500">{description}</p>
    </div>
  );
}

export function Pagination({
  page,
  totalPages,
  onChange,
}: {
  page: number;
  totalPages: number;
  onChange: (page: number) => void;
}) {
  if (totalPages <= 1) return null;
  return (
    <div className="flex items-center justify-end gap-2 pt-5">
      <Button
        variant="secondary"
        aria-label="Trang trước"
        disabled={page <= 1}
        onClick={() => onChange(page - 1)}
      >
        <ChevronLeft size={16} />
      </Button>
      <span className="px-2 text-sm text-slate-600">
        Trang <b>{page}</b> / {totalPages}
      </span>
      <Button
        variant="secondary"
        aria-label="Trang sau"
        disabled={page >= totalPages}
        onClick={() => onChange(page + 1)}
      >
        <ChevronRight size={16} />
      </Button>
    </div>
  );
}

export function Modal({
  open,
  title,
  children,
  onClose,
}: {
  open: boolean;
  title: string;
  children: ReactNode;
  onClose: () => void;
}) {
  if (!open) return null;
  return (
    <div
      className="fixed inset-0 z-50 grid place-items-center bg-navy/55 p-4 backdrop-blur-sm"
      role="dialog"
      aria-modal="true"
      aria-labelledby="modal-title"
      onMouseDown={(e) => e.target === e.currentTarget && onClose()}
    >
      <div className="max-h-[90vh] w-full max-w-lg overflow-auto rounded-2xl bg-white shadow-2xl">
        <div className="flex items-center justify-between border-b border-slate-100 p-5">
          <h2 id="modal-title" className="font-display text-xl font-semibold text-navy">
            {title}
          </h2>
          <button
            onClick={onClose}
            className="rounded-lg p-2 text-slate-500 hover:bg-slate-100"
            aria-label="Đóng"
          >
            <X size={20} />
          </button>
        </div>
        <div className="p-5">{children}</div>
      </div>
    </div>
  );
}

interface ToastItem {
  id: number;
  message: string;
  type: "success" | "error";
}
const ToastContext = createContext<(message: string, type?: "success" | "error") => void>(() => {});

export function ToastProvider({ children }: { children: ReactNode }) {
  const [items, setItems] = useState<ToastItem[]>([]);
  const notify = useCallback((message: string, type: "success" | "error" = "success") => {
    const id = Date.now();
    setItems((current) => [...current, { id, message, type }]);
    window.setTimeout(() => setItems((current) => current.filter((item) => item.id !== id)), 4200);
  }, []);
  const value = useMemo(() => notify, [notify]);
  return (
    <ToastContext.Provider value={value}>
      {children}
      <div
        className="fixed bottom-5 right-5 z-[60] grid w-[min(90vw,380px)] gap-2"
        aria-live="polite"
      >
        {items.map((item) => (
          <div
            key={item.id}
            className={`flex items-start gap-3 rounded-xl border bg-white p-4 shadow-xl ${item.type === "error" ? "border-red-200" : "border-emerald-200"}`}
          >
            {item.type === "error" ? (
              <AlertCircle className="shrink-0 text-red-500" size={20} />
            ) : (
              <CheckCircle2 className="shrink-0 text-emerald-600" size={20} />
            )}
            <p className="text-sm font-medium text-slate-700">{item.message}</p>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export const useToast = () => useContext(ToastContext);
