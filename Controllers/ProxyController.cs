using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Mtg.Sample.Models;
using Newtonsoft.Json;

namespace Mtg.Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProxyController : ControllerBase
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IGraphServiceClient graphClient;
        private readonly ITokenAcquisition tokenAcquisition;
        private readonly IOptions<GraphOptions> graphOptions;

        public ProxyController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor,
            IGraphServiceClient graphSdkHelper, ITokenAcquisition tokenAcquisition, IOptions<GraphOptions>  graphOptions)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.graphClient = graphSdkHelper;
            this.tokenAcquisition = tokenAcquisition;
            this.graphOptions = graphOptions;
        }


        [HttpGet]
        public IActionResult SignIn()
        {
            var redirectUrl = Url.Page("/Index");
            return RedirectToPage(redirectUrl);

        }


        [HttpGet]
        [Route("{*all}")]
        public async Task<IActionResult> GetAsync(string all)
        {
            return await ProcessRequestAsync("GET", all, null).ConfigureAwait(false);
        }

        [HttpPost]
        [Route("{*all}")]
        public async Task<IActionResult> PostAsync(string all, [FromBody] object body)
        {
            return await ProcessRequestAsync("POST", all, body).ConfigureAwait(false);
        }

        [HttpDelete]
        [Route("{*all}")]
        public async Task<IActionResult> DeleteAsync(string all)
        {
            return await ProcessRequestAsync("DELETE", all, null).ConfigureAwait(false);
        }

        [HttpPut]
        [Route("{*all}")]
        public async Task<IActionResult> PutAsync(string all, [FromBody] object body)
        {
            return await ProcessRequestAsync("PUT", all, body).ConfigureAwait(false);
        }

        [HttpPatch]
        [Route("{*all}")]
        public async Task<IActionResult> PatchAsync(string all, [FromBody] object body)
        {
            return await ProcessRequestAsync("PATCH", all, body).ConfigureAwait(false);
        }

        private async Task<IActionResult> ProcessRequestAsync(string method, string all, object content)
        {
            var qs = HttpContext.Request.QueryString;
            var url = $"{GetBaseUrlWithoutVersion(graphClient)}/{all}{qs.ToUriComponent()}";

            var request = new BaseRequest(url, graphClient, null)
            {
                Method = method,
                ContentType = HttpContext.Request.ContentType,
            };

            var neededHeaders = Request.Headers.Where(h => h.Key.ToLower() == "if-match").ToList();
            if (neededHeaders.Count() > 0)
            {
                foreach (var header in neededHeaders)
                {
                    request.Headers.Add(new HeaderOption(header.Key, string.Join(",", header.Value)));
                }
            }

            var contentType = "application/json";

            try
            {
                using (var response = await request.SendRequestAsync(content?.ToString(), CancellationToken.None, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
                {
                    response.Content.Headers.TryGetValues("content-type", out var contentTypes);

                    contentType = contentTypes?.FirstOrDefault() ?? contentType;

                    var byteArrayContent = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                    return new FileContentResult(byteArrayContent, new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(contentType));
                }
            }
            catch (ServiceException ex)
            {

                if (ex.InnerException is MsalUiRequiredException)
                {

                    await tokenAcquisition.ReplyForbiddenWithWwwAuthenticateHeaderAsync(graphOptions.Value.GetScopes(), ex.InnerException as MsalUiRequiredException);
                    return Redirect(Url.Page("/Account/SignIn"));
                }

                return new JsonResult(ex.Error.ToString());
            }
        }
    
        private string GetBaseUrlWithoutVersion(IGraphServiceClient graphClient)
        {
            var baseUrl = graphClient.BaseUrl;
            var index = baseUrl.LastIndexOf('/');
            return baseUrl.Substring(0, index);
        }
    }
}
