import { Edit3, FileSearch, Plus, Search, Trash2 } from "lucide-react";
import { FormEvent, useState } from "react";
import {
  Badge,
  Button,
  EmptyState,
  ErrorState,
  Field,
  LoadingState,
  Modal,
  Pagination,
  useToast,
} from "../components/ui";
import { useApiData } from "../hooks/useApiData";
import { api, normalizePaged, queryString } from "../lib/api";
import { formatDate, formatVND } from "../lib/format";

interface ResourceConfig {
  label: string;
  endpoint: string;
  createEndpoint?: string;
  deleteEndpoint?: string;
  supportsUpdate?: boolean;
  columns?: Array<{ key: string; label: string }>;
  fields: Array<{ key: string; label: string; type?: string; required?: boolean }>;
}

const resources: Record<string, ResourceConfig[]> = {
  rooms: [
    {
      label: "Phòng",
      endpoint: "/api/Rooms",
      fields: [
        { key: "roomNumber", label: "Số phòng", required: true },
        { key: "floor", label: "Tầng", type: "number", required: true },
        { key: "roomTypeId", label: "Mã loại phòng", type: "number", required: true },
        { key: "status", label: "Trạng thái", required: true },
        { key: "description", label: "Mô tả" },
        { key: "imageUrl", label: "Đường dẫn ảnh" },
        { key: "amenityIds", label: "Mã tiện nghi (cách nhau bằng dấu phẩy)" },
      ],
    },
    {
      label: "Loại phòng",
      endpoint: "/api/RoomTypes",
      fields: [
        { key: "name", label: "Tên loại phòng", required: true },
        { key: "capacity", label: "Sức chứa", type: "number", required: true },
        { key: "pricePerNight", label: "Giá mỗi đêm", type: "number", required: true },
        { key: "description", label: "Mô tả" },
      ],
    },
  ],
  catalog: [
    {
      label: "Tiện nghi",
      endpoint: "/api/Amenities",
      fields: [
        { key: "name", label: "Tên tiện nghi", required: true },
      ],
    },
    {
      label: "Dịch vụ bổ sung",
      endpoint: "/api/ExtraServices",
      fields: [
        { key: "name", label: "Tên dịch vụ", required: true },
        { key: "price", label: "Đơn giá", type: "number", required: true },
        { key: "unit", label: "Đơn vị tính", required: true },
        { key: "isActive", label: "Đang hoạt động (true/false)", required: true },
      ],
    },
  ],
  pricing: [
    {
      label: "Mã giảm giá",
      endpoint: "/api/Coupons",
      fields: [
        { key: "code", label: "Mã coupon", required: true },
        { key: "discountType", label: "Loại (Percent/Fixed)", required: true },
        { key: "value", label: "Giá trị", type: "number", required: true },
        { key: "maxDiscount", label: "Giảm tối đa", type: "number" },
        { key: "minBookingAmount", label: "Đơn tối thiểu", type: "number" },
        { key: "validFrom", label: "Từ ngày", type: "date", required: true },
        { key: "validTo", label: "Đến ngày", type: "date", required: true },
        { key: "maxUses", label: "Lượt sử dụng", type: "number" },
        { key: "isActive", label: "Đang hoạt động (true/false)", required: true },
      ],
    },
    {
      label: "Quy tắc giá",
      endpoint: "/api/PricingRules",
      fields: [
        { key: "name", label: "Tên quy tắc", required: true },
        { key: "ruleType", label: "Loại (Weekend/Season)", required: true },
        { key: "startDate", label: "Từ ngày", type: "date" },
        { key: "endDate", label: "Đến ngày", type: "date" },
        { key: "roomTypeId", label: "Mã loại phòng", type: "number" },
        { key: "percent", label: "Điều chỉnh (%)", type: "number", required: true },
        { key: "priority", label: "Ưu tiên", type: "number" },
        { key: "isActive", label: "Đang hoạt động (true/false)", required: true },
      ],
    },
  ],
  staff: [
    {
      label: "Tài khoản nhân viên",
      endpoint: "/api/Users",
      createEndpoint: "/api/Users/staff",
      deleteEndpoint: "/api/Users/staff",
      supportsUpdate: false,
      columns: [
        { key: "username", label: "Tên đăng nhập" },
        { key: "role", label: "Vai trò" },
      ],
      fields: [
        { key: "username", label: "Tên đăng nhập", required: true },
        { key: "password", label: "Mật khẩu", type: "password", required: true },
      ],
    },
  ],
};

function cellValue(value: unknown, key: string) {
  if (value === null || value === undefined || value === "") return "—";
  if (typeof value === "boolean") return <Badge status={value ? "Active" : "Inactive"} />;
  if (typeof value === "object")
    return String(
      (value as Record<string, unknown>).name ||
        (value as Record<string, unknown>).roomNumber ||
        "—",
    );
  if (/price|amount|value|discount/i.test(key) && typeof value === "number")
    return formatVND(value);
  if (/date|At$/.test(key) && typeof value === "string") return formatDate(value);
  return String(value);
}

export function AdminResourcesPage({
  group,
}: {
  group: "rooms" | "catalog" | "pricing" | "staff";
}) {
  const configs = resources[group];
  const [index, setIndex] = useState(0);
  const config = configs[index];
  const [page, setPage] = useState(1);
  const [editing, setEditing] = useState<Record<string, unknown> | null | undefined>(undefined);
  const notify = useToast();
  const result = useApiData(
    () =>
      api<unknown[]>(`${config.endpoint}${queryString({ pageNumber: page, pageSize: 15 })}`).then(
        (v) => normalizePaged<Record<string, unknown>>(v),
      ),
    [config.endpoint, page],
  );
  const columns = config.columns ? config.columns : config.fields.slice(0, 5);
  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const payload: Record<string, unknown> = {};
    const form = new FormData(event.currentTarget);
    config.fields.forEach((field) => {
      const raw = form.get(field.key);
      if (raw === "") return;
      if (field.key === "amenityIds") {
        payload[field.key] = String(raw)
          .split(",")
          .map((id) => Number(id.trim()))
          .filter((id) => Number.isInteger(id) && id > 0);
      } else if (field.type === "number") {
        payload[field.key] = Number(raw);
      } else if (field.key === "isActive") {
        payload[field.key] = String(raw).toLowerCase() === "true";
      } else {
        payload[field.key] = raw;
      }
    });
    try {
      await api(
        editing?.id ? `${config.endpoint}/${editing.id}` : (config.createEndpoint ?? config.endpoint),
        {
          method: editing?.id ? "PUT" : "POST",
          body: JSON.stringify(payload),
        },
      );
      notify(`${editing?.id ? "Đã cập nhật" : "Đã tạo"} ${config.label.toLowerCase()}.`);
      setEditing(undefined);
      result.reload();
    } catch (reason) {
      notify(reason instanceof Error ? reason.message : "Không thể lưu dữ liệu", "error");
    }
  };
  const remove = async (item: Record<string, unknown>) => {
    if (!window.confirm(`Bạn chắc chắn muốn xóa ${config.label.toLowerCase()} này?`)) return;
    try {
      await api(`${config.deleteEndpoint ?? config.endpoint}/${item.id}`, { method: "DELETE" });
      notify("Đã xóa dữ liệu.");
      result.reload();
    } catch (reason) {
      notify(reason instanceof Error ? reason.message : "Không thể xóa", "error");
    }
  };
  return (
    <div>
      <p className="eyebrow">Danh mục hệ thống</p>
      <div className="flex flex-wrap items-end justify-between gap-4">
        <h2 className="section-title">Quản lý {config.label.toLowerCase()}</h2>
        <Button onClick={() => setEditing(null)}>
          <Plus size={17} />
          Thêm {config.label.toLowerCase()}
        </Button>
      </div>
      {configs.length > 1 && (
        <div className="mt-6 flex gap-2 border-b border-slate-200">
          {configs.map((item, i) => (
            <button
              key={item.endpoint}
              onClick={() => {
                setIndex(i);
                setPage(1);
              }}
              className={`border-b-2 px-4 py-3 text-sm font-semibold ${i === index ? "border-gold text-navy" : "border-transparent text-slate-500"}`}
            >
              {item.label}
            </button>
          ))}
        </div>
      )}
      <section className="mt-5 overflow-hidden rounded-2xl border border-slate-200 bg-white">
        {result.loading ? (
          <LoadingState />
        ) : result.error ? (
          <ErrorState message={result.error} retry={result.reload} />
        ) : !result.data?.items.length ? (
          <EmptyState
            title={`Chưa có ${config.label.toLowerCase()}`}
            description="Dữ liệu mới sẽ hiển thị tại đây sau khi được tạo."
          />
        ) : (
          <>
            <div className="overflow-x-auto">
              <table>
                <thead>
                  <tr>
                    {columns.map((field) => (
                      <th key={field.key}>{field.label}</th>
                    ))}
                    <th>Trạng thái</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {result.data.items.map((item, row) => (
                    <tr key={String(item.id || row)}>
                      {columns.map((field) => (
                        <td key={field.key}>{cellValue(item[field.key], field.key)}</td>
                      ))}
                      <td>
                        <Badge
                          status={String(
                            item.status ?? (item.isActive === false ? "Inactive" : "Active"),
                          )}
                        />
                      </td>
                      <td>
                        <div className="flex justify-end gap-1">
                          {config.supportsUpdate !== false && (
                            <Button
                              variant="ghost"
                              title="Chỉnh sửa"
                              onClick={() => setEditing(item)}
                            >
                              <Edit3 size={16} />
                            </Button>
                          )}
                          <Button
                            variant="ghost"
                            title="Xóa"
                            className="text-red-600"
                            onClick={() => remove(item)}
                          >
                            <Trash2 size={16} />
                          </Button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="px-5 pb-5">
              <Pagination
                page={result.data.pageNumber}
                totalPages={result.data.totalPages}
                onChange={setPage}
              />
            </div>
          </>
        )}
      </section>
      <Modal
        open={editing !== undefined}
        onClose={() => setEditing(undefined)}
        title={`${editing?.id ? "Chỉnh sửa" : "Thêm"} ${config.label.toLowerCase()}`}
      >
        <form onSubmit={submit} className="grid gap-4">
          {config.fields.map((field) => (
            <Field
              key={field.key}
              name={field.key}
              label={field.label}
              type={field.type || "text"}
              required={field.required}
              defaultValue={
                field.key === "amenityIds" && Array.isArray(editing?.amenityIds)
                  ? editing.amenityIds.join(", ")
                  : String(
                      editing?.[field.key] ??
                        (field.key === "status"
                          ? "Available"
                          : field.key === "isActive"
                            ? "true"
                            : ""),
                    )
              }
              readOnly={field.key === "role" || (field.key === "code" && !!editing?.id)}
            />
          ))}
          <div className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="ghost" onClick={() => setEditing(undefined)}>
              Hủy
            </Button>
            <Button>Lưu thay đổi</Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}

export function AuditLogsPage() {
  const [page, setPage] = useState(1);
  const [filters, setFilters] = useState<Record<string, string>>({});
  const query = queryString({ pageNumber: page, pageSize: 20, ...filters });
  const result = useApiData(
    () =>
      api<unknown[]>(`/api/AuditLogs${query}`).then((v) =>
        normalizePaged<Record<string, unknown>>(v),
      ),
    [query],
  );
  const submit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    setFilters(
      Object.fromEntries(Array.from(form.entries()).map(([key, value]) => [key, String(value)])),
    );
    setPage(1);
  };
  return (
    <div>
      <p className="eyebrow">Theo dõi thay đổi</p>
      <h2 className="section-title">Nhật ký hoạt động</h2>
      <form
        onSubmit={submit}
        className="mt-7 grid gap-3 rounded-2xl border border-slate-200 bg-white p-4 md:grid-cols-3 xl:grid-cols-6"
      >
        <Field label="Loại entity" name="entityType" />
        <Field label="Mã entity" name="entityId" type="number" />
        <Field label="Username" name="userName" />
        <Field label="Hành động" name="action" />
        <Field label="Từ ngày" name="from" type="date" />
        <Field label="Đến ngày" name="to" type="date" />
        <Button className="self-end">
          <Search size={17} />
          Lọc
        </Button>
      </form>
      <section className="mt-5 overflow-hidden rounded-2xl border border-slate-200 bg-white">
        {result.loading ? (
          <LoadingState />
        ) : result.error ? (
          <ErrorState message={result.error} retry={result.reload} />
        ) : !result.data?.items.length ? (
          <EmptyState
            title="Không có nhật ký phù hợp"
            description="Thay đổi hệ thống sẽ được ghi nhận tại đây."
          />
        ) : (
          <>
            <div className="overflow-x-auto">
              <table>
                <thead>
                  <tr>
                    <th>Thời gian</th>
                    <th>Người thực hiện</th>
                    <th>Hành động</th>
                    <th>Đối tượng</th>
                    <th>Mã</th>
                    <th>Chi tiết</th>
                  </tr>
                </thead>
                <tbody>
                  {result.data.items.map((log, i) => (
                    <tr key={String(log.id || i)}>
                      <td>{formatDate(String(log.createdAt || log.timestamp || ""))}</td>
                      <td className="font-medium">{String(log.username || "Hệ thống")}</td>
                      <td>
                        <Badge status={String(log.action || "—")} />
                      </td>
                      <td>{String(log.entityType || "—")}</td>
                      <td>{String(log.entityId || "—")}</td>
                      <td>
                        <Button variant="ghost">
                          <FileSearch size={16} />
                          Xem
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="px-5 pb-5">
              <Pagination
                page={result.data.pageNumber}
                totalPages={result.data.totalPages}
                onChange={setPage}
              />
            </div>
          </>
        )}
      </section>
    </div>
  );
}
