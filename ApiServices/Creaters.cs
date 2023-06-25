using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.InvestApi;

namespace TinkoffPriceMonitor.ApiServices
{
    public class Creaters
    {
        public static Task<InvestApiClient> CreateClientAsync()
        {
            string token = "t.xyqIxVciDDS4TtnLukX44Mf-PjiPIl8Wx_lk4xsaoCA23eyVbaSlMS5WZTJUeidfBrrqyE458_nxk9KuFb3S-A";
            //var token = File.ReadAllText("token.txt");
            //if (string.IsNullOrEmpty(token))
            //{
            //    return null;
            //}

            var appName = "tinkoff.invest-api-csharp-sdk";
            var callCredentials = CallCredentials.FromInterceptor((_, metadata) =>
            {
                metadata.Add("Authorization", "Bearer " + token);
                metadata.Add("x-app-name", appName);
                return Task.CompletedTask;
            });

            var methodConfig = new MethodConfig
            {
                Names =
        {
            MethodName.Default,
        },
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = 5,
                    InitialBackoff = TimeSpan.FromSeconds(1.0),
                    MaxBackoff = TimeSpan.FromSeconds(5.0),
                    BackoffMultiplier = 1.5,
                    RetryableStatusCodes =
            {
                StatusCode.Unavailable,
            },
                },
            };

            var channelOptions = new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), callCredentials),
                ServiceConfig = new ServiceConfig
                {
                    MethodConfigs =
            {
                methodConfig,
            },
                },
            };

            var channel = GrpcChannel.ForAddress("https://invest-public-api.tinkoff.ru:443", channelOptions);
            var invoker = channel.CreateCallInvoker();
            var client = new InvestApiClient(invoker);

            return Task.FromResult(client);
        }

    }
}
