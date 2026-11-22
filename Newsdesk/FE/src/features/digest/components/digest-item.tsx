import { cn } from "@/lib/utils";
import { ArticleLink, Meta, OriginalLink, PriorityMark, TopicChips } from "@/features/news/components/article-bits";
import { IMPORTANT_THRESHOLD, VERY_IMPORTANT_THRESHOLD, type Article } from "@/features/news/types";
import type { DigestItem } from "../types";
import { AiLabel, KeyPointList } from "./ai-bits";

/** Dùng lại các thành phần bài viết của feature news (chúng nhận kiểu Article). */
export function toArticle(item: DigestItem): Article {
  return {
    id: item.articleId,
    title: item.title,
    url: item.url,
    summary: null,
    imageUrl: item.imageUrl,
    category: item.category,
    sourceName: item.sourceName,
    publishedAt: item.publishedAt,
    priority: item.priority,
    topics: item.topics,
  };
}

/**
 * Một tin trong bản tin: số thứ hạng lớn màu nhấn, tiêu đề (cỡ theo mức quan trọng), ý chính, các gạch đầu dòng, "vì sao đáng đọc"
 * và link về bài gốc. Bài chưa được tóm tắt vẫn hiện đủ tiêu đề + link, chỉ ghi rõ là đang chờ.
 */
export function DigestItemView({ item }: { item: DigestItem }) {
  const article = toArticle(item);
  const very = item.priority >= VERY_IMPORTANT_THRESHOLD;
  const important = item.priority >= IMPORTANT_THRESHOLD;
  return (
    <article
      className="row-in flex gap-4 border-t border-border py-6 sm:gap-6"
      style={{ "--i": Math.min(item.rank - 1, 7) } as React.CSSProperties}
    >
      <span aria-hidden="true" className="headline w-9 shrink-0 text-3xl font-bold leading-none text-primary sm:w-12 sm:text-4xl">
        {item.rank}
      </span>
      <div className="min-w-0 flex-1 space-y-3">
        <div className="flex flex-wrap items-center gap-x-3 gap-y-1">
          <PriorityMark priority={item.priority} />
          <TopicChips topics={item.topics} />
        </div>
        <h2 className={cn("leading-snug", very ? "text-[1.75rem] font-bold" : important ? "text-2xl font-semibold" : "text-xl font-medium")}>
          <ArticleLink article={article} />
        </h2>

        {item.tldr ? (
          <>
            <p className="max-w-[62ch] text-lg leading-relaxed text-foreground/90">{item.tldr}</p>
            <KeyPointList points={item.keyPoints} className="max-w-[62ch] text-base text-foreground/85" />
            {item.whyItMatters && (
              <p className="max-w-[62ch] rounded-md bg-accent px-4 py-2.5 text-sm font-semibold leading-snug text-accent-foreground">
                Vì sao đáng đọc: {item.whyItMatters}
              </p>
            )}
            <div><AiLabel language={item.language} /></div>
          </>
        ) : (
          <p className="text-sm text-muted-foreground">AI đang tóm tắt tin này. Bạn có thể đọc bài gốc ngay.</p>
        )}

        <div className="flex flex-wrap items-center justify-between gap-x-6">
          <Meta article={article} />
          <OriginalLink article={article} />
        </div>
      </div>
    </article>
  );
}
