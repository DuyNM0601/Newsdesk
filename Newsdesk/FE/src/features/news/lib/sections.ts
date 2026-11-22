import type { ListParams } from "../api/news";
import { NewsCategory, NewsTopic, type NewsCategoryValue, type NewsTopicValue } from "../types";

export type Section = {
  slug: string;
  label: string;
  blurb: string;
  category?: NewsCategoryValue;
  topic?: NewsTopicValue;
};

// Thứ tự phản ánh ưu tiên của người đọc: AI, lập trình, tin ảnh hưởng nghề dev, rồi công nghệ chung, kinh tế, chính trị.
export const sections: Section[] = [
  { slug: "ai", label: "AI", blurb: "Mô hình, công cụ và nghiên cứu về trí tuệ nhân tạo.", topic: NewsTopic.Ai },
  { slug: "lap-trinh", label: "Lập trình", blurb: "Ngôn ngữ, framework, hạ tầng và kỹ thuật phần mềm.", category: NewsCategory.Programming },
  {
    slug: "nghe-dev",
    label: "Nghề dev",
    blurb: "Tin công nghệ có thể ảnh hưởng tới việc làm, công cụ, giấy phép và bảo mật của người làm phần mềm.",
    topic: NewsTopic.DevImpact,
  },
  { slug: "cong-nghe", label: "Công nghệ", blurb: "Phần cứng, sản phẩm và xu hướng công nghệ.", category: NewsCategory.Tech },
  { slug: "kinh-te", label: "Kinh tế", blurb: "Thị trường, lãi suất, việc làm và thương mại.", category: NewsCategory.Economy },
  { slug: "chinh-tri", label: "Chính trị", blurb: "Tin chính trị và quan hệ quốc tế.", category: NewsCategory.Politics },
];

/** Tham số mặc định của trang một mục (mới nhất, 15 bài). Trang mục và việc tải sẵn khi rê chuột dùng chung để khớp bộ nhớ đệm. */
export function sectionQuery(s: Section): ListParams {
  return { category: s.category, topic: s.topic, q: "", sort: "new", important: false, pageSize: SECTION_PAGE_SIZE };
}

export const SECTION_PAGE_SIZE = 15;

/** Trang tìm kiếm: không giới hạn mục. */
export const allSection: Section = { slug: "tat-ca", label: "Tất cả tin", blurb: "Tìm trong tiêu đề của mọi bài." };

export function findSection(slug: string | undefined): Section | undefined {
  return slug === allSection.slug ? allSection : sections.find((s) => s.slug === slug);
}

/** Nhãn hiển thị của chủ đề trên mỗi bài. DevImpact đứng đầu vì là điều người đọc muốn biết trước. */
export const topicLabel: Record<NewsTopicValue, string> = {
  DevImpact: "Ảnh hưởng nghề dev",
  Ai: "AI",
  Dev: "Lập trình",
};

export const topicOrder: NewsTopicValue[] = [NewsTopic.DevImpact, NewsTopic.Ai, NewsTopic.Dev];
