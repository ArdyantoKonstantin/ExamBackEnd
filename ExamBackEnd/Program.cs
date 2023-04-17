using ExamBackEnd.Entities;
using ExamBackEnd.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRazorPages();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database
builder.Services.AddDbContextPool<ApplicationDbContext>(Q =>
{
    Q.UseSqlite("Data Source=local.db");
    Q.UseOpenIddict();
});

builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/SignIn";
        options.LogoutPath = "/connect/logout";
    });

builder.Services.AddAuthorization(options => {
    options.AddPolicy("api", p =>
    {
        p.AddAuthenticationSchemes(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        p.RequireClaim(Claims.Private.Scope, "api");
    });
});

builder.Services.AddDataProtection().PersistKeysToDbContext<ApplicationDbContext>();

builder.Services.AddOpenIddict()
    // Register the OpenIddict core components.
    .AddCore(options =>
    {
        // Configure OpenIddict to use the Entity Framework Core stores and models.
        // Note: call ReplaceDefaultEntities() to replace the default entities.
        options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>();
    })
    // Register the OpenIddict server components.
    .AddServer(options =>
    {
        // Enable the authorization, token, introspection and userinfo endpoints.
        options.SetAuthorizationEndpointUris(OpenIdSettings.Endpoints.Authorization)
                .SetTokenEndpointUris(OpenIdSettings.Endpoints.Token)
                .SetIntrospectionEndpointUris(OpenIdSettings.Endpoints.Introspection)
                .SetUserinfoEndpointUris(OpenIdSettings.Endpoints.Userinfo)
                .SetRevocationEndpointUris(OpenIdSettings.Endpoints.Revoke)
                .SetLogoutEndpointUris(OpenIdSettings.Endpoints.Logout);

        // Enable the client credentials flow for machine to machine auth.
        options.AllowClientCredentialsFlow();

        // Enable the authorization code flow and refresh token flow for native and web apps.
        options.AllowAuthorizationCodeFlow();
        options.AllowRefreshTokenFlow();

        // Expose all the supported claims in the discovery document.
        options.RegisterClaims(OpenIdSettings.Claims);

        // Expose all the supported scopes in the discovery document.
        options.RegisterScopes(OpenIdSettings.Scopes);

        // Register the signing and encryption credentials.
        options.AddDevelopmentEncryptionCertificate();
        options.AddDevelopmentSigningCertificate();

        // Register the ASP.NET Core host and configure the ASP.NET Core options.
        options.UseAspNetCore()
                .DisableTransportSecurityRequirement()
                .EnableAuthorizationEndpointPassthrough()
                .EnableTokenEndpointPassthrough()
                .EnableUserinfoEndpointPassthrough()
                .EnableLogoutEndpointPassthrough();

        // Create Data Protection tokens instead of JWT tokens.
        // ASP.NET Core Data Protection uses its own key ring to encrypt and protect tokens against tampering
        // and is supported for all types of tokens, except identity tokens, that are always JWT tokens.
        options.UseDataProtection();

        // Configures OpenIddict to use reference tokens, so that the access token payloads
        // are stored in the database (only an identifier is returned to the client application).
        options.UseReferenceAccessTokens()
            .UseReferenceRefreshTokens();

        options.SetAccessTokenLifetime(TimeSpan.FromHours(24));
        options.SetRefreshTokenLifetime(TimeSpan.FromDays(30));
        options.SetRefreshTokenReuseLeeway(TimeSpan.FromSeconds(60));
    })
    // Register the OpenIddict validation components.
    .AddValidation(options =>
    {
        // Import the configuration from the local OpenIddict server instance.
        options.UseLocalServer();

        // Enable authorization entry validation, which is required to be able
        // to reject access tokens retrieved from a revoked authorization code.
        options.EnableAuthorizationEntryValidation();

        // Enables token validation so that a database call is made for each API request,
        // required when the OpenIddict server is configured to use reference tokens.
        options.EnableTokenEntryValidation();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
        options.UseDataProtection();
    });

// never do this in production!!!
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<AutomaticMigrationService>();
    builder.Services.AddHostedService<SetupDevelopmentEnvironmentHostedService>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
