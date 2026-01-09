using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DataHub.Host.Services
{
    public class DefaultAnonymousAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "DevUser"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("permission", "Permissions.Alarms.View"),
                new Claim("permission", "Permissions.Alarms.Edit"),
                new Claim("permission", "Permissions.Alarms.Delete") // Add permissions as needed
            }, "Development");

            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
    }
}
