import { request } from "@/lib/api-client";
import type { ArticlePage, NewsCategoryValue, NewsTopicValue, Source } from "../types";

export type ListParams = {
  category?: NewsCategoryValue;
  topic?: NewsTopicValue;
  q?: string;
  important?: boolean;
  /** "new" = mới nhất (mặc định); "top" = quan trọng nhất trong 72 giờ gần đây. */
  sort?: "new" | "top";
  page?: number;
  pageSize?: number;
};

function buildQuery(params: ListParams): string {
  const qs = new URLSearchParams();
  if (params.category) qs.set("category", params.category);
  if (params.topic) qs.set("topic", params.topic);
  if (params.q?.trim()) qs.set("q", params.q.trim());
  if (params.important) qs.set("important", "true");
  if (params.sort) qs.set("sort", params.sort);
  if (params.page) qs.set("page", String(params.page));
  if (params.pageSize) qs.set("pageSize", String(params.pageSize));
  return qs.toString();
}

// Bộ nhớ đệm ngắn trong trình duyệt: cùng một truy vấn trong 60 giây dùng lại kết quả (và lời hứa đang chạy), nên rê chuột
// vào tab rồi bấm gần như hiện bài ngay. Lỗi không được nhớ. Tin cũ hơn 60 giây sẽ tải lại.
const TTL_MS = 60_000;
const MAX_ENTRIES = 60;
const cache = new Map<string, { at: number; promise: Promise<ArticlePage> }>();

export function listArticles(params: ListParams): Promise<ArticlePage> {
  const qs = buildQuery(params);
  const hit = cache.get(qs);
  if (hit && Date.now() - hit.at < TTL_MS) return hit.promise;

  const promise = request<ArticlePage>(`/news?${qs}`);
  cache.set(qs, { at: Date.now(), promise });
  promise.catch(() => cache.delete(qs));
  if (cache.size > MAX_ENTRIES) cache.delete(cache.keys().next().value!);
  return promise;
}

/** Tải sẵn một truy vấn (rê chuột / chạm vào tab) để lúc bấm có dữ liệu ngay. Lỗi bỏ qua. */
export function prefetchArticles(params: ListParams): void {
  listArticles(params).catch(() => {});
}

export function listSources(signal?: AbortSignal): Promise<Source[]> {
  return request<Source[]>("/sources", { signal });
}
