// Phản chiếu hằng số chuỗi của BE (NewsCategory, NewsTopic) và ArticleModel / ArticlePageModel.
export const NewsCategory = {
  Tech: "Tech",
  Programming: "Programming",
  Politics: "Politics",
  Economy: "Economy",
} as const;
export type NewsCategoryValue = (typeof NewsCategory)[keyof typeof NewsCategory];

export const NewsTopic = {
  Ai: "Ai",
  Dev: "Dev",
  DevImpact: "DevImpact",
} as const;
export type NewsTopicValue = (typeof NewsTopic)[keyof typeof NewsTopic];

export type Article = {
  id: string;
  title: string;
  /** Link về bài gốc. */
  url: string;
  summary: string | null;
  imageUrl: string | null;
  category: NewsCategoryValue;
  sourceName: string;
  publishedAt: string;
  /** Điểm quan trọng 0-100, tính bằng luật từ khoá ở BE (chưa phải AI). */
  priority: number;
  topics: NewsTopicValue[];
};

export type ArticlePage = {
  items: Article[];
  page: number;
  pageSize: number;
  hasMore: boolean;
};

export type Source = {
  id: string;
  name: string;
  category: NewsCategoryValue;
  language: string;
};

/** Khớp NewsRules.ImportantThreshold ở BE. */
export const IMPORTANT_THRESHOLD = 50;
/** Từ mức này là "rất quan trọng". */
export const VERY_IMPORTANT_THRESHOLD = 75;
