import { useCallback, useEffect, useState } from "react";

export function useApiData<T>(loader: () => Promise<T>, dependencies: unknown[] = []) {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      setData(await loader());
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Đã xảy ra lỗi không xác định");
    } finally {
      setLoading(false);
    }
    // Callers pass primitive dependencies for their request.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, dependencies);

  useEffect(() => {
    load();
  }, [load]);

  return { data, loading, error, reload: load, setData };
}
