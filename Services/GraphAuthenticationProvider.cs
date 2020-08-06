using Microsoft.Extensions.Options;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Mtg.Sample.Services
{
    public class GraphAuthenticationProvider : IAuthenticationProvider
    {
        private readonly IAuthProvider authProvider;
        private readonly IEnumerable<string> scopes;

        public GraphAuthenticationProvider(IAuthProvider authProvider, IEnumerable<string> scopes)
        {
            this.authProvider = authProvider;
            this.scopes = scopes;
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            var token = await authProvider.GetAccessTokenForUserAsync(this.scopes);
            request.Headers.Add("Authorization", $"Bearer {token}");
        }
    }
}
