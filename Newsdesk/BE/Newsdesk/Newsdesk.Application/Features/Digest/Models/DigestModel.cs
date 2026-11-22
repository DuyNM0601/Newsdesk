namespace Newsdesk.Application.Features.Digest.Models
{
    /// <summary>Bản tin của một ngày (giờ Việt Nam). Date null nếu chưa có bản tin nào.</summary>
    public class DigestModel
    {
        /// <summary>Ngày dạng yyyy-MM-dd.</summary>
        public string? Date { get; set; }

        public int Total { get; set; }

        /// <summary>Số bài đã có tóm tắt AI (phần còn lại đang chờ tóm tắt).</summary>
        public int Summarized { get; set; }

        public IReadOnlyList<DigestItemModel> Items { get; set; } = [];
    }
}
