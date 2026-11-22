using Microsoft.IdentityModel.JsonWebTokens;
using Newsdesk.Application.Common;

namespace Newsdesk.API.Shared
{
    public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
    {
        public string? UserId => accessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        public string? DisplayName => accessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Name)?.Value;
    }
}
