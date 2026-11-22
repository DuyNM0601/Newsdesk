import { useCallback, useEffect, useState } from "react";
import { ApiError } from "@/lib/api-client";
import { listRepos } from "../api/repos";
import type { Repo } from "../types";

export type ReposState = { status: "loading" } | { status: "ready"; repos: Repo[] } | { status: "error"; message: string };

export function useRepos(limit = 24): ReposState & { retry: () => void } {
  const [state, setState] = useState<ReposState>({ status: "loading" });
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    const ac = new AbortController();
    listRepos(limit, ac.signal)
      .then((repos) => setState({ status: "ready", repos }))
      .catch((e) => {
        if (!(e instanceof DOMException && e.name === "AbortError")) {
          setState({ status: "error", message: e instanceof ApiError ? e.message : "Không kết nối được máy chủ tin tức. Hãy thử lại sau ít phút." });
        }
      });
    return () => ac.abort();
  }, [limit, attempt]);

  const retry = useCallback(() => {
    setState({ status: "loading" });
    setAttempt((n) => n + 1);
  }, []);
  return { ...state, retry };
}
