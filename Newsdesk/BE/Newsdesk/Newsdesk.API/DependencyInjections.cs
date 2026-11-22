using Newsdesk.API.Shared;
using Newsdesk.Application.Common;

namespace Newsdesk.API
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddApi(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUser>();
            return services;
        }
    }
}
