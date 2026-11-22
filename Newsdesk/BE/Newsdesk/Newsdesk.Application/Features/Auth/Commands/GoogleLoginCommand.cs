using Newsdesk.Application.Features.Auth.Models;
using Newsdesk.Application.Shared;

namespace Newsdesk.Application.Features.Auth.Commands
{
    public sealed record GoogleLoginCommand(string IdToken) : ICommand<AuthModel>;
}
