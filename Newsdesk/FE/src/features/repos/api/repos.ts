import { request } from "@/lib/api-client";
import type { Repo } from "../types";

/** Repo GitHub đang lên, ngày giới thiệu mới nhất trước. */
export function listRepos(limit = 24, signal?: AbortSignal): Promise<Repo[]> {
  return request<Repo[]>(`/repos?limit=${limit}`, { signal });
}
