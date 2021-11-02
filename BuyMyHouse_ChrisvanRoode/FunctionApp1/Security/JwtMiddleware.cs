using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Functions;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Services;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Kennemerland.Startup {
    public class JwtMiddleware : IFunctionsWorkerMiddleware {
        ILogger Logger { get; }

        public JwtMiddleware(ILogger<JwtMiddleware> Logger) {
            this.Logger = Logger;
        }

        public async Task Invoke(FunctionContext Context, FunctionExecutionDelegate Next) {
            string HeadersString = (string)Context.BindingContext.BindingData["Headers"];

            Dictionary<string, string> Headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(HeadersString);

            if (Headers.TryGetValue("Authorization", out string AuthorizationHeader)) {
                try {
                    AuthenticationHeaderValue BearerHeader = AuthenticationHeaderValue.Parse(AuthorizationHeader);

               }
                catch (Exception e) {
                    Logger.LogError(e.Message);
                }
            }

            await Next(Context);
        }

    }
}


