using System.Text.RegularExpressions;
using Newsdesk.Domain.Entities;

namespace Newsdesk.Application.Features.Repos
{
    public static partial class RepoRules
    {
        /// <summary>Số repo giới thiệu mỗi ngày.</summary>
        public const int DailyCount = 12;

        /// <summary>"Đang lên" = tạo trong chừng này ngày gần đây mà đã nhiều sao.</summary>
        public const int WindowDays = 14;

        public const int MinStars = 100;

        /// <summary>README ngắn hơn mức này (sau khi làm sạch) coi như không có nội dung để tóm tắt.</summary>
        public const int MinReadmeLength = 200;

        public const int ReadmeExcerptMax = 2000;

        /// <summary>Số lần thử tóm tắt tối đa cho một repo.</summary>
        public const int MaxAttempts = 3;

        /// <summary>RepoKind.Ai nếu tên, mô tả hoặc chủ đề khớp từ khoá AI. Luật từ khoá, bắt sót/nhầm là có (shortcut).</summary>
        public static string KindOf(string name, string? description, IEnumerable<string> topics) =>
            AiRegex().IsMatch($"{name} {description} {string.Join(' ', topics)}") ? RepoKind.Ai : RepoKind.Other;

        /// <summary>Danh sách "awesome-..." chỉ là danh sách link, không phải sản phẩm: bỏ để không chiếm chỗ.</summary>
        public static bool IsLinkList(string name, IEnumerable<string> topics) =>
            name.Contains("awesome", StringComparison.OrdinalIgnoreCase) || topics.Any(t => t.StartsWith("awesome", StringComparison.OrdinalIgnoreCase));

        /// <summary>Bỏ ảnh, huy hiệu, thẻ HTML, chú thích; giữ chữ của link; gộp dòng trống; cắt ngắn. Chỉ để làm đầu vào cho AI.</summary>
        public static string CleanReadme(string markdown)
        {
            string s = HtmlCommentRegex().Replace(markdown, " ");
            s = BadgeLinkRegex().Replace(s, " ");
            s = ImageRegex().Replace(s, " ");
            s = HtmlTagRegex().Replace(s, " ");
            s = LinkRegex().Replace(s, "$1");
            s = Regex.Replace(s, @"[ \t]+", " ");
            s = Regex.Replace(s, @"\n\s*\n+", "\n\n").Trim();
            return s.Length <= ReadmeExcerptMax ? s : s[..ReadmeExcerptMax];
        }

        [GeneratedRegex(@"\b(ai|llms?|gpt\w*|openai|anthropic|claude|gemini|copilot|agents?|agentic|mcp|rag|machine[- ]learning|deep[- ]learning|neural|transformers?|diffusion|embeddings?|inference|chatbots?|generative|langchain|ollama)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex AiRegex();

        [GeneratedRegex(@"<!--.*?-->", RegexOptions.Singleline)]
        private static partial Regex HtmlCommentRegex();

        // [![alt](img)](link): huy hiệu bọc trong link.
        [GeneratedRegex(@"\[!\[[^\]]*\]\([^)]*\)\]\([^)]*\)")]
        private static partial Regex BadgeLinkRegex();

        [GeneratedRegex(@"!\[[^\]]*\]\([^)]*\)")]
        private static partial Regex ImageRegex();

        [GeneratedRegex(@"<[^>]+>")]
        private static partial Regex HtmlTagRegex();

        [GeneratedRegex(@"\[([^\]]+)\]\([^)]*\)")]
        private static partial Regex LinkRegex();
    }
}
