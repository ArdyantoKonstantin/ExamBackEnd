using ExamBackEnd.Entities;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ExamBackEnd.Services
{
    public class AutomaticMigrationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IOpenIddictApplicationManager _appManager;
        private readonly IOpenIddictScopeManager _scopeManager;

        public AutomaticMigrationService(
            ApplicationDbContext applicationDbContext,
            IOpenIddictApplicationManager openIddictApplicationManager,
            IOpenIddictScopeManager openIddictScopeManager
        )
        {
            _db = applicationDbContext;
            _appManager = openIddictApplicationManager;
            _scopeManager = openIddictScopeManager;
        }

        public async Task MigrateAsync(CancellationToken cancellationToken)
        {
            await _db.Database.MigrateAsync(cancellationToken);
            await CreateApiServerApp(cancellationToken);
            await CreateApiScope(cancellationToken);
            await CreateCmsApp(cancellationToken);
            await AddAdministrator(cancellationToken);
        }

        private async Task AddAdministrator(CancellationToken cancellationToken)
        {
            var id = "01GXZBZDT9CZRHQF2QCCFBH40N";
            var exist = await _db.Users.Where(Q => Q.Id == id).AnyAsync(cancellationToken);
            if (exist)
            {
                return;
            }

            var account = new User
            {
                Id = id,
                Name = "Customer",
                Email = "customer@shopping.com",
                Password = BCrypt.Net.BCrypt.HashPassword("password"),
            };
            _db.Users.Add(account);
            await _db.SaveChangesAsync(cancellationToken);
        }

        private async Task CreateApiServerApp(CancellationToken cancellationToken)
        {
            var exist = await _appManager.FindByClientIdAsync("api-server", cancellationToken);
            if (exist != null)
            {
                return;
            }

            await _appManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "api-server",
                DisplayName = "API Server",
                Type = ClientTypes.Confidential,
                ClientSecret = "HelloWorld1!",
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.Introspection,
                    Permissions.Endpoints.Revocation,
                    Permissions.GrantTypes.ClientCredentials
                }
            }, cancellationToken);
        }

        private async Task CreateApiScope(CancellationToken cancellationToken)
        {
            var exist = await _scopeManager.FindByNameAsync("api", cancellationToken);
            if (exist != null)
            {
                return;
            }

            await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "api",
                DisplayName = "API Scope",
                Resources =
                {
                    "api-server"
                }
            }, cancellationToken);
        }

        private async Task<string?> CreateCmsApp(CancellationToken cancellationToken)
        {
            var exist = await _appManager.FindByClientIdAsync("cms", cancellationToken);
            if (exist != null)
            {
                return await _appManager.GetIdAsync(exist, cancellationToken);
            }

            var o = await _appManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "cms",
                DisplayName = "CMS (Front-End)",
                RedirectUris = {
                    new Uri("http://localhost:3000/api/auth/callback/oidc"),
                    new Uri("https://oauth.pstmn.io/v1/callback")
                },
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Revocation,
                    Permissions.ResponseTypes.Code,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Roles,
                    Permissions.Scopes.Phone,
                    Permissions.Scopes.Address,
                    Permissions.Prefixes.Scope + "api"
                },
                Type = ClientTypes.Public
            }, cancellationToken);

            return await _appManager.GetIdAsync(o, cancellationToken);
        }
    }
}
