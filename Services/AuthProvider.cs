using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace Mtg.Sample.Services
{

    public interface IAuthProvider
    {

        Task<string> GetAccessTokenForAppAsync();
        Task<string> GetAccessTokenForUserAsync(IEnumerable<string> scopes);

    }

    /// <summary>
    /// Auth provider class to encapsulate a call with ITokenAcquisition
    /// </summary>
    public class AuthProvider : IAuthProvider
    {

        public AuthProvider(ITokenAcquisition tokenAcquisition, IHttpContextAccessor httpContextAccessor)
        {
            this.TokenAcquisition = tokenAcquisition;
            this.HttpContextAccessor = httpContextAccessor;
        }

        public ITokenAcquisition TokenAcquisition { get; }
        public IHttpContextAccessor HttpContextAccessor { get; }

        public async Task<string> GetAccessTokenForAppAsync()
        {
            var token = await this.TokenAcquisition.GetAccessTokenForAppAsync(new string[] { "https://graph.microsoft.com/.default" });
            return token;
        }
            

        public async Task<string> GetAccessTokenForUserAsync(IEnumerable<string> scopes)
        {
            var userClaims = this.HttpContextAccessor.HttpContext.User;
            var token = await this.TokenAcquisition.GetAccessTokenForUserAsync(scopes, user: userClaims);
            return token;

        }
            
    }
}
