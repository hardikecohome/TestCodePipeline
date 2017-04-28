﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Web.Common.Security;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.Practices.Unity;

namespace DealnetPortal.Web.Infrastructure
{
    public class TransientHttpApiClient : ITransientHttpApiClient
    {
        private readonly IHttpApiClient _authorizedHttpClient;
        private readonly IHttpApiClient _anonymousHttpClient;
        public TransientHttpApiClient(IHttpApiClient authorizedHttpClient, IHttpApiClient anonymousClient)
        {
            _authorizedHttpClient = authorizedHttpClient;
            _anonymousHttpClient = anonymousClient;
        }

        public IHttpApiClient Client
        {
            get
            {
                if (HttpContext.Current?.User?.Identity?.IsAuthenticated == true || _anonymousHttpClient == null)
                {
                    return _authorizedHttpClient;
                }
                var httpClient = _anonymousHttpClient;//new HttpApiClient(System.Configuration.ConfigurationManager.AppSettings["ApiUrl"]); //DependencyResolver.Current.GetService<ITransientHttpApiClient>();
                string cultureFromRoute = null;
                try
                {
                    cultureFromRoute = HttpContext.Current?.Request.RequestContext.RouteData.Values["culture"] as string;
                }
                catch (HttpException)
                {
                    //ignored - means context is not available at this point
                }
                if (cultureFromRoute != null)
                {
                    httpClient.Client.DefaultRequestHeaders.AcceptLanguage.Clear();
                    httpClient.Client.DefaultRequestHeaders.AcceptLanguage.Add(
                        new StringWithQualityHeaderValue(cultureFromRoute));
                }
                return httpClient;
            }
        }

        public Uri BaseAddress => _authorizedHttpClient.Client.BaseAddress;
    }
}
