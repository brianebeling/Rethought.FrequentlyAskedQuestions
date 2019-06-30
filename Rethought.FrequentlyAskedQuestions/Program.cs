using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.WebSocket;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Dialogflow.V2;
using Grpc.Auth;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Rethought.FrequentlyAskedQuestions.Strategies;

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
            var channel = new Channel(SessionsClient.DefaultEndpoint.Host, googleCredential.ToChannelCredentials());

            var sessionsClient = SessionsClient.Create(channel);

            var interactiveService = new InteractiveService(discordSocketClient, TimeSpan.FromMinutes(3));

            var intentsClient = IntentsClient.Create(channel);
            var intentService = new IntentService(intentsClient, dialogflowConfiguration);

            var prebuiltIntents =
                new PrebuiltIntents(
                    "projects/frequentlyaskedquestionsdemo-m/agent/intents/978d997d-b25d-4f52-ac86-716438fa50bb",
                    "projects/frequentlyaskedquestionsdemo-m/agent/intents/94e6b3e7-2f12-4cdf-a79d-7b366f08a2fe",
                    "projects/frequentlyaskedquestionsdemo-m/agent/intents/6cd41a06-2f42-4b8d-aad2-35f4af286003");

            var socketCommandContextFactory = new SocketCommandContextFactory(discordSocketClient);

            var conversationService = new ConversationService(discordSocketClient, interactiveService, intentService);

            var messageHandler =
                new MessageHandler(
                    new List<IMessageReceivedStrategy>
                    {
                        new UserQuestionReceivedStrategy(
                            discordSocketClient,
                            new List<IUserQuestionReceivedStrategy>
                            {
                                new RespondToIntentStrategy(
                                    interactiveService,
                                    intentService,
                                    socketCommandContextFactory,
                                    conversationService),
                                new AddNewIntentStrategy(
                                    prebuiltIntents,
                                    conversationService),
                                new ShowIntentsStrategy(
                                    prebuiltIntents,
                                    conversationService,
                                    intentService),
                                new CorrectIntentStrategy(prebuiltIntents, conversationService)
                            },
                            sessionsClient,
                            dialogflowConfiguration)
                    });

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
            return GoogleCredential.FromFile(Directory.GetCurrentDirectory() + "\\" +
                                             dialogflowConfiguration.AuthFileName);
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