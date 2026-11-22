// Phản chiếu RepoModel ở BE.
export type Repo = {
  fullName: string;
  /** Link tới repo trên GitHub. */
  url: string;
  description: string | null;
  language: string | null;
  /** Mã giấy phép SPDX; null nếu repo không khai báo. */
  license: string | null;
  /** Số sao lúc phát hiện (không cập nhật sau đó). */
  stars: number;
  kind: "Ai" | "Other";
  /** yyyy-MM-dd, giờ Việt Nam. */
  discoveredOn: string;
  /** Repo này làm gì; null nếu AI chưa tóm tắt. */
  tldr: string | null;
  keyPoints: string[];
  whyItMatters: string | null;
  summaryLanguage: string | null;
};
