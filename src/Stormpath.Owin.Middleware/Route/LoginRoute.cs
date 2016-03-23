﻿// <copyright file="LoginRoute.cs" company="Stormpath, Inc.">
// Copyright (c) 2016 Stormpath, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.Configuration.Abstractions;
using Stormpath.Owin.Common;
using Stormpath.Owin.Common.ViewModel;
using Stormpath.Owin.Common.ViewModelBuilder;
using Stormpath.Owin.Middleware.Internal;
using Stormpath.Owin.Middleware.Model;
using Stormpath.Owin.Middleware.Model.Error;
using Stormpath.Owin.Middleware.Owin;
using Stormpath.SDK.Account;
using Stormpath.SDK.Client;
using Stormpath.SDK.Error;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Oauth;

namespace Stormpath.Owin.Middleware.Route
{
    public class LoginRoute : AbstractRouteMiddleware
    {
        private readonly static string[] SupportedMethods = { "GET", "POST" };
        private readonly static string[] SupportedContentTypes = { "text/html", "application/json" };

        public LoginRoute(
            StormpathConfiguration configuration,
            ILogger logger,
            IClient client)
            : base(configuration, logger, client, SupportedMethods, SupportedContentTypes)
        {
        }

        protected override Task GetHtml(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString);

            var viewModelBuilder = new ExtendedLoginViewModelBuilder(_configuration.Web, queryString, null);
            var loginViewModel = viewModelBuilder.Build();

            return RenderForm(context, loginViewModel, cancellationToken);
        }

        private Task RenderForm(IOwinEnvironment context, ExtendedLoginViewModel viewModel, CancellationToken cancellationToken)
        {
            context.Response.Headers.SetString("Content-Type", Constants.HtmlContentType);

            var loginView = new Common.View.Login();
            return HttpResponse.Ok(loginView, viewModel, context);
        }

        private async Task<IOauthGrantAuthenticationResult> HandleLogin(IClient client, string login, string password, CancellationToken cancellationToken)
        {
            var application = await client.GetApplicationAsync(_configuration.Application.Href, cancellationToken);

            var passwordGrantRequest = OauthRequests.NewPasswordGrantRequest()
                .SetLogin(login)
                .SetPassword(password)
                .Build();

            var passwordGrantAuthenticator = application.NewPasswordGrantAuthenticator();

            var grantResult = await passwordGrantAuthenticator
                .AuthenticateAsync(passwordGrantRequest, cancellationToken);

            return grantResult;
        }

        protected override async Task PostHtml(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var queryString = QueryStringParser.Parse(context.Request.QueryString);

            var requestBody = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var formData = FormContentParser.Parse(requestBody);

            var login = formData.GetString("login");
            var password = formData.GetString("password");

            bool missingLoginOrPassword = string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password);
            if (missingLoginOrPassword)
            {
                var viewModelBuilder = new ExtendedLoginViewModelBuilder(_configuration.Web, queryString, formData);
                var loginViewModel = viewModelBuilder.Build();
                loginViewModel.FormErrors.Add("The login and password fields are required.");

                await RenderForm(context, loginViewModel, cancellationToken);
                return;
            }

            try
            {
                var grantResult = await HandleLogin(client, login, password, cancellationToken);

                Cookies.AddToResponse(context, client, grantResult, _configuration);
            }
            catch (ResourceException rex)
            {
                var viewModelBuilder = new ExtendedLoginViewModelBuilder(_configuration.Web, queryString, formData);
                var loginViewModel = viewModelBuilder.Build();
                loginViewModel.FormErrors.Add(rex.Message);

                await RenderForm(context, loginViewModel, cancellationToken);
                return;
            }

            var nextUri = _configuration.Web.Login.NextUri;

            var nextUriFromQueryString = queryString["next"]?.FirstOrDefault();
            if (!string.IsNullOrEmpty(nextUriFromQueryString))
            {
                nextUri = nextUriFromQueryString;
            }

            await HttpResponse.Redirect(context, nextUri);
            return;
        }

        protected override Task GetJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var viewModelBuilder = new LoginViewModelBuilder(_configuration.Web.Login);
            var loginViewModel = viewModelBuilder.Build();

            return JsonResponse.Ok(context, loginViewModel);
        }

        protected override async Task PostJson(IOwinEnvironment context, IClient client, CancellationToken cancellationToken)
        {
            var bodyString = await context.Request.GetBodyAsStringAsync(cancellationToken);
            var body = Serializer.Deserialize<LoginPostModel>(bodyString);
            var login = body?.Login;
            var password = body?.Password;

            bool missingLoginOrPassword = string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password);
            if (missingLoginOrPassword)
            {
                await Error.Create(context, new BadRequest("Missing login or password."), cancellationToken);
                return;
            }

            var grantResult = await HandleLogin(client, login, password, cancellationToken);
            // Errors will be caught up in AbstractRouteMiddleware

            Cookies.AddToResponse(context, client, grantResult, _configuration);

            var token = await grantResult.GetAccessTokenAsync(cancellationToken);
            var account = await token.GetAccountAsync(cancellationToken);

            var sanitizer = new ResponseSanitizer<IAccount>();
            var responseModel = new
            {
                account = sanitizer.Sanitize(account)
            };

            await JsonResponse.Ok(context, responseModel);
            return;
        }
    }
}
