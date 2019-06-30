using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Google.Cloud.Dialogflow.V2;
using MoreLinq;
using Newtonsoft.Json;

namespace Rethought.FrequentlyAskedQuestions.Strategies
{
    public class RespondToIntentStrategy : IUserQuestionReceivedStrategy
    {
        private readonly ConversationService conversationService;
        private readonly IntentService intentService;
        private readonly InteractiveService interactiveService;
        private readonly SocketCommandContextFactory socketCommandContextFactory;

        public RespondToIntentStrategy(InteractiveService interactiveService, IntentService intentService,
            SocketCommandContextFactory socketCommandContextFactory, ConversationService conversationService)
        {
            this.interactiveService = interactiveService;
            this.intentService = intentService;
            this.socketCommandContextFactory = socketCommandContextFactory;
            this.conversationService = conversationService;
        }

        public async Task ExecuteAsync(SocketUserMessage socketUserMessage, DetectIntentResponse intent)
        {
            if (conversationService.Conversations.ContainsKey(socketUserMessage.Author.Id)) return;

            if (intent.QueryResult.IntentDetectionConfidence <= 0.5) return;

            var fulfillmentMessage = intent.QueryResult.FulfillmentMessages.RandomSubset(1).FirstOrDefault();

            if (fulfillmentMessage == null) return;

            var textResponse =
                JsonConvert.DeserializeObject<Response>(fulfillmentMessage.Text.ToString());

            var embedBuilder = new EmbedBuilder();

            embedBuilder.WithTitle(intent.QueryResult.QueryText);
            embedBuilder.WithDescription(textResponse.Text.First());
            embedBuilder.WithFooter(builder =>
                builder.WithText($"Confidence: {intent.QueryResult.IntentDetectionConfidence}"));

            var message = await socketUserMessage.Channel.SendMessageAsync(embed: embedBuilder.Build());

            if (intent.QueryResult.Intent.IsFallback) return;

            await message.AddReactionsAsync(new IEmote[]
            {
                new Emoji("✅")
            });

            interactiveService.AddReactionCallback(message,
                new TrainingReactionCallback(intent.QueryResult.QueryText, intent, intentService,
                    socketCommandContextFactory.Create(socketUserMessage),
                    TimeSpan.FromMinutes(3),
                    new ReactionAmountCriterion(), RunMode.Async, interactiveService, message));
        }
    }
}