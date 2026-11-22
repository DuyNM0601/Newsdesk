namespace Newsdesk.Application.Features.Digest.Models
{
    /// <summary>Một bài trong bản tin kèm tóm tắt AI (nếu đã có). Link về bài gốc luôn có để người đọc xem chi tiết.</summary>
    public class DigestItemModel
    {
        public int Rank { get; set; }

        public string ArticleId { get; set; } = default!;

        public string Title { get; set; } = default!;

        /// <summary>Link về bài gốc trên trang của nguồn.</summary>
        public string Url { get; set; } = default!;

        public string Category { get; set; } = default!;

        public string SourceName { get; set; } = default!;

        public DateTimeOffset PublishedAt { get; set; }

        public string? ImageUrl { get; set; }

        public int Priority { get; set; }

        public string[] Topics { get; set; } = [];

        /// <summary>Ý chính trong một hai câu; null nếu bài chưa được tóm tắt.</summary>
        public string? Tldr { get; set; }

        public string[] KeyPoints { get; set; } = [];

        /// <summary>Vì sao đáng quan tâm với người làm phần mềm; rỗng nếu không liên quan.</summary>
        public string? WhyItMatters { get; set; }

        /// <summary>Ngôn ngữ của bản tóm tắt ("en", "vi"); null nếu chưa có.</summary>
        public string? Language { get; set; }
    }
}
