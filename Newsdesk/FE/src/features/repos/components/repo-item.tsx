import { ArrowUpRightIcon, CodeIcon, ScaleIcon, StarIcon } from "lucide-react";
import { AiLabel, KeyPointList } from "@/features/digest/components/ai-bits";
import type { Repo } from "../types";

const compact = new Intl.NumberFormat("en", { notation: "compact", maximumFractionDigits: 1 });

/**
 * Một repo: tên (link GitHub), vài số cần thiết (sao, ngôn ngữ, giấy phép), "repo này làm gì" do AI tóm tắt và "dành cho ai".
 * Chưa tóm tắt thì vẫn hiện mô tả của chính repo, ghi rõ là đang chờ.
 */
export function RepoItem({ repo, index }: { repo: Repo; index: number }) {
  return (
    <article className="row-in border-t border-border py-6" style={{ "--i": Math.min(index, 7) } as React.CSSProperties}>
      <div className="flex flex-wrap items-center gap-x-3 gap-y-1.5 text-sm">
        {repo.kind === "Ai" && <span className="rounded-full bg-tone-ai-bg px-2.5 py-0.5 text-xs font-semibold text-tone-ai">AI</span>}
        <span className="inline-flex items-center gap-1 font-semibold">
          <StarIcon className="size-4 text-primary" aria-hidden="true" />
          {compact.format(repo.stars)}
          <span className="sr-only"> sao</span>
        </span>
        {repo.language && (
          <span className="inline-flex items-center gap-1 text-muted-foreground">
            <CodeIcon className="size-4" aria-hidden="true" />
            {repo.language}
          </span>
        )}
        {repo.license && (
          <span className="inline-flex items-center gap-1 text-muted-foreground">
            <ScaleIcon className="size-4" aria-hidden="true" />
            {repo.license}
          </span>
        )}
      </div>

      <h2 className="mt-2 text-2xl font-semibold leading-snug">
        <a href={repo.url} target="_blank" rel="noopener noreferrer" className="headline underline-offset-4 hover:underline focus-visible:underline">
          {repo.fullName}
          <span className="sr-only"> (mở trên GitHub)</span>
        </a>
      </h2>

      {repo.tldr ? (
        <div className="mt-3 space-y-3">
          <p className="max-w-[62ch] text-lg leading-relaxed text-foreground/90">{repo.tldr}</p>
          <KeyPointList points={repo.keyPoints} className="max-w-[62ch] text-base text-foreground/85" />
          {repo.whyItMatters && (
            <p className="max-w-[62ch] rounded-md bg-accent px-4 py-2.5 text-sm font-semibold leading-snug text-accent-foreground">
              Dành cho ai: {repo.whyItMatters}
            </p>
          )}
          <div>
            <AiLabel language={repo.summaryLanguage} />
          </div>
        </div>
      ) : (
        <p className="mt-3 max-w-[62ch] text-base text-foreground/85">
          {repo.description ?? "Repo chưa có mô tả."}
          <span className="mt-1 block text-sm text-muted-foreground">AI đang tóm tắt repo này.</span>
        </p>
      )}

      <a
        href={repo.url}
        target="_blank"
        rel="noopener noreferrer"
        className="mt-2 inline-flex min-h-11 items-center gap-1 text-sm font-semibold text-primary underline-offset-4 hover:underline"
      >
        Mở trên GitHub
        <ArrowUpRightIcon className="size-4" aria-hidden="true" />
      </a>
    </article>
  );
}
