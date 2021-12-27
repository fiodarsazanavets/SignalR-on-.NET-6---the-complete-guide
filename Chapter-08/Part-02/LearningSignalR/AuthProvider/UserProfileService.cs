using AuthProvider.Models;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthProvider
{
    public class UserProfileService : IProfileService
    {
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory;
        private readonly UserManager<ApplicationUser> usersManager;

        public UserProfileService(
            UserManager<ApplicationUser> usersManager,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory)
        {
            this.usersManager = usersManager;
            this.claimsFactory = claimsFactory;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subject = context.Subject.GetSubjectId();
            var user = await usersManager.FindByIdAsync(subject);
            var claimsPrincipal = await claimsFactory.CreateAsync(user);
            var claimsList = claimsPrincipal.Claims.ToList();

            claimsList = claimsList
                .Where(c => context.RequestedClaimTypes
                .Contains(c.Type))
                .ToList();

            // Add user-specific claims
            claimsList.Add(new Claim(JwtClaimTypes.Email, user.Email));
            claimsList.Add(new Claim(JwtClaimTypes.Name, user.UserName));

            if (usersManager.SupportsUserRole)
            {
                foreach (var roleName in await
                usersManager.GetRolesAsync(user))
                {
                    claimsList.Add(new
                    Claim(JwtClaimTypes.Role, roleName));

                    // Add a special claim for an admin user
                    if (roleName == "admin")
                        claimsList.Add(new Claim("admin", "true"));
                }
            }
            context.IssuedClaims = claimsList;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var subject = context.Subject.GetSubjectId();
            var user = await usersManager.FindByIdAsync(subject);
            context.IsActive = user != null;
        }
    }
}
