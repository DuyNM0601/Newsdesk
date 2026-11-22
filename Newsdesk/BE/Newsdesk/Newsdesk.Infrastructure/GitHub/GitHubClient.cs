using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Newsdesk.Application.Common;

namespace Newsdesk.Infrastructure.GitHub
{
    /// <summary>
    /// GitHub REST API công khai, miễn phí, chỉ HttpClient (không thêm package). Số request tối thiểu: MỘT tìm kiếm cho cả ngày và một
    /// request README cho mỗi repo được chọn. Chỉ đọc dữ liệu công khai nên token (nếu có) KHÔNG cần quyền nào.
    /// </summary>
    public sealed partial class GitHubClient(HttpClient http) : IGitHubClient
    {
        public async Task<IReadOnlyList<GitHubRepoInfo>> SearchRisingAsync(DateTimeOffset createdAfter, int minStars, CancellationToken cancellationToken)
        {
            string q = $"created:>{createdAfter:yyyy-MM-dd} stars:>{minStars}";
            using HttpResponseMessage response = await SendAsync($"/search/repositories?q={Uri.EscapeDataString(q)}&sort=stars&order=desc&per_page=100", raw: false, cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

            var repos = new List<GitHubRepoInfo>();
            if (!doc.RootElement.TryGetProperty("items", out JsonElement items) || items.ValueKind != JsonValueKind.Array)
                return repos;

            foreach (JsonElement r in items.EnumerateArray())
            {
                // Bản fork, repo đã lưu trữ / bị vô hiệu không phải thứ đáng giới thiệu.
                if (Flag(r, "fork") || Flag(r, "archived") || Flag(r, "disabled"))
                    continue;

                string? fullName = Str(r, "full_name");
                string? url = Str(r, "html_url");
                if (fullName is null || url is null || !FullNameRegex().IsMatch(fullName))
                    continue;

                string? license = r.TryGetProperty("license", out JsonElement l) && l.ValueKind == JsonValueKind.Object ? Str(l, "spdx_id") : null;
                var topics = new List<string>();
                if (r.TryGetProperty("topics", out JsonElement t) && t.ValueKind == JsonValueKind.Array)
                    topics.AddRange(t.EnumerateArray().Select(x => x.GetString()).OfType<string>());

                repos.Add(new GitHubRepoInfo(
                    fullName,
                    url,
                    Str(r, "description"),
                    Str(r, "language"),
                    license is "NOASSERTION" ? null : license,
                    topics,
                    r.TryGetProperty("stargazers_count", out JsonElement s) ? s.GetInt32() : 0,
                    r.TryGetProperty("created_at", out JsonElement c) && c.TryGetDateTimeOffset(out DateTimeOffset created) ? created : DateTimeOffset.UtcNow));
            }

            return repos;
        }

        public async Task<string?> GetReadmeAsync(string fullName, CancellationToken cancellationToken)
        {
            if (!FullNameRegex().IsMatch(fullName))
                throw new ArgumentException("Invalid repository name.", nameof(fullName));

            using HttpResponseMessage response = await SendAsync($"/repos/{fullName}/readme", raw: true, cancellationToken, allowNotFound: true);
            return response.StatusCode == HttpStatusCode.NotFound ? null : await response.Content.ReadAsStringAsync(cancellationToken);
        }

        private async Task<HttpResponseMessage> SendAsync(string path, bool raw, CancellationToken ct, bool allowNotFound = false)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            if (raw)
                request.Headers.Accept.ParseAdd("application/vnd.github.raw+json");

            HttpResponseMessage response;
            try
            {
                response = await http.SendAsync(request, ct);
            }
            catch (HttpRequestException ex)
            {
                throw new GitHubUnavailableException("Cannot reach api.github.com.", ex);
            }

            if (response.IsSuccessStatusCode || (allowNotFound && response.StatusCode == HttpStatusCode.NotFound))
                return response;

            int code = (int)response.StatusCode;
            response.Dispose();
            throw new GitHubUnavailableException(code switch
            {
                401 => "GitHub rejected the token (GitHub:Token is invalid or expired).",
                403 or 429 => "GitHub rate limit reached. Set a read-only token in GitHub:Token (free) for a much higher limit.",
                _ => $"GitHub returned HTTP {code}.",
            });
        }

        private static string? Str(JsonElement e, string name) =>
            e.TryGetProperty(name, out JsonElement v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

        private static bool Flag(JsonElement e, string name) =>
            e.TryGetProperty(name, out JsonElement v) && v.ValueKind == JsonValueKind.True;

        [GeneratedRegex(@"^[\w.-]+/[\w.-]+$")]
        private static partial Regex FullNameRegex();
    }
}
