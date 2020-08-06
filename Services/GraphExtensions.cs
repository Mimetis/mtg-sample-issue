using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Mtg.Sample.Models;
using Mtg.Sample.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GraphExtensions
    {
        /// <summary>
        /// Add a IAuthProvider and IGraphProvider singleton
        /// </summary>
        public static IServiceCollection AddMicrosoftGraph(this IServiceCollection services, IConfiguration configuration, string configSectionName = "Graph")
        {
            // Register Configuration as available as IOption<GraphOptions>
            services.Configure<GraphOptions>(options => configuration.Bind(configSectionName, options));

            services.AddScoped<IAuthProvider, AuthProvider>();

            // Add required services, in case we don't have them yet
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            // Add GraphClient provider
            services.AddScoped<IGraphServiceClient>(serviceProvider =>
           {
                // Get auth provider
                var authProvider = serviceProvider.GetService<IAuthProvider>();

                // Get instance of graph options
                var graphOptions = serviceProvider.GetService<IOptions<GraphOptions>>();

                //// Scopes which are already requested by MSAL.NET. They should not be re-requested;.
                var scopesRequestedByMsal = new string[] { "openid", "profile", "offline_access" };

               var scopes = graphOptions.Value.GetScopes().Except(scopesRequestedByMsal);

                // get MSAL graph client
                var msalGraphClient = new GraphServiceClient(new GraphAuthenticationProvider(authProvider, scopes));

               return msalGraphClient;
           });

            return services;
        }


        //    /// <summary>
        //    /// Add OpenId Authentication mechanism and User Authentication to services collection
        //    /// </summary>
        //    public static IServiceCollection AddUserAuth(this IServiceCollection services, Action<AzureAdOptions> azureAdConfigureoptions)
        //    {
        //        AuthenticationBuilder builder = services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme);

        //        builder.AddUserAuth(azureAdConfigureoptions);

        //        return services;
        //    }

        //    /// <summary>
        //    /// Add a IAuthProvider, IGraphProvider singletons, 
        //    /// </summary>
        //    public static AuthenticationBuilder AddUserAuth(this AuthenticationBuilder builder, Action<AzureAdOptions> azureAdConfigureoptions)
        //    {
        //        // Add Graph Authentication
        //        AddMicrosoftGraph(builder.Services, azureAdConfigureoptions);

        //        // Add cookie management to associate a session with a cookie on the client machine
        //        builder.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        //        // Add open id connect authentication
        //        builder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        //        {
        //            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        //            // Get service provider to be able to get azure ad options
        //            var serviceProvider = builder.Services.BuildServiceProvider();

        //            // get instance of azure ad options
        //            var azureAdOptions = serviceProvider.GetService<IOptions<AzureAdOptions>>().Value;

        //            if (string.IsNullOrWhiteSpace(options.Authority))
        //                options.Authority = YAuthorityHelper.BuildAuthorityV2(azureAdOptions);

        //            //options.RequireHttpsMetadata = false;

        //            if (string.IsNullOrWhiteSpace(options.ClientId))
        //                options.ClientId = azureAdOptions.ClientId;

        //            if (string.IsNullOrWhiteSpace(options.ClientSecret))
        //                options.ClientSecret = azureAdOptions.ClientSecret;

        //            options.ResponseType = "code id_token";

        //            options.SaveTokens = true;
        //            options.GetClaimsFromUserInfoEndpoint = true;

        //            foreach (var scope in azureAdOptions.GraphScopes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
        //                if (!options.Scope.Contains(scope))
        //                    options.Scope.Add(scope);


        //            // Simplify Token validations
        //            options.TokenValidationParameters = new TokenValidationParameters()
        //            {
        //                ValidateAudience = false,
        //                ValidateIssuer = false,
        //                ValidateIssuerSigningKey = false
        //            };

        //            // Avoids having users being presented the select account dialog when they are already signed-in
        //            // for instance when going through incremental consent
        //            var redirectToIdpHandler = options.Events.OnRedirectToIdentityProvider;
        //            options.Events.OnRedirectToIdentityProvider = async context =>
        //            {
        //                var login = context.Properties.GetParameter<string>(OpenIdConnectParameterNames.LoginHint);
        //                if (!string.IsNullOrWhiteSpace(login))
        //                {
        //                    context.ProtocolMessage.LoginHint = login;
        //                    context.ProtocolMessage.DomainHint = context.Properties.GetParameter<string>(
        //                        OpenIdConnectParameterNames.DomainHint);

        //                    // delete the login_hint and domainHint from the Properties when we are done otherwise
        //                    // it will take up extra space in the cookie.
        //                    context.Properties.Parameters.Remove(OpenIdConnectParameterNames.LoginHint);
        //                    context.Properties.Parameters.Remove(OpenIdConnectParameterNames.DomainHint);
        //                }

        //                // context.ProtocolMessage.SetParameter("client_info", "1");


        //                await redirectToIdpHandler(context).ConfigureAwait(false);
        //            };

        //            // Handling the auth redemption by MSAL.NET so that a token is available in the token cache
        //            // where it will be usable from Controllers later (through the TokenAcquisition service)
        //            var codeReceivedHandler = options.Events.OnAuthorizationCodeReceived;
        //            options.Events.OnAuthorizationCodeReceived = async context =>
        //            {
        //                var scope = options.Scope;

        //                // As AcquireTokenByAuthorizationCodeAsync is asynchronous we want to tell ASP.NET core that we are handing the code
        //                // even if it's not done yet, so that it does not concurrently call the Token endpoint. (otherwise there will be a
        //                // race condition ending-up in an error from Azure AD telling "code already redeemed")
        //                context.HandleCodeRedemption();

        //                // The cache will need the claims from the ID token.
        //                // If they are not yet in the HttpContext.User's claims, add them here.
        //                if (!context.HttpContext.User.Claims.Any())
        //                    (context.HttpContext.User.Identity as ClaimsIdentity).AddClaims(context.Principal.Claims);

        //                var autProvider = context.HttpContext.RequestServices.GetRequiredService<IAuthProvider>();
        //                var result = await autProvider.GetUserAccessTokenByAuthorizationCode(context.ProtocolMessage.Code).ConfigureAwait(false);

        //                context.HandleCodeRedemption(null, result.IdToken);

        //                await codeReceivedHandler(context).ConfigureAwait(false);
        //            };

        //            // Handling the token validated to get the client_info for cases where tenantId is not present (example: B2C)
        //            var onTokenValidatedHandler = options.Events.OnTokenValidated;
        //            options.Events.OnTokenValidated = async context =>
        //            {
        //                if (context.Request.Form.ContainsKey(ClaimConstants.ClientInfo))
        //                {
        //                    context.Request.Form.TryGetValue(ClaimConstants.ClientInfo, out Microsoft.Extensions.Primitives.StringValues value);
        //                    ClientInfo clientInfoFromServer = null;

        //                    var jsonByteArray = Base64UrlHelpers.DecodeToBytes(value);

        //                    if (!string.IsNullOrEmpty(value))
        //                    {
        //                        using (var stream = new MemoryStream(jsonByteArray))
        //                        {
        //                            using (var reader = new StreamReader(stream, Encoding.UTF8))
        //                            {
        //                                using (var jtr = new JsonTextReader(reader))
        //                                {
        //                                    var jobject = await JObject.LoadAsync(jtr);

        //                                    clientInfoFromServer = jobject.ToObject<ClientInfo>();
        //                                }

        //                            }

        //                        }
        //                    }

        //                    if (clientInfoFromServer != null)
        //                    {
        //                        context.Principal.Identities.FirstOrDefault().AddClaim(
        //                            new Claim(ClaimConstants.UniqueTenantIdentifier, clientInfoFromServer.UniqueTenantIdentifier));
        //                        context.Principal.Identities.FirstOrDefault().AddClaim(
        //                            new Claim(ClaimConstants.UniqueObjectIdentifier, clientInfoFromServer.UniqueObjectIdentifier));
        //                    }
        //                }
        //                await onTokenValidatedHandler(context).ConfigureAwait(false);
        //            };

        //            // Handling the sign-out: removing the account from MSAL.NET cache
        //            var signOutHandler = options.Events.OnRedirectToIdentityProviderForSignOut;
        //            options.Events.OnRedirectToIdentityProviderForSignOut = async context =>
        //            {

        //                var autProvider = context.HttpContext.RequestServices.GetRequiredService<IAuthProvider>();
        //                await autProvider.RemoveAccountAsync().ConfigureAwait(false);

        //                await signOutHandler(context).ConfigureAwait(false);
        //            };

        //        });

        //        return builder;

        //    }

        //    /// <summary>
        //    /// Add Authentication services (AddAuthentication()) with JWT as default schema. 
        //    /// Then add JWT Bearer token validation for protecting any API with the [Authorize] attribute
        //    /// </summary>
        //    public static IServiceCollection AddWebApiAuth(
        //        this IServiceCollection services,
        //        Action<AzureAdOptions> azureAdConfigureoptions)
        //    {
        //        // Add authentication with default shcema as "Bearer"
        //        AuthenticationBuilder builder = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

        //        // Add protect web api
        //        builder.AddWebApiAuth(azureAdConfigureoptions);

        //        return services;
        //    }

        //    /// <summary>
        //    /// Add JWT Bearer token validation for protecting any API with the [Authorize] attribute
        //    /// </summary>
        //    public static AuthenticationBuilder AddWebApiAuth(this AuthenticationBuilder builder, Action<AzureAdOptions> azureAdConfigureoptions)
        //    {
        //        if (builder == null)
        //            throw new ArgumentNullException(nameof(builder));

        //        // Register Configuration as available as IOption<T>
        //        builder.Services.Configure(azureAdConfigureoptions);

        //        // Add required services, in case we don't have them yet
        //        builder.Services.AddHttpContextAccessor();
        //        builder.Services.AddHttpClient();

        //        builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        //        {
        //            // Get service provider to be able to get azure ad options
        //            var serviceProvider = builder.Services.BuildServiceProvider();

        //            // get instance of azure ad options
        //            var azureAdOptions = serviceProvider.GetService<IOptions<AzureAdOptions>>().Value;

        //            // Check if we have already an authority value
        //            // Otherwise create from Instance / Tenant / "v2.0" properties
        //            if (string.IsNullOrWhiteSpace(options.Authority))
        //                options.Authority = YAuthorityHelper.BuildAuthorityV2(azureAdOptions);

        //            // Check if we have authority mentioned. Otherwise fill with the ClientId property
        //            if (string.IsNullOrWhiteSpace(options.Audience))
        //                options.Audience = azureAdOptions.ClientId;

        //            // Simplify Token validations
        //            options.TokenValidationParameters = new TokenValidationParameters()
        //            {
        //                ValidateAudience = false,
        //                ValidateIssuer = false,
        //                ValidateIssuerSigningKey = false
        //            };

        //        });

        //        return builder;
        //    }
    }


    //[DataContract]
    //internal class ClientInfo
    //{
    //    [DataMember(Name = "uid", IsRequired = false)]
    //    public string UniqueObjectIdentifier { get; set; }

    //    [DataMember(Name = "utid", IsRequired = false)]
    //    public string UniqueTenantIdentifier { get; set; }
    //}
}
