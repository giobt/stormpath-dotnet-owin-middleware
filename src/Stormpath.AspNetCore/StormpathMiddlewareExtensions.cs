﻿// <copyright file="StormpathMiddlewareExtensions.cs" company="Stormpath, Inc.">
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
using Microsoft.AspNet.Builder;

namespace Stormpath.AspNetCore
{
    public static class StormpathMiddlewareExtensions
    {
        /// <summary>
        /// Adds the Stormpath middleware to the pipeline with the given options.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseStormpath(this IApplicationBuilder app, object configuration)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            // todo construct framework user-agent string

            // todo build configuration

            // todo attempt to connect and get configuration from server

            // todo build final configuration

            return app.UseMiddleware<StormpathMiddleware>(compiledConfiguration);
        }
    }
}
