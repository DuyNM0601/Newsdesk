import { useEffect } from "react";
import { Link, useParams } from "react-router-dom";
import { ArrowLeftIcon, ArrowRightIcon } from "lucide-react";
import { format, parseISO } from "date-fns";
import { vi } from "date-fns/locale";
import { DigestItemView } from "@/features/digest/components/digest-item";
import { useDigest, useDigestDates } from "@/features/digest/hooks/use-digest";
import { EmptyState, ErrorState, ListSkeleton } from "@/features/news/components/states";

function longDate(iso: string): string {
  const s = format(parseISO(iso), "EEEE, d 'tháng' M, yyyy", { locale: vi });
  return s.charAt(0).toUpperCase() + s.slice(1);
}

const stepLink =
  "inline-flex min-h-11 items-center gap-1.5 rounded-md border border-input px-3 text-sm font-semibold transition-colors duration-150 ease-(--ease-out) hover:bg-muted active:scale-[0.97]";

/** Bản tin của một ngày: khoảng 50 tin mới và quan trọng nhất, mỗi tin có ý chính do AI tóm tắt kèm link về bài gốc. */
export function DigestPage() {
  const { date } = useParams();
  const dates = useDigestDates();
  const state = useDigest(date);

  useEffect(() => {
    document.title = "Bản tin hằng ngày – Newsdesk";
  }, []);

  const current = state.status === "ready" ? state.digest.date : null;
  const index = current ? dates.indexOf(current) : -1;
  const older = index >= 0 ? dates[index + 1] : undefined; // dates sắp mới nhất trước
  const newer = index > 0 ? dates[index - 1] : undefined;

  return (
    <div className="mx-auto w-full max-w-4xl px-4 pb-16 sm:px-6">
      <header className="pt-8">
        <h1 className="headline text-4xl font-semibold sm:text-5xl">Bản tin hằng ngày</h1>
        {current && <p className="mt-2 text-lg font-medium">{longDate(current)}</p>}
        <p className="mt-2 max-w-[65ch] text-base text-muted-foreground">
          Mỗi sáng 6 giờ (giờ Việt Nam), hệ thống chọn khoảng 50 tin mới và quan trọng nhất của 24 giờ qua. AI tóm tắt ý chính để bạn nắm nhanh;
          muốn biết chi tiết thì bấm sang bài gốc.
        </p>
      </header>

      {(older || newer) && (
        <nav aria-label="Chọn ngày" className="mt-5 flex flex-wrap items-center gap-3 border-y border-border py-3">
          {older ? (
            <Link to={`/ban-tin/${older}`} className={stepLink}>
              <ArrowLeftIcon className="size-4" aria-hidden="true" />
              Ngày trước
            </Link>
          ) : null}
          {newer ? (
            <Link to={`/ban-tin/${newer}`} className={stepLink}>
              Ngày sau
              <ArrowRightIcon className="size-4" aria-hidden="true" />
            </Link>
          ) : null}
        </nav>
      )}

      <div className="mt-4" aria-live="polite">
        {state.status === "loading" && <ListSkeleton rows={5} />}
        {state.status === "error" && <ErrorState message={state.message} onRetry={state.retry} />}
        {state.status === "ready" && state.digest.items.length === 0 && (
          <EmptyState
            title="Chưa có bản tin"
            hint={date ? "Ngày này chưa có bản tin." : "Bản tin đầu tiên sẽ xuất hiện sau khi hệ thống thu thập đủ tin. Hãy quay lại sau ít phút."}
          />
        )}
        {state.status === "ready" && state.digest.items.length > 0 && (
          <>
            <p className="pb-3 text-sm text-muted-foreground">
              {state.digest.summarized}/{state.digest.total} tin đã có tóm tắt
              {state.digest.summarized < state.digest.total ? ". Phần còn lại đang được AI tóm tắt, quay lại sau ít phút." : "."}
            </p>
            <div key={state.digest.date}>
              {state.digest.items.map((item) => (
                <DigestItemView key={item.articleId} item={item} />
              ))}
            </div>
          </>
        )}
      </div>
    </div>
  );
}
