using Newsdesk.Domain.Entities;

namespace Newsdesk.Application.Features.Digest
{
    public static class DigestRules
    {
        /// <summary>Số bài trong bản tin mỗi ngày.</summary>
        public const int Size = 50;

        /// <summary>Một nguồn chiếm tối đa chừng này bài, để không để một feed ồ ạt lấn hết bản tin.</summary>
        public const int MaxPerSource = 8;

        /// <summary>Số lần thử tóm tắt tối đa cho một bài (kể cả thất bại) trước khi bỏ cuộc.</summary>
        public const int MaxAttempts = 3;

        /// <summary>Bài phải có đoạn trích tối thiểu chừng này ký tự mới tóm tắt được trung thực; chỉ có tiêu đề thì model sẽ bịa.</summary>
        public const int MinExcerptLength = 40;

        /// <summary>Bản tin gom bài đăng trong khoảng này trước lúc tạo.</summary>
        public static readonly TimeSpan Window = TimeSpan.FromHours(24);

        /// <summary>
        /// Hạn ngạch theo mục (tổng 50) để bản tin có cả AI/công nghệ/lập trình lẫn kinh tế, chính trị. Phần còn trống của mục
        /// nào thiếu bài sẽ được điền bằng bài điểm cao nhất của mục khác.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, int> CategoryQuota = new Dictionary<string, int>
        {
            [NewsCategory.Programming] = 16,
            [NewsCategory.Tech] = 16,
            [NewsCategory.Economy] = 10,
            [NewsCategory.Politics] = 8,
        };

        /// <summary>
        /// Ngày của bản tin tính theo giờ Việt Nam (UTC+7, không có giờ mùa hè).
        /// shortcut: múi giờ cố định UTC+7; đưa vào cấu hình khi phục vụ người dùng ở múi giờ khác.
        /// </summary>
        public static DateOnly TodayInVietnam(DateTimeOffset utcNow) => DateOnly.FromDateTime(utcNow.ToOffset(TimeSpan.FromHours(7)).DateTime);
    }
}
