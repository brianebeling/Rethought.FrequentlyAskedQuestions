using System;
using System.IO;
using System.Linq;
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
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, false)
                .Build();
            
            var configurationReader = new ConfigurationReader(configuration);

            var discordSocketClient = new DiscordSocketClient(
                new DiscordSocketConfig
                {
                    AlwaysDownloadUsers = true
                });

            var dialogflowConfiguration = configurationReader.GetDialogflowConfiguration();
            var creds = GoogleCredential.FromFile(Directory.GetCurrentDirectory() + "\\" + dialogflowConfiguration.AuthFileName);
            var channel = new Grpc.Core.Channel(SessionsClient.DefaultEndpoint.Host, creds.ToChannelCredentials());

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

        private static Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
    }

    public class MessageHandler
    {
        private readonly SessionsClient sessionsClient;
        private readonly DialogflowConfiguration dialogflowConfiguration;

        public MessageHandler(SessionsClient sessionsClient, DialogflowConfiguration dialogflowConfiguration)
        {
            this.sessionsClient = sessionsClient;
            this.dialogflowConfiguration = dialogflowConfiguration;
        }

        public async Task HandleAsync(SocketMessage socketMessage)
        {
            if (socketMessage.Source != MessageSource.User) return;

            var splitMessageContent = socketMessage.Content.Split(string.Empty);

            if (socketMessage.MentionedRoles.Any(mentionedRole => splitMessageContent.First() == mentionedRole.Mention))
            {
                return;
            }

            var query = new QueryInput()
            {
                Text = new TextInput()
                {
                    Text = socketMessage.Content,
                    LanguageCode = "en-us"
                }
            };

            var sessionId = Guid.NewGuid().ToString();

            var response = await sessionsClient.DetectIntentAsync(new SessionName(dialogflowConfiguration.ProjectId, sessionId), query);
            
            await socketMessage.Channel.SendMessageAsync($"Detected the intent (Confidence: {response.QueryResult.IntentDetectionConfidence}): " + response.QueryResult.Intent.DisplayName);
            
        }
    }

    public class ConfigurationReader
    {
        private readonly IConfigurationRoot configurationRoot;

        public ConfigurationReader(IConfigurationRoot configurationRoot)
        {
            this.configurationRoot = configurationRoot;
        }

        public DiscordAuthentication GetDiscordAuthentication()
        {
            var discordAuthentication = new DiscordAuthentication();
            this.configurationRoot.GetSection("Authentication").GetSection("Discord").Bind(discordAuthentication);

            return discordAuthentication;
        }

        public DialogflowConfiguration GetDialogflowConfiguration()
        {
            var dialogflowConfiguration = new DialogflowConfiguration();
            this.configurationRoot.GetSection("Authentication").GetSection("Dialogflow").Bind(dialogflowConfiguration);
            this.configurationRoot.GetSection("Dialogflow").Bind(dialogflowConfiguration);

            return dialogflowConfiguration;

        }
    }

    public class DialogflowConfiguration
    {
        public string AuthFileName { get; set; }

        public string ProjectId { get; set; }
    }

    public class DiscordAuthentication
    {
        public string Token { get; set; }
    }
}
