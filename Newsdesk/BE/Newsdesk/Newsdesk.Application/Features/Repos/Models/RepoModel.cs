namespace Newsdesk.Application.Features.Repos.Models
{
    /// <summary>Một repo GitHub "đang lên" kèm tóm tắt AI. Link về GitHub luôn có để xem chi tiết.</summary>
    public class RepoModel
    {
        public string FullName { get; set; } = default!;

        /// <summary>Link tới repo trên GitHub.</summary>
        public string Url { get; set; } = default!;

        public string? Description { get; set; }

        public string? Language { get; set; }

        /// <summary>Mã giấy phép SPDX; null nếu repo không khai báo.</summary>
        public string? License { get; set; }

        /// <summary>Số sao lúc phát hiện (không cập nhật sau đó).</summary>
        public int Stars { get; set; }

        /// <summary>"Ai" hoặc "Other" (RepoKind).</summary>
        public string Kind { get; set; } = default!;

        /// <summary>Ngày giới thiệu, yyyy-MM-dd (giờ Việt Nam).</summary>
        public string DiscoveredOn { get; set; } = default!;

        /// <summary>Repo này làm gì, một hai câu; null nếu chưa tóm tắt.</summary>
        public string? Tldr { get; set; }

        public string[] KeyPoints { get; set; } = [];

        /// <summary>Dành cho ai / dùng khi nào; null nếu rỗng.</summary>
        public string? WhyItMatters { get; set; }

        public string? SummaryLanguage { get; set; }
    }
}
