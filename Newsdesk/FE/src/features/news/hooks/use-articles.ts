import { useCallback, useEffect, useState } from "react";
import { ApiError } from "@/lib/api-client";
import { listArticles, listSources, type ListParams } from "../api/news";
import type { Article, ArticlePage, Source } from "../types";

export type Loadable<T> = { status: "loading" } | { status: "ready"; data: T } | { status: "error"; message: string };

function messageOf(e: unknown): string {
  if (e instanceof ApiError) return e.message;
  return "Không kết nối được máy chủ tin tức. Hãy thử lại sau ít phút.";
}

/** Một trang bài (dùng cho các khối ở trang chủ). Tải lại khi tham số đổi; `retry` tải lại sau lỗi. */
export function useArticles(params: ListParams): Loadable<ArticlePage> & { retry: () => void } {
  const key = JSON.stringify(params);
  const [state, setState] = useState<Loadable<ArticlePage>>({ status: "loading" });
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    let stale = false;
    setState({ status: "loading" });
    listArticles(JSON.parse(key) as ListParams)
      .then((data) => !stale && setState({ status: "ready", data }))
      .catch((e) => !stale && setState({ status: "error", message: messageOf(e) }));
    return () => {
      stale = true;
    };
  }, [key, attempt]);

  const retry = useCallback(() => setAttempt((n) => n + 1), []);
  return { ...state, retry };
}

type Feed = {
  items: Article[];
  page: number;
  hasMore: boolean;
  status: "loading" | "ready" | "error";
  /** Đang tải danh sách mới trong khi vẫn hiện danh sách cũ (đổi tab / bộ lọc): không xoá nội dung, chỉ làm mờ nhẹ. */
  refreshing: boolean;
  loadingMore: boolean;
  message: string;
  /** Tăng mỗi lần danh sách thay mới; dùng làm `key` để các dòng "vào" lần lượt lại. */
  generation: number;
};

const initialFeed: Feed = { items: [], page: 0, hasMore: false, status: "loading", refreshing: false, loadingMore: false, message: "", generation: 0 };

/**
 * Danh sách nhiều trang (nút "Xem thêm"). Đổi tham số thì GIỮ danh sách cũ trên màn hình cho tới khi bài mới về
 * (stale-while-revalidate), thay vì xoá rồi hiện khung chờ; chỉ lần tải đầu tiên mới có khung chờ.
 */
export function useArticleFeed(params: Omit<ListParams, "page">) {
  const key = JSON.stringify(params);
  const [feed, setFeed] = useState<Feed>(initialFeed);
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    let stale = false;
    setFeed((f) => (f.status === "ready" ? { ...f, refreshing: true } : { ...f, status: "loading", message: "" }));
    listArticles({ ...(JSON.parse(key) as ListParams), page: 1 })
      .then((p) => {
        if (stale) return;
        setFeed((f) => ({
          items: p.items,
          page: 1,
          hasMore: p.hasMore,
          status: "ready",
          refreshing: false,
          loadingMore: false,
          message: "",
          generation: f.generation + 1,
        }));
      })
      .catch((e) => {
        if (!stale) setFeed((f) => ({ ...f, status: "error", refreshing: false, message: messageOf(e) }));
      });
    return () => {
      stale = true;
    };
  }, [key, attempt]);

  const loadMore = useCallback(() => {
    if (feed.loadingMore || !feed.hasMore) return;
    setFeed((f) => ({ ...f, loadingMore: true }));
    listArticles({ ...(JSON.parse(key) as ListParams), page: feed.page + 1 })
      .then((p) => setFeed((f) => ({ ...f, items: [...f.items, ...p.items], page: p.page, hasMore: p.hasMore, loadingMore: false })))
      .catch((e) => setFeed((f) => ({ ...f, loadingMore: false, message: messageOf(e) })));
  }, [feed.loadingMore, feed.hasMore, feed.page, key]);

  const retry = useCallback(() => setAttempt((n) => n + 1), []);
  return { ...feed, loadMore, retry };
}

/** Tên các nguồn đang theo dõi (chân trang). Lỗi thì im lặng: chỉ là thông tin phụ. */
export function useSourceNames(): string[] {
  const [names, setNames] = useState<string[]>([]);
  useEffect(() => {
    const ac = new AbortController();
    listSources(ac.signal)
      .then((sources: Source[]) => setNames([...new Set(sources.map((s) => s.name))]))
      .catch(() => {});
    return () => ac.abort();
  }, []);
  return names;
}
