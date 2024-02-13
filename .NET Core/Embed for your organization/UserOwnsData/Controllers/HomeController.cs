﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// ----------------------------------------------------------------------------

namespace UserOwnsData.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Identity.Web;
    using Microsoft.Graph;
    using System.Threading.Tasks;
    using UserOwnsData.Models;
    using Microsoft.Extensions.DependencyInjection;
    using System.Collections.Generic;

    [Authorize]
    public class HomeController : Controller
    {
        private readonly GraphServiceClient m_graphServiceClient;

        private readonly ITokenAcquisition m_tokenAcquisition;

        public IConfiguration Configuration { get; }

        public HomeController(ITokenAcquisition tokenAcquisition,
                              GraphServiceClient graphServiceClient,
                              IConfiguration configuration)
        {
            this.m_tokenAcquisition = tokenAcquisition;
            this.m_graphServiceClient = graphServiceClient;
            Configuration = configuration;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        // Redirects to login page to request increment consent
        [AuthorizeForScopes(ScopeKeySection = "AzureAd:Scopes")]
        public async Task<IActionResult> Embed()
        {
            // Generate token for the signed in user
            var m_tokenAcquisition = this.HttpContext.RequestServices
                .GetRequiredService<ITokenAcquisition>();
            string scopes = Configuration["AzureAd:Scopes"];
            string tenantId = Configuration["AzureAd:TenantId"];

            var accessToken = await m_tokenAcquisition.GetAccessTokenForUserAsync(scopes.Split(' '), tenantId);

            // Get username of logged in user
            var userInfo = await m_graphServiceClient.Me.Request().GetAsync();
            var userName = userInfo.DisplayName;

            AuthDetails authDetails = new AuthDetails
            {
                UserName = userName,
                AccessToken = accessToken
            };

            return View(authDetails);
        }
    }
}
