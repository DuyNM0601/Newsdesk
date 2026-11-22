import { useEffect, useMemo } from "react";
import { format } from "date-fns";
import { vi } from "date-fns/locale";
import { DevWatch, LeadStory, QuickBrief, SectionBlock } from "@/features/news/components/front-page";
import { ErrorState, ListSkeleton } from "@/features/news/components/states";
import { useDigest } from "@/features/digest/hooks/use-digest";
import type { DigestItem } from "@/features/digest/types";
import { useArticles } from "@/features/news/hooks/use-articles";
import { sections, type Section } from "@/features/news/lib/sections";
import { NewsTopic } from "@/features/news/types";

// Các mục hiện dưới trang nhất. "Nghề dev" đã có dải riêng ở trên nên không lặp lại.
const frontSections = sections.filter((s) => s.slug !== "nghe-dev");

function SectionLoader({ section }: { section: Section }) {
  const result = useArticles({ category: section.category, topic: section.topic, pageSize: 4 });
  if (result.status === "loading") return <ListSkeleton rows={4} />;
  if (result.status === "error") return <ErrorState message={result.message} onRetry={result.retry} />;
  if (result.data.items.length === 0) return null;
  return <SectionBlock section={section} articles={result.data.items} />;
}

export function HomePage() {
  const top = useArticles({ sort: "top", pageSize: 6 });
  const dev = useArticles({ topic: NewsTopic.DevImpact, sort: "top", pageSize: 8 });
  const today = useMemo(() => {
    const s = format(new Date(), "EEEE, d 'tháng' M, yyyy", { locale: vi });
    return s.charAt(0).toUpperCase() + s.slice(1);
  }, []);

  useEffect(() => {
    document.title = "Newsdesk – Tin AI, lập trình và nghề dev, quan trọng nhất lên đầu";
  }, []);

  // Bản tin hôm nay (AI tóm tắt): bài nào có tóm tắt thì trang nhất dùng ý chính của AI thay cho câu đầu của feed.
  const digest = useDigest();
  const aiById = useMemo(() => {
    const map: Record<string, DigestItem> = {};
    if (digest.status === "ready") for (const item of digest.digest.items) if (item.tldr) map[item.articleId] = item;
    return map;
  }, [digest]);
  const digestTotal = digest.status === "ready" ? digest.digest.total : 0;

  const shown = top.status === "ready" ? new Set(top.data.items.map((a) => a.id)) : new Set<string>();
  const devItems = dev.status === "ready" ? dev.data.items.filter((a) => !shown.has(a.id)).slice(0, 2) : [];

  return (
    <div className="mx-auto w-full max-w-6xl px-4 pb-16 sm:px-6">
      <div className="flex flex-wrap items-baseline justify-between gap-x-6 gap-y-1 border-b border-border py-3 text-sm text-muted-foreground">
        <span>{today}</span>
        <span>Tin từ các nguồn quốc tế chính thống, cập nhật mỗi 15 phút</span>
      </div>

      <h1 className="sr-only">Trang nhất Newsdesk</h1>

      {top.status === "error" ? (
        <div className="py-8">
          <ErrorState message={top.message} onRetry={top.retry} />
        </div>
      ) : (
        <section aria-label="Tin quan trọng nhất" className="grid gap-12 py-10 lg:grid-cols-[minmax(0,1fr)_24rem] lg:gap-x-12 lg:gap-y-14">
          {top.status === "loading" ? (
            <>
              <ListSkeleton rows={3} />
              <ListSkeleton rows={5} />
            </>
          ) : top.data.items.length === 0 ? (
            <p className="text-muted-foreground lg:col-span-2">Chưa có tin trong 3 ngày gần đây. Hãy quay lại sau ít phút.</p>
          ) : (
            <>
              <div className="lg:col-start-1 lg:row-start-1">
                <LeadStory article={top.data.items[0]} ai={aiById[top.data.items[0].id]} />
              </div>
              {/* Điểm nhanh cao hơn bài đầu, nên chiếm cả hai hàng bên phải; "Ảnh hưởng nghề dev" lấp chỗ trống dưới bài đầu. */}
              <div className="lg:col-start-2 lg:row-span-2 lg:row-start-1">
                <QuickBrief articles={top.data.items.slice(1)} aiById={aiById} digestTotal={digestTotal} />
              </div>
              {devItems.length > 0 && (
                <div className="lg:col-start-1 lg:row-start-2">
                  <DevWatch articles={devItems} />
                </div>
              )}
            </>
          )}
        </section>
      )}

      <div className="grid gap-x-12 gap-y-14 border-t border-border pt-14 md:grid-cols-2">
        {frontSections.map((s) => (
          <SectionLoader key={s.slug} section={s} />
        ))}
      </div>
    </div>
  );
}
