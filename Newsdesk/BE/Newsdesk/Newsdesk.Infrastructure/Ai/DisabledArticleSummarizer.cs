using Newsdesk.Application.Common;

namespace Newsdesk.Infrastructure.Ai
{
    /// <summary>Dùng khi Ai:Provider = "None": mọi lần tóm tắt báo "chưa sẵn sàng" nên lô bị dừng êm, không tính là bài lỗi.</summary>
    public sealed class DisabledArticleSummarizer : IArticleSummarizer
    {
        public Task<SummaryResult> SummarizeAsync(SummarizeInput input, CancellationToken cancellationToken) =>
            throw new SummarizerUnavailableException("AI summarization is turned off (Ai:Provider = None).");
    }
}
