import { useSourceNames } from "@/features/news/hooks/use-articles";

export function SiteFooter() {
  const sources = useSourceNames();
  return (
    <footer className="mt-auto border-t border-border bg-muted">
      <div className="mx-auto w-full max-w-6xl space-y-3 px-4 py-8 text-sm text-muted-foreground sm:px-6">
        <p className="max-w-[70ch] text-foreground/80">
          Newsdesk chỉ hiển thị tiêu đề và đoạn trích do chính các nguồn công bố. Mọi bài đều dẫn về bài gốc.
        </p>
        {sources.length > 0 && (
          <p className="max-w-[70ch]">
            <span className="font-semibold text-foreground/80">Nguồn: </span>
            {sources.join(" · ")}
          </p>
        )}
        <p className="max-w-[70ch]">
          Mức quan trọng hiện được tính bằng từ khoá, ưu tiên AI, lập trình và tin ảnh hưởng nghề dev. Bản tóm tắt bằng AI sẽ có nhãn riêng và có thể sai.
        </p>
      </div>
    </footer>
  );
}
