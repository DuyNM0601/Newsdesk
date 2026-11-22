import { request } from "@/lib/api-client";
import type { Digest } from "../types";

/** Bản tin của một ngày (yyyy-MM-dd); bỏ trống thì lấy bản tin mới nhất. */
export function getDigest(date?: string, signal?: AbortSignal): Promise<Digest> {
  const qs = date ? `?date=${encodeURIComponent(date)}` : "";
  return request<Digest>(`/digest${qs}`, { signal });
}

/** Các ngày đã có bản tin, mới nhất trước. */
export function listDigestDates(signal?: AbortSignal): Promise<string[]> {
  return request<string[]>("/digest/dates", { signal });
}
