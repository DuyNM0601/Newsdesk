import { useState } from "react";
import { ArrowUpRightIcon } from "lucide-react";
import { cn } from "@/lib/utils";
import { timeAgo } from "../lib/format";
import { topicLabel, topicOrder } from "../lib/sections";
import { NewsTopic, VERY_IMPORTANT_THRESHOLD, IMPORTANT_THRESHOLD, type Article, type NewsTopicValue } from "../types";

const toneClass: Record<NewsTopicValue, string> = {
  [NewsTopic.Ai]: "bg-tone-ai-bg text-tone-ai",
  [NewsTopic.Dev]: "bg-tone-dev-bg text-tone-dev",
  [NewsTopic.DevImpact]: "bg-tone-impact-bg text-tone-impact",
};

/** Nhãn chủ đề khớp bài. Luôn là chữ, màu chỉ để tô thêm. */
export function TopicChips({ topics, className }: { topics: NewsTopicValue[]; className?: string }) {
  const shown = topicOrder.filter((t) => topics.includes(t));
  if (shown.length === 0) return null;
  return (
    <ul className={cn("flex flex-wrap gap-1.5", className)}>
      {shown.map((t) => (
        <li key={t} className={cn("rounded-full px-2.5 py-0.5 text-xs font-semibold", toneClass[t])}>
          {topicLabel[t]}
        </li>
      ))}
    </ul>
  );
}

/** Dấu mức quan trọng kiểu bản in: 2 hoặc 3 chấm đặc, kèm chữ. Bài thường không có dấu để mắt chỉ dừng ở bài đáng đọc. */
export function PriorityMark({ priority, className }: { priority: number; className?: string }) {
  if (priority < IMPORTANT_THRESHOLD) return null;
  const very = priority >= VERY_IMPORTANT_THRESHOLD;
  const label = very ? "Rất quan trọng" : "Quan trọng";
  return (
    <span className={cn("inline-flex items-center gap-1.5 text-xs font-semibold text-foreground", className)}>
      <svg width="30" height="8" viewBox="0 0 30 8" aria-hidden="true" className="text-primary">
        {[0, 1, 2].map((i) => (
          <circle
            key={i}
            cx={4 + i * 11}
            cy="4"
            r="3.2"
            fill={i < (very ? 3 : 2) ? "currentColor" : "none"}
            stroke="currentColor"
            strokeWidth="1.2"
          />
        ))}
      </svg>
      {label}
    </span>
  );
}

export function Meta({ article, className }: { article: Article; className?: string }) {
  return (
    <p className={cn("text-sm text-muted-foreground", className)}>
      <span className="font-medium text-foreground/80">{article.sourceName}</span>
      <span aria-hidden="true"> · </span>
      <time dateTime={article.publishedAt}>{timeAgo(article.publishedAt)}</time>
    </p>
  );
}

/** Tiêu đề bài, mở bài gốc ở tab mới. Gạch chân khi di chuột / focus. */
export function ArticleLink({ article, className }: { article: Article; className?: string }) {
  return (
    <a
      href={article.url}
      target="_blank"
      rel="noopener noreferrer"
      className={cn("headline decoration-1 underline-offset-4 hover:underline focus-visible:underline", className)}
    >
      {article.title}
      <span className="sr-only"> (mở bài gốc ở {article.sourceName})</span>
    </a>
  );
}

/** Ảnh bài (nếu feed có). Ảnh lỗi thì biến mất hẳn, không để khung trống. */
export function Thumb({ article, className }: { article: Article; className?: string }) {
  const [failed, setFailed] = useState(false);
  if (!article.imageUrl || failed) return null;
  return (
    <img
      src={article.imageUrl}
      alt=""
      loading="lazy"
      decoding="async"
      referrerPolicy="no-referrer"
      onError={() => setFailed(true)}
      className={cn("rounded-sm bg-muted object-cover", className)}
    />
  );
}

export function OriginalLink({ article, className }: { article: Article; className?: string }) {
  return (
    <a
      href={article.url}
      target="_blank"
      rel="noopener noreferrer"
      className={cn("inline-flex min-h-11 items-center gap-1 text-sm font-semibold text-primary hover:underline underline-offset-4", className)}
    >
      Đọc bài gốc
      <ArrowUpRightIcon className="size-4" aria-hidden="true" />
      <span className="sr-only"> của {article.sourceName}</span>
    </a>
  );
}
