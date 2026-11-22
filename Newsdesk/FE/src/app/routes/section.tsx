import { useEffect, useState, type FormEvent } from "react";
import { Navigate, useParams, useSearchParams } from "react-router-dom";
import { SearchIcon, XIcon } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { ArticleRow } from "@/features/news/components/front-page";
import { EmptyState, ErrorState, ListSkeleton } from "@/features/news/components/states";
import { useArticleFeed } from "@/features/news/hooks/use-articles";
import { findSection, SECTION_PAGE_SIZE } from "@/features/news/lib/sections";
import { NewsCategory, type NewsTopicValue } from "@/features/news/types";

function Segmented<T extends string>({
  label,
  value,
  options,
  onChange,
}: {
  label: string;
  value: T;
  options: { value: T; label: string }[];
  onChange: (v: T) => void;
}) {
  return (
    <div role="group" aria-label={label} className="inline-flex rounded-md border border-input p-0.5">
      {options.map((o) => (
        <button
          key={o.value}
          type="button"
          aria-pressed={value === o.value}
          onClick={() => onChange(o.value)}
          className={cn(
            "min-h-11 rounded-[5px] px-4 text-sm font-semibold transition-[background-color,color,transform] duration-150 ease-(--ease-out) active:scale-[0.97]",
            value === o.value ? "bg-foreground text-background" : "text-foreground hover:bg-muted",
          )}
        >
          {o.label}
        </button>
      ))}
    </div>
  );
}

export function SectionPage() {
  const { slug } = useParams();
  const section = findSection(slug);
  const [params, setParams] = useSearchParams();
  const q = params.get("q") ?? "";
  const sort = params.get("sort") === "top" ? "top" : "new";
  const important = params.get("important") === "1";
  const [draft, setDraft] = useState(q);

  useEffect(() => setDraft(q), [q]);
  useEffect(() => {
    document.title = `${section?.label ?? "Newsdesk"}${q ? ` · "${q}"` : ""} – Newsdesk`;
  }, [section, q]);

  const feed = useArticleFeed({ category: section?.category, topic: section?.topic, q, sort, important, pageSize: SECTION_PAGE_SIZE });

  if (!section) return <Navigate to="/" replace />;

  const hideTopics: NewsTopicValue[] = section.topic ? [section.topic] : section.category === NewsCategory.Programming ? ["Dev"] : [];

  function update(next: Record<string, string | null>) {
    const p = new URLSearchParams(params);
    for (const [k, v] of Object.entries(next)) {
      if (v === null || v === "") p.delete(k);
      else p.set(k, v);
    }
    setParams(p, { replace: true });
  }

  function submit(e: FormEvent) {
    e.preventDefault();
    update({ q: draft.trim() });
  }

  const filtered = q !== "" || important || sort === "top";

  return (
    <div className="mx-auto w-full max-w-4xl px-4 pb-16 sm:px-6">
      {/* key theo mục: tiêu đề mới "vào" nhẹ mỗi lần đổi tab thay vì đổi chữ tức thì. */}
      <header key={section.slug} className="row-in pt-8">
        <h1 className="headline text-4xl font-semibold sm:text-5xl">{section.label}</h1>
        <p className="mt-2 max-w-[65ch] text-base text-muted-foreground">{section.blurb}</p>
      </header>

      <form onSubmit={submit} role="search" className="mt-6 flex flex-wrap items-end gap-x-4 gap-y-3 border-y border-border py-4">
        <div className="min-w-60 flex-1">
          <label htmlFor="q" className="mb-1.5 block text-sm font-semibold">
            Tìm trong tiêu đề
          </label>
          <div className="relative">
            <SearchIcon className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" aria-hidden="true" />
            <Input
              id="q"
              type="search"
              value={draft}
              onChange={(e) => setDraft(e.target.value)}
              placeholder="Ví dụ: Kubernetes"
              className="h-11 border-input pl-9 pr-10 text-base"
            />
            {draft && (
              <button
                type="button"
                aria-label="Xoá từ khoá"
                onClick={() => {
                  setDraft("");
                  update({ q: null });
                }}
                className="absolute right-1 top-1/2 flex size-9 -translate-y-1/2 items-center justify-center rounded text-muted-foreground hover:text-foreground"
              >
                <XIcon className="size-4" aria-hidden="true" />
              </button>
            )}
          </div>
        </div>
        <Button type="submit" className="h-11 px-6 text-sm font-semibold">
          Tìm
        </Button>
        <Segmented
          label="Sắp xếp"
          value={sort}
          options={[
            { value: "new", label: "Mới nhất" },
            { value: "top", label: "Quan trọng nhất" },
          ]}
          onChange={(v) => update({ sort: v === "top" ? "top" : null })}
        />
        <Segmented
          label="Mức quan trọng"
          value={important ? "important" : "all"}
          options={[
            { value: "all", label: "Tất cả" },
            { value: "important", label: "Chỉ tin quan trọng" },
          ]}
          onChange={(v) => update({ important: v === "important" ? "1" : null })}
        />
      </form>
      {sort === "top" && <p className="mt-2 text-sm text-muted-foreground">Quan trọng nhất trong 3 ngày gần đây, xếp bằng từ khoá (ưu tiên AI, lập trình, nghề dev).</p>}

      {/* Đang tải danh sách mới: GIỮ danh sách cũ, làm mờ nhẹ sau ~150ms (tải nhanh hơn thì không nháy). Khi bài mới về, cả danh sách
          thay mới và các dòng "vào" lần lượt (key = generation). */}
      <div
        className={cn("mt-4 transition-opacity duration-150 ease-(--ease-out)", feed.refreshing && "opacity-55 delay-150")}
        aria-live="polite"
        aria-busy={feed.status === "loading" || feed.refreshing}
      >
        {feed.status === "loading" && <ListSkeleton rows={6} />}
        {feed.status === "error" && <ErrorState message={feed.message} onRetry={feed.retry} />}
        {feed.status === "ready" && feed.items.length === 0 && (
          <EmptyState
            title="Không có bài phù hợp"
            hint={filtered ? "Thử bỏ bớt bộ lọc hoặc đổi từ khoá." : "Mục này chưa có bài. Hãy quay lại sau ít phút."}
            action={filtered ? { label: "Bỏ bộ lọc", onClick: () => setParams({}, { replace: true }) } : undefined}
          />
        )}
        {feed.status === "ready" && (
          <div key={feed.generation}>
            {feed.items.map((a, i) => (
              <ArticleRow key={a.id} article={a} hideTopics={hideTopics} index={i < SECTION_PAGE_SIZE ? i : undefined} />
            ))}
          </div>
        )}
      </div>

      {feed.status === "ready" && feed.hasMore && (
        <div className="mt-6 flex flex-col items-center gap-2 border-t border-border pt-6">
          <Button type="button" variant="outline" onClick={feed.loadMore} disabled={feed.loadingMore} className="h-12 px-8 text-base font-semibold active:scale-[0.97]">
            {feed.loadingMore ? "Đang tải..." : "Xem thêm bài"}
          </Button>
          {feed.message && <p className="text-sm text-danger">{feed.message}</p>}
        </div>
      )}
    </div>
  );
}
