using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tinkoff.InvestApi;
using TinkoffPriceMonitor.Models;

namespace TinkoffPriceMonitor.ApiServices
{
    public class Creaters
    {
        public static Task<InvestApiClient> CreateClientAsync()
        {
            string? token = GetSettings().TinkoffToken;

            //if (string.IsNullOrEmpty(token)) return Task.CompletedTask;

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

        // Загружаю токен
        private static SettingsModel GetSettings()
        {
            string filePath = "settings.json";
            SettingsModel settingsModel = new SettingsModel();

            try
            {
                if (File.Exists(filePath))
                {
                    // Если файл существует, загрузите его содержимое
                    string jsonData = File.ReadAllText(filePath);
                    JObject data = JObject.Parse(jsonData);

                    // Пример загрузки данных из JSON в модель представления
                    settingsModel.TinkoffToken = data["TinkoffToken"]?.ToString();
                    settingsModel.ChromeLocation = data["ChromeLocation"]?.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }

            return settingsModel;
        }
    }
}
