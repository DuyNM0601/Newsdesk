import { formatDistanceToNowStrict } from "date-fns";
import { vi } from "date-fns/locale";
import type { Article } from "../types";

/** "2 giờ trước". Ngày không hợp lệ thì trả chuỗi rỗng để không in "Invalid Date". */
export function timeAgo(iso: string): string {
  const date = new Date(iso);
  return Number.isNaN(date.getTime()) ? "" : formatDistanceToNowStrict(date, { addSuffix: true, locale: vi });
}

/** Câu đầu của đoạn trích feed, cắt ở ranh giới câu hoặc từ, tối đa `max` ký tự. Rỗng nếu feed không có đoạn trích. */
export function firstSentence(summary: string | null, max = 180): string {
  const text = (summary ?? "").replace(/\s+/g, " ").replace(/\s*(Read more|Continue reading|The post .* appeared first on .*)\.?$/i, "").trim();
  if (!text) return "";
  const end = text.search(/[.!?…](\s|$)/);
  const sentence = end > 20 && end < max ? text.slice(0, end + 1) : text;
  if (sentence.length <= max) return sentence;
  return `${sentence.slice(0, max).replace(/\s+\S*$/, "")}…`;
}

/** Một dòng "vì sao đáng đọc" cho người làm phần mềm, dựa trên chủ đề khớp (không phải AI). */
export function reasonLine(article: Article): string {
  if (article.topics.includes("DevImpact")) return "Có thể ảnh hưởng tới việc làm, công cụ hoặc bảo mật của dev";
  if (article.topics.includes("Ai")) return "Tin về AI";
  if (article.topics.includes("Dev")) return "Liên quan tới lập trình";
  return "";
}
