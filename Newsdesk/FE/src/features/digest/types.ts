import type { NewsCategoryValue, NewsTopicValue } from "@/features/news/types";

// Phản chiếu DigestItemModel / DigestModel ở BE.
export type DigestItem = {
  rank: number;
  articleId: string;
  title: string;
  /** Link về bài gốc trên trang của nguồn. */
  url: string;
  category: NewsCategoryValue;
  sourceName: string;
  publishedAt: string;
  imageUrl: string | null;
  priority: number;
  topics: NewsTopicValue[];
  /** Ý chính; null nếu bài chưa được AI tóm tắt. */
  tldr: string | null;
  keyPoints: string[];
  whyItMatters: string | null;
  /** Ngôn ngữ bản tóm tắt ("en" | "vi"); null nếu chưa có. */
  language: string | null;
};

export type Digest = {
  /** yyyy-MM-dd theo giờ Việt Nam; null nếu chưa có bản tin nào. */
  date: string | null;
  total: number;
  summarized: number;
  items: DigestItem[];
};
