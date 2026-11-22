import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";

/** Khung chờ cho một danh sách tiêu đề, giữ chỗ để trang không giật khi dữ liệu về. */
export function ListSkeleton({ rows = 4, className }: { rows?: number; className?: string }) {
  return (
    <div className={className} role="status" aria-label="Đang tải tin">
      {Array.from({ length: rows }, (_, i) => (
        <div key={i} className="space-y-2 border-t border-border py-4 first:border-t-0">
          <Skeleton className="h-5 w-11/12" />
          <Skeleton className="h-5 w-2/3" />
          <Skeleton className="h-4 w-1/3" />
        </div>
      ))}
    </div>
  );
}

export function ErrorState({ message, onRetry }: { message: string; onRetry: () => void }) {
  return (
    <div role="alert" className="rounded-md border border-danger/40 bg-danger-bg p-5">
      <p className="font-semibold">Không tải được tin</p>
      <p className="mt-1 text-sm text-foreground/80">{message}</p>
      <Button type="button" variant="outline" onClick={onRetry} className="mt-3 h-11">
        Thử lại
      </Button>
    </div>
  );
}

export function EmptyState({ title, hint, action }: { title: string; hint: string; action?: { label: string; onClick: () => void } }) {
  return (
    <div className="rounded-md border border-dashed border-border p-8 text-center">
      <p className="text-lg font-semibold">{title}</p>
      <p className="mx-auto mt-1 max-w-[50ch] text-sm text-muted-foreground">{hint}</p>
      {action && (
        <Button type="button" variant="outline" onClick={action.onClick} className="mt-4 h-11">
          {action.label}
        </Button>
      )}
    </div>
  );
}
