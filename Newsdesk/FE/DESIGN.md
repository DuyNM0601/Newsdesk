# Design — Newsdesk (thế giới "báo in hiện đại")

Giấy báo sáng, mực gần đen, một màu nhấn xanh. Trang nhất xếp theo **độ quan trọng**, không theo giờ: bài đáng đọc nhất to nhất, ưu tiên AI, lập trình và tin ảnh hưởng nghề dev. Nguồn gốc luôn hiện rõ (tên nguồn, giờ, link về bài gốc).

## Màu (token ở `src/index.css`, đừng viết mã hex trong component)

| Token | Sáng | Tối | Dùng cho |
|---|---|---|---|
| `background` | #FBFAF7 (giấy báo) | #111418 | nền trang |
| `foreground` / `rule` | #15171A (mực) | #ECE9E1 | chữ, vạch mực đậm dưới tên báo, đầu mỗi mục |
| `card` | #FFFFFF | #181C22 | ô nhập, hộp |
| `muted-foreground` | #4A4F57 | #A9AEB8 | chữ phụ (nguồn, giờ) |
| `border` | #E3E0D8 | #2A3038 | đường kẻ mảnh giữa các bài |
| `primary` | #1D4ED8 | #8FB0FF | liên kết, nút chính (một màu nhấn duy nhất) |
| `tone-ai` | #1D4ED8 trên #E8EEFF | #8FB0FF | nhãn "AI" |
| `tone-dev` | #0F6B4F trên #E3F4EC | #5FD1A2 | nhãn "Lập trình" |
| `tone-impact` | #9A3412 trên #FDEBDD | #F4A672 | nhãn "Ảnh hưởng nghề dev" và dải cùng tên |

Nhãn chủ đề **luôn là chữ**, màu chỉ tô thêm. Sáng là mặc định (đọc tin ban ngày ở văn phòng); tối cho buổi tối.

## Chữ

- **Literata** (chữ có chân, hỗ trợ tiếng Việt, thiết kế để đọc): tên báo và mọi tiêu đề tin (class `.headline`, `text-wrap: balance`). Tiêu đề tin là tiếng Anh theo nguồn.
- **Be Vietnam Pro**: toàn bộ giao diện tiếng Việt (nhãn, nút, chữ phụ, đoạn trích).
- Số dùng `font-variant-numeric: tabular-nums`. Đoạn trích giới hạn ~65 ký tự mỗi dòng.
- Bậc cỡ: bài đầu trang 32→48px, tiêu đề mục 24px, tiêu đề trong danh sách 18–24px, chữ phụ 14px.

## Dấu hiệu riêng

- **Mức quan trọng bằng "chấm mực"**: `PriorityMark` vẽ 2 hoặc 3 chấm đặc kèm chữ "Quan trọng" / "Rất quan trọng" (từ điểm 50 / 75). Bài thường không có dấu, để mắt chỉ dừng ở bài đáng đọc.
- **Điểm nhanh**: hộp có vạch mực dày phía trên, danh sách đánh số tiếp theo bài đầu trang (số thứ hạng mang thông tin), kèm câu đầu của đoạn trích. Ghi rõ xếp hạng bằng từ khoá, không phải AI.
- **Ảnh hưởng nghề dev**: dải nền `tone-impact` dưới bài đầu trang, mỗi bài một dòng "vì sao đáng đọc".
- Một khoảnh khắc chuyển động: khối đầu trang "in ra" lần lượt (`.ink-rise`, trượt lên 10px + hiện dần, lệch 70ms), tắt khi `prefers-reduced-motion`.

## Bố cục và component

- Khung rộng tối đa `max-w-6xl`; trang mục `max-w-4xl`. Đầu trang: tên báo, vạch mực 4px, thanh chuyên mục dính khi cuộn (cuộn ngang trong chính nó trên điện thoại). Điện thoại có thanh dưới 4 mục (Trang nhất, AI, Nghề dev, Tìm).
- Thứ tự chuyên mục phản ánh ưu tiên của người đọc: AI, Lập trình, Nghề dev, Công nghệ, Kinh tế, Chính trị.
- Không dùng thẻ bo tròn lồng nhau; bài là hàng chữ ngăn bằng đường kẻ mảnh. Nút và ô nhập cao tối thiểu 44px, vòng focus 3px.
- Component dùng chung ở `src/features/news/components/` (`article-bits`, `front-page`, `states`); nút/dialog dùng shadcn.

## Không được làm

- Gradient chữ, kicker/eyebrow phía trên tiêu đề, viền màu dày bên trái thẻ, bóng đổ cứng, emoji làm icon.
- Dùng màu làm cách duy nhất phân biệt chủ đề.
- Đưa AI vào mà không có nhãn "có thể sai".
