import { Link } from "react-router-dom";
import { ArrowRightIcon, ArrowUpRightIcon } from "lucide-react";
import { cn } from "@/lib/utils";
import { firstSentence, reasonLine } from "../lib/format";
import type { Section } from "../lib/sections";
import { IMPORTANT_THRESHOLD, NewsCategory, NewsTopic, VERY_IMPORTANT_THRESHOLD, type Article, type NewsTopicValue } from "../types";
import { ArticleLink, Meta, PriorityMark, Thumb, TopicChips } from "./article-bits";

/**
 * Bài quan trọng nhất: điểm nhìn đầu tiên của cả trang. Thứ tự đọc cố định: dấu quan trọng → TIÊU ĐỀ lớn nhất trang →
 * câu đầu → "vì sao đáng đọc" (khung nền màu nhấn) → nút đọc bài gốc → ảnh. Ảnh để cuối vì nó ngẫu nhiên theo feed,
 * không được chiếm chỗ của tiêu đề.
 */
export function LeadStory({ article }: { article: Article }) {
  const lede = firstSentence(article.summary, 260);
  const reason = reasonLine(article);
  return (
    <article className="ink-rise flex flex-col gap-5" style={{ "--i": 0 } as React.CSSProperties}>
      <div className="flex flex-wrap items-center gap-x-4 gap-y-2">
        <PriorityMark priority={article.priority} />
        <TopicChips topics={article.topics} />
      </div>
      <h2 className="text-balance text-[2.5rem] font-bold leading-[1.03] sm:text-6xl">
        <ArticleLink article={article} />
      </h2>
      {lede && <p className="max-w-[60ch] text-xl leading-relaxed text-foreground/90">{lede}</p>}
      {reason && (
        <p className="max-w-[60ch] rounded-md bg-accent px-4 py-3 text-base font-semibold leading-snug text-accent-foreground">
          Vì sao đáng đọc: {reason}
        </p>
      )}
      <div className="flex flex-wrap items-center gap-x-5 gap-y-3">
        <a
          href={article.url}
          target="_blank"
          rel="noopener noreferrer"
          className="inline-flex min-h-12 items-center gap-2 rounded-md bg-primary px-5 text-base font-semibold text-primary-foreground transition-[opacity,transform] duration-150 ease-(--ease-out) hover:opacity-90 active:scale-[0.97]"
        >
          Đọc bài gốc
          <ArrowUpRightIcon className="size-5" aria-hidden="true" />
          <span className="sr-only"> của {article.sourceName}</span>
        </a>
        <Meta article={article} />
      </div>
      <Thumb article={article} className="mt-1 aspect-[2/1] w-full" />
    </article>
  );
}

/**
 * "Điểm nhanh": các tin đáng đọc tiếp theo, xếp theo mức quan trọng giảm dần. Là điểm nhìn thứ hai: hộp nền riêng, số thứ hạng
 * lớn màu nhấn (số mang thông tin, tiếp nối bài đầu trang), tin đầu to hơn. Ghi rõ xếp hạng bằng từ khoá, không phải AI.
 */
export function QuickBrief({ articles, startRank = 2 }: { articles: Article[]; startRank?: number }) {
  return (
    <section aria-labelledby="quick-brief" className="ink-rise border-t-4 border-rule bg-muted p-5 lg:p-6" style={{ "--i": 2 } as React.CSSProperties}>
      <h2 id="quick-brief" className="text-2xl font-bold">
        Điểm nhanh
      </h2>
      <p className="mt-1 text-sm text-muted-foreground">Tiếp theo bài đầu trang, quan trọng nhất ở trên.</p>
      <ol className="mt-4 divide-y divide-foreground/15">
        {articles.map((a, i) => {
          const lede = firstSentence(a.summary, 130);
          const first = i === 0;
          return (
            <li key={a.id} className="flex gap-4 py-4">
              <span aria-hidden="true" className="headline w-9 shrink-0 text-4xl font-bold leading-none text-primary">
                {startRank + i}
              </span>
              <div className="min-w-0 flex-1">
                <h3 className={cn("leading-snug", first ? "text-xl font-bold" : "text-lg font-semibold")}>
                  <ArticleLink article={a} />
                </h3>
                {lede && <p className="mt-1 line-clamp-2 text-sm leading-relaxed text-foreground/80">{lede}</p>}
                <div className="mt-2 flex flex-wrap items-center gap-x-3 gap-y-1">
                  <PriorityMark priority={a.priority} />
                  <Meta article={a} />
                </div>
              </div>
            </li>
          );
        })}
      </ol>
      <p className="mt-2 text-xs leading-relaxed text-muted-foreground">
        Xếp hạng bằng từ khoá, ưu tiên AI, lập trình và tin ảnh hưởng nghề dev. Chưa phải AI đánh giá.
      </p>
    </section>
  );
}

/** Dải "Ảnh hưởng nghề dev": điểm nhìn thứ ba, nền màu riêng, mỗi bài một dòng "vì sao đáng đọc". */
export function DevWatch({ articles }: { articles: Article[] }) {
  return (
    <section aria-labelledby="dev-watch" className="ink-rise rounded-md bg-tone-impact-bg px-5 py-6 sm:px-8" style={{ "--i": 3 } as React.CSSProperties}>
      <div className="flex flex-wrap items-baseline justify-between gap-x-6 gap-y-1">
        <h2 id="dev-watch" className="text-3xl font-bold text-tone-impact">
          Ảnh hưởng nghề dev
        </h2>
        <Link to="/muc/nghe-dev" className="inline-flex min-h-11 items-center gap-1 text-sm font-semibold underline underline-offset-4">
          Xem tất cả
          <ArrowRightIcon className="size-4" aria-hidden="true" />
        </Link>
      </div>
      <p className="mt-1 max-w-[65ch] text-sm text-foreground/80">
        Tin có thể tác động tới việc làm, công cụ, giấy phép và bảo mật của người làm phần mềm.
      </p>
      <ul className="mt-5 grid grid-cols-[repeat(auto-fit,minmax(13rem,1fr))] gap-x-8 gap-y-5">
        {articles.map((a) => (
          <li key={a.id} className="flex flex-col gap-2 border-t border-foreground/25 pt-3">
            <h3 className="text-xl font-bold leading-snug">
              <ArticleLink article={a} />
            </h3>
            <p className="text-sm text-foreground/80">{reasonLine(a)}</p>
            <Meta article={a} className="mt-auto" />
          </li>
        ))}
      </ul>
    </section>
  );
}

/**
 * Cỡ chữ và độ đậm của tiêu đề đi theo mức quan trọng (mực đậm = đáng đọc trước): rất quan trọng to và đậm nhất,
 * quan trọng vừa, bài thường nhẹ nhất. Nhờ vậy mắt tự dừng ở bài đáng đọc ngay trong một danh sách.
 */
function titleClass(priority: number, compact: boolean): string {
  const very = priority >= VERY_IMPORTANT_THRESHOLD;
  const important = priority >= IMPORTANT_THRESHOLD;
  if (compact) return very ? "text-xl font-bold" : important ? "text-lg font-semibold" : "text-lg font-medium text-foreground/90";
  return very ? "text-[1.75rem] font-bold" : important ? "text-2xl font-semibold" : "text-xl font-medium";
}

/** Một dòng bài trong danh sách mục (trang chủ và trang mục). */
export function ArticleRow({
  article,
  compact = false,
  hideTopics = [],
  index,
}: {
  article: Article;
  compact?: boolean;
  /** Chủ đề đã là tên của mục đang xem thì không lặp lại thành nhãn trên từng bài. */
  hideTopics?: NewsTopicValue[];
  /** Vị trí trong danh sách: có thì dòng "vào" lần lượt (trễ 35ms mỗi dòng, tối đa 7 dòng đầu); không có thì hiện sẵn. */
  index?: number;
}) {
  const lede = compact ? "" : firstSentence(article.summary, 200);
  return (
    <article
      className={cn("flex gap-4 border-t border-border", compact ? "py-3.5" : "py-5", index !== undefined && "row-in")}
      style={index !== undefined ? ({ "--i": Math.min(index, 7) } as React.CSSProperties) : undefined}
    >
      <div className="min-w-0 flex-1">
        <div className="flex flex-wrap items-center gap-x-3 gap-y-1">
          <PriorityMark priority={article.priority} />
          <TopicChips topics={article.topics.filter((t) => !hideTopics.includes(t))} />
        </div>
        <h3 className={cn("mt-1.5 leading-snug", titleClass(article.priority, compact))}>
          <ArticleLink article={article} />
        </h3>
        {lede && <p className="mt-1.5 line-clamp-2 max-w-[65ch] text-base leading-relaxed text-foreground/80">{lede}</p>}
        <Meta article={article} className="mt-1.5" />
      </div>
      {!compact && <Thumb article={article} className="hidden aspect-[4/3] w-36 shrink-0 self-start sm:block" />}
    </article>
  );
}

/**
 * Khối một mục ở trang chủ. Tên mục là một khối mực đặc đặt trên vạch (như tab của trang báo), lặp lại cho mọi mục để mắt
 * có điểm neo khi lướt xuống; bài trong khối nhẹ hơn nên không tranh với trang nhất.
 */
export function SectionBlock({ section, articles }: { section: Section; articles: Article[] }) {
  const hidden: NewsTopicValue[] = section.topic ? [section.topic] : section.category === NewsCategory.Programming ? [NewsTopic.Dev] : [];
  return (
    <section aria-labelledby={`sec-${section.slug}`} className="min-w-0">
      <div className="flex items-end justify-between gap-4 border-b-2 border-rule">
        <h2 id={`sec-${section.slug}`} className="bg-foreground px-3.5 py-2 text-xl font-bold text-background">
          {section.label}
        </h2>
        <Link to={`/muc/${section.slug}`} className="inline-flex min-h-11 items-center gap-1 text-sm font-semibold text-primary underline-offset-4 hover:underline">
          Xem tất cả
          <ArrowRightIcon className="size-4" aria-hidden="true" />
        </Link>
      </div>
      <div className="[&>article:first-child]:border-t-0">
        {articles.map((a, i) => (
          <ArticleRow key={a.id} article={a} compact hideTopics={hidden} index={i} />
        ))}
      </div>
    </section>
  );
}
