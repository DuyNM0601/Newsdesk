using MediatR;
using Newsdesk.Application.Shared;

namespace Newsdesk.Application.Features.Auth.Commands
{
    /// <summary>Thu hồi refresh token của phiên hiện tại. Luôn thành công (idempotent), kể cả khi token không tồn tại.</summary>
    public sealed record LogoutCommand(string? RefreshToken) : ICommand<Unit>;
}
