import { useEffect } from "react";
import { format, parseISO } from "date-fns";
import { vi } from "date-fns/locale";
import { EmptyState, ErrorState, ListSkeleton } from "@/features/news/components/states";
import { RepoItem } from "@/features/repos/components/repo-item";
import { useRepos } from "@/features/repos/hooks/use-repos";

/** Repo GitHub mới tạo gần đây mà đang được nhiều sao (AI trước), mỗi repo có tóm tắt "làm gì" và link sang GitHub. */
export function GitHubPage() {
  const state = useRepos(24);

  useEffect(() => {
    document.title = "GitHub đang lên – Newsdesk";
  }, []);

  return (
    <div className="mx-auto w-full max-w-4xl px-4 pb-16 sm:px-6">
      <header className="pt-8">
        <h1 className="headline text-4xl font-semibold sm:text-5xl">GitHub đang lên</h1>
        <p className="mt-2 max-w-[65ch] text-base text-muted-foreground">
          Mỗi sáng hệ thống chọn khoảng 12 repo mới tạo trong 14 ngày gần đây mà đã nhiều sao, repo AI xếp trước. AI tóm tắt repo làm gì và dành cho ai;
          muốn xem chi tiết thì mở thẳng trên GitHub. Số sao là lúc repo được phát hiện.
        </p>
      </header>

      <div className="mt-4" aria-live="polite">
        {state.status === "loading" && <ListSkeleton rows={4} />}
        {state.status === "error" && <ErrorState message={state.message} onRetry={state.retry} />}
        {state.status === "ready" && state.repos.length === 0 && (
          <EmptyState title="Chưa có repo nào" hint="Danh sách đầu tiên sẽ xuất hiện sau lần chọn repo kế tiếp (mỗi sáng 5 giờ 30, giờ Việt Nam)." />
        )}
        {state.status === "ready" &&
          state.repos.map((repo, i) => {
            const newDay = i === 0 || state.repos[i - 1].discoveredOn !== repo.discoveredOn;
            return (
              <div key={repo.fullName}>
                {newDay && (
                  <p className="pb-1 pt-6 text-sm font-semibold text-muted-foreground">
                    Giới thiệu ngày {format(parseISO(repo.discoveredOn), "d 'tháng' M, yyyy", { locale: vi })}
                  </p>
                )}
                <RepoItem repo={repo} index={i} />
              </div>
            );
          })}
      </div>
    </div>
  );
}
