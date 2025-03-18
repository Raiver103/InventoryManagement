using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace InventoryManagement.Infrastructure.Identity
{
    public class ClaimsTransformation : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = (ClaimsIdentity)principal.Identity;
            var roleClaim = principal.FindFirst(c => c.Type == "https://yourdomain.com/claims/roles");

            if (roleClaim != null)
            {
                var roles = roleClaim.Value.Split(',');

                foreach (var role in roles)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }

            return Task.FromResult(principal);
        }
    }
}
