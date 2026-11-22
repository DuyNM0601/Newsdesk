using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newsdesk.Domain.Entities;

namespace Newsdesk.Application.Features.News
{
    public static partial class NewsRules
    {
        public const int MaxPageSize = 50;

        /// <summary>Số mục tối đa lấy từ mỗi lần tải một feed.</summary>
        public const int MaxItemsPerFetch = 50;

        /// <summary>Từ mức này trở lên là "quan trọng" (lọc `important`).</summary>
        public const int ImportantThreshold = 50;

        /// <summary>Sắp xếp theo độ quan trọng chỉ xét bài trong khoảng này, để tin cũ điểm cao không chiếm đầu trang mãi.</summary>
        public static readonly TimeSpan TopWindow = TimeSpan.FromHours(72);

        /// <summary>Chỉ nhận link http/https tuyệt đối, bỏ phần #fragment. Trả null nếu link không hợp lệ.</summary>
        public static string? NormalizeUrl(string? url)
        {
            if (!Uri.TryCreate(url?.Trim(), UriKind.Absolute, out Uri? uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                return null;
            return uri.GetLeftPart(UriPartial.Query);
        }

        /// <summary>SHA-256 dạng hex in thường (64 ký tự) của link đã chuẩn hoá; khoá chống trùng bài.</summary>
        public static string HashUrl(string normalizedUrl) =>
            Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(normalizedUrl)));

        public static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];

        /// <summary>
        /// Chấm độ quan trọng bằng luật từ khoá trên tiêu đề + đoạn trích (tiếng Anh). Ưu tiên của người đọc: AI, lập trình,
        /// rồi tin ảnh hưởng nghề dev. Tin "ảnh hưởng nghề dev" phải vừa có từ khoá tác động (sa thải, giấy phép, lỗ hổng...)
        /// vừa nằm trong bối cảnh công nghệ, để "jobs report" của kinh tế không bị gắn nhầm.
        /// shortcut: luật từ khoá, bắt sót/nhầm là có; thay bằng điểm do AI chấm ở giai đoạn AI.
        /// </summary>
        public static (int Priority, string Topics) Classify(string category, string title, string? summary)
        {
            string text = $"{title} {summary}";
            bool ai = AiRegex().IsMatch(text);
            bool dev = DevRegex().IsMatch(text) || category == NewsCategory.Programming;
            bool techContext = ai || dev || category is NewsCategory.Tech or NewsCategory.Programming;
            bool impact = techContext && ImpactRegex().IsMatch(text);

            int priority = category switch
            {
                NewsCategory.Programming => 30,
                NewsCategory.Tech => 25,
                NewsCategory.Economy => 10,
                _ => 5,
            };
            if (ai) priority += 25;
            if (dev && category != NewsCategory.Programming) priority += 15;
            if (impact) priority += 30;

            var topics = new List<string>();
            if (ai) topics.Add(NewsTopic.Ai);
            if (dev) topics.Add(NewsTopic.Dev);
            if (impact) topics.Add(NewsTopic.DevImpact);
            return (Math.Min(priority, 100), topics.Count == 0 ? "" : $",{string.Join(',', topics)},");
        }

        [GeneratedRegex(@"\b(ai|a\.i\.|llms?|openai|anthropic|chatgpt|gpt-?\d*|claude|gemini|copilot|generative|machine learning|neural|deepmind|mistral|agentic|ai agents?)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex AiRegex();

        [GeneratedRegex(@"\b(developers?|programming|programmers?|software engineer\w*|open[- ]source|github|apis?|sdk|\.net|c#|python|javascript|typescript|rust|golang|kubernetes|docker|framework|compiler|devops|coding|stack overflow|npm|linux)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex DevRegex();

        [GeneratedRegex(@"\b(layoffs?|laid off|hiring|job cuts?|jobs?|salar(?:y|ies)|automation|automate[sd]?|replace[sd]?|outsourc\w*|remote work|visa|licen[cs]e|licen[cs]ing|deprecat\w*|end of life|breach|vulnerabilit\w*|cve|supply[- ]chain|outage|regulat\w*|tariffs?|export controls?)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex ImpactRegex();
    }
}
