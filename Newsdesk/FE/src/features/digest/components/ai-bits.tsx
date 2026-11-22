import { SparklesIcon } from "lucide-react";
import { cn } from "@/lib/utils";

/** Nhãn bắt buộc đi kèm mọi nội dung do AI viết. Có ngôn ngữ để người đọc biết bản tóm tắt bằng tiếng gì. */
export function AiLabel({ language, className }: { language: string | null; className?: string }) {
  return (
    <span className={cn("inline-flex items-center gap-1.5 text-xs font-semibold text-muted-foreground", className)}>
      <SparklesIcon className="size-3.5 text-primary" aria-hidden="true" />
      Tóm tắt bởi AI{language === "en" ? " (tiếng Anh)" : ""}, có thể sai
    </span>
  );
}

/** Các ý chính dạng gạch đầu dòng. Hiển thị dạng chữ thường (không bao giờ HTML) vì đây là chữ do model sinh ra từ nội dung bên ngoài. */
export function KeyPointList({ points, className }: { points: string[]; className?: string }) {
  if (points.length === 0) return null;
  return (
    <ul className={cn("list-disc space-y-1.5 pl-5 marker:text-primary", className)}>
      {points.map((p, i) => (
        <li key={i} className="leading-relaxed">
          {p}
        </li>
      ))}
    </ul>
  );
}
