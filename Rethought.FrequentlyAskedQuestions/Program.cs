using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Dialogflow.V2;
using Grpc.Auth;
using Microsoft.Extensions.Configuration;


namespace Rethought.FrequentlyAskedQuestions
{
    internal class Program
    {
        private static async Task Main()
        {
            var configurationReader = CreateConfigurationReader();

            var discordSocketClient = CreateDiscordSocketClient();

            var dialogflowConfiguration = configurationReader.GetDialogflowConfiguration();
            var googleCredential = CreateGoogleCredential(dialogflowConfiguration);
            var channel = new Grpc.Core.Channel(SessionsClient.DefaultEndpoint.Host, googleCredential.ToChannelCredentials());

            var sessionsClient = SessionsClient.Create(channel);
            
            var messageHandler = new MessageHandler(sessionsClient, dialogflowConfiguration);

            discordSocketClient.Log += Log;

            await discordSocketClient.LoginAsync(TokenType.Bot, configurationReader.GetDiscordAuthentication().Token);
            await discordSocketClient.StartAsync();

            discordSocketClient.Ready += async () =>
            {
                await discordSocketClient.SetActivityAsync(new Game("your questions", ActivityType.Listening));

                discordSocketClient.MessageReceived += async message => { await messageHandler.HandleAsync(message); };
            };

            await Task.Delay(-1);
        }

        private static GoogleCredential CreateGoogleCredential(DialogflowConfiguration dialogflowConfiguration)
        {
            return GoogleCredential.FromFile(Directory.GetCurrentDirectory() + "\\" + dialogflowConfiguration.AuthFileName);
        }

        private static DiscordSocketClient CreateDiscordSocketClient()
        {
            var discordSocketClient = new DiscordSocketClient(
                new DiscordSocketConfig
                {
                    AlwaysDownloadUsers = true
                });
            return discordSocketClient;
        }

        private static ConfigurationReader CreateConfigurationReader()
        {
            var configurationRoot = CreateConfigurationRoot();

            var configurationReader = new ConfigurationReader(configurationRoot);
            return configurationReader;
        }

        private static IConfigurationRoot CreateConfigurationRoot()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, false)
                .Build();
            return configuration;
        }

        private static Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
    }
}
