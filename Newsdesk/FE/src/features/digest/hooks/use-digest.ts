import { useCallback, useEffect, useState } from "react";
import { ApiError } from "@/lib/api-client";
import { getDigest, listDigestDates } from "../api/digest";
import type { Digest } from "../types";

export type DigestState = { status: "loading" } | { status: "ready"; digest: Digest } | { status: "error"; message: string };

function messageOf(e: unknown): string {
  return e instanceof ApiError ? e.message : "Không kết nối được máy chủ tin tức. Hãy thử lại sau ít phút.";
}

/** Bản tin của một ngày (hoặc mới nhất nếu không truyền ngày). Đổi ngày thì giữ bản cũ trên màn hình tới khi bản mới về. */
export function useDigest(date?: string): DigestState & { retry: () => void } {
  const [state, setState] = useState<DigestState>({ status: "loading" });
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    const ac = new AbortController();
    getDigest(date, ac.signal)
      .then((digest) => setState({ status: "ready", digest }))
      .catch((e) => {
        if (!(e instanceof DOMException && e.name === "AbortError")) setState({ status: "error", message: messageOf(e) });
      });
    return () => ac.abort();
  }, [date, attempt]);

  const retry = useCallback(() => {
    setState({ status: "loading" });
    setAttempt((n) => n + 1);
  }, []);
  return { ...state, retry };
}

/** Các ngày đã có bản tin (mới nhất trước). Lỗi thì trả mảng rỗng: chỉ là điều hướng phụ. */
export function useDigestDates(): string[] {
  const [dates, setDates] = useState<string[]>([]);
  useEffect(() => {
    const ac = new AbortController();
    listDigestDates(ac.signal)
      .then(setDates)
      .catch(() => {});
    return () => ac.abort();
  }, []);
  return dates;
}
