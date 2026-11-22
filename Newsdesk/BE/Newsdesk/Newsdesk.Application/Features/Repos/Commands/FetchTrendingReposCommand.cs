using Newsdesk.Application.Shared;

namespace Newsdesk.Application.Features.Repos.Commands
{
    /// <summary>
    /// Chọn khoảng 12 repo GitHub "đang lên" (tạo trong 14 ngày gần đây, nhiều sao, AI trước) chưa từng giới thiệu, lấy README của chúng
    /// và lưu cho ngày hôm nay (giờ Việt Nam). Chỉ MỘT request tìm kiếm + một request README cho mỗi repo được chọn.
    /// Trả về số repo mới; 0 nếu hôm nay đã chạy rồi (idempotent).
    /// </summary>
    public sealed record FetchTrendingReposCommand : ICommand<int>;
}
