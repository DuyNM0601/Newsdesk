# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Stack

Đã có codebase: Vite + React + TypeScript + Tailwind v4 + shadcn/ui (thư mục `Newsdesk/FE/`), BE ASP.NET Core (`Newsdesk/BE/Newsdesk/`).

## Users

Người đi làm Việt Nam, ít thời gian, muốn nắm tin công nghệ, lập trình, chính trị và kinh tế quốc tế mới và quan trọng ở một chỗ, đọc bằng tiếng Việt. Người đọc không cần đăng nhập; đăng nhập Google chỉ dùng cho tính năng cá nhân sau này.

## Product Purpose

Tổng hợp tin mới và quan trọng từ các nguồn quốc tế chính thống (RSS), cho người đọc lọc theo mục (công nghệ, lập trình, chính trị, kinh tế), tìm trong tiêu đề, và đọc nhanh nhờ bản tin hôm nay do AI tóm tắt. Mỗi bài dẫn về bài gốc. Thành công là người đọc nắm được tình hình trong vài phút rồi bấm sang nguồn gốc khi cần.

## Positioning

Nguồn quốc tế chính thống (BBC, The Guardian, Al Jazeera, NPR, CNBC, Ars Technica, The Verge, .NET Blog, GitHub Blog...), không viết lại bài, dẫn về bài gốc, kèm AI tóm tắt dễ hiểu bằng tiếng Việt.

## Operating Context

Dữ liệu từ 16 nguồn RSS, tải mỗi 15 phút. Giao diện tiếng Việt, tiêu đề bài giữ tiếng Anh theo nguồn. Chạy công khai cho nhiều người dùng, trên điện thoại lẫn máy tính.

## Capabilities and Constraints

- Chỉ lưu tiêu đề, đoạn trích của feed và link về bài gốc; KHÔNG lưu hay hiển thị toàn văn bài (bản quyền).
- Có API `GET /news` (lọc theo mục, tìm trong tiêu đề, phân trang) và `GET /sources`.
- AI tóm tắt chạy nền, kết quả lưu sẵn; luôn có nhãn "AI tóm tắt, có thể sai". Chưa làm xong, mọi dữ liệu AI trên thiết kế là minh hoạ.
- Khoá học lập trình là danh sách chọn tay, chưa có dữ liệu.
- Chưa quyết: chế độ tối, ảnh bài viết (nhiều feed có ảnh, một số không).

## Brand Commitments

Tên Newsdesk. Giao diện tiếng Việt. Dùng font hỗ trợ tiếng Việt đầy đủ (có thể là Be Vietnam Pro).

## Evidence on Hand

16 nguồn RSS thật đã tải được (401 bài thử). Chưa có logo, ảnh thương hiệu, số liệu người dùng. Không bịa số liệu, lượt đọc hay lời chứng thực.

## Product Principles

1. Nguồn gốc là trên hết: luôn thấy tên nguồn, thời gian và đường dẫn về bài gốc.
2. Đọc nhanh: người bận nắm được ý chính trong vài phút.
3. AI luôn được gắn nhãn và không thay thế bài gốc.
4. Trung lập, không giật gân, nhất là tin chính trị.

## Accessibility & Inclusion

Chữ tiếng Việt có dấu đọc rõ ở mọi cỡ, tương phản đạt WCAG AA, dùng được bằng bàn phím, hợp điện thoại.
