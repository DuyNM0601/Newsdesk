using System.Text.Json.Serialization;
using Newsdesk.Domain.Entities;

namespace Newsdesk.Application.Features.News.Models
{
    /// <summary>Bài tin trả về client, không lộ Entity.</summary>
    public class ArticleModel
    {
        public string Id { get; set; } = default!;

        public string Title { get; set; } = default!;

        /// <summary>Link về bài gốc.</summary>
        public string Url { get; set; } = default!;

        public string? Summary { get; set; }

        public string? ImageUrl { get; set; }

        /// <summary>NewsCategory.* ("Tech", "Programming", "Politics", "Economy").</summary>
        public string Category { get; set; } = default!;

        public string SourceName { get; set; } = default!;

        public DateTimeOffset PublishedAt { get; set; }

        /// <summary>Điểm quan trọng 0-100 (luật từ khoá, chưa phải AI).</summary>
        public int Priority { get; set; }

        /// <summary>NewsTopic.* khớp bài này: "Ai", "Dev", "DevImpact".</summary>
        public string[] Topics { get; set; } = [];

        /// <summary>Chuỗi thô từ cột Topics, chỉ để handler tách thành mảng; không nằm trong JSON.</summary>
        [JsonIgnore]
        public string TopicsRaw { get; set; } = "";
    }

    public class ArticlePageModel
    {
        public IReadOnlyList<ArticleModel> Items { get; set; } = [];

        public int Page { get; set; }

        public int PageSize { get; set; }

        public bool HasMore { get; set; }
    }

    public class SourceModel
    {
        public string Id { get; set; } = default!;

        public string Name { get; set; } = default!;

        public string Category { get; set; } = default!;

        public string Language { get; set; } = default!;

        /// <summary>"Active" hoặc "Inactive" (SourceStatus).</summary>
        public string Status { get; set; } = default!;

        public DateTimeOffset? LastFetchedAt { get; set; }

        /// <summary>"Success" hoặc "Failed" (FetchStatus); null nếu chưa tải.</summary>
        public string? LastFetchStatus { get; set; }

        public static SourceModel From(Source source) => new()
        {
            Id = source.Id,
            Name = source.Name,
            Category = source.Category,
            Language = source.Language,
            Status = source.Status,
            LastFetchedAt = source.LastFetchedAt,
            LastFetchStatus = source.LastFetchStatus,
        };
    }
}
