using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;

namespace Mtg.Sample.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }


        public IActionResult OnGet()
        {
            //var userIsAuth = HttpContext.User.Identity.IsAuthenticated;
            //string accountIdentifier = HttpContext.User.GetMsalAccountId();

            //await HttpContext.Session.LoadAsync();

            //var account = HttpContext.Session.Get(accountIdentifier);

            //if (userIsAuth && accountIdentifier != null && account == null)
            //{
            //    var redirectUrl = Url.Page("/", null, Request.Scheme);

            //    return Challenge(
            //        new AuthenticationProperties { RedirectUri = redirectUrl },
            //        OpenIdConnectDefaults.AuthenticationScheme);
            //}

            return Page();

        }

        public void OnPost()
        {
        }
    }

}
