using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Google.Cloud.Dialogflow.V2;

namespace Rethought.FrequentlyAskedQuestions
{
    public class IntentService
    {
        private readonly DialogflowConfiguration dialogflowConfiguration;
        private readonly IntentsClient intentsClient;

        public IntentService(IntentsClient intentsClient, DialogflowConfiguration dialogflowConfiguration)
        {
            this.intentsClient = intentsClient;
            this.dialogflowConfiguration = dialogflowConfiguration;
        }

        public async Task AddIntentAsync(
            string displayName,
            IReadOnlyList<Intent.Types.TrainingPhrase> trainingPhrases,
            IReadOnlyList<Intent.Types.Message> fulfillmentResponses,
            CancellationToken cancellationToken)
        {
            var createIntentRequest = new CreateIntentRequest
            {
                Parent = $"projects/{dialogflowConfiguration.ProjectId}/agent",
                Intent = new Intent
                {
                    DisplayName = displayName
                }
            };

            createIntentRequest.Intent.TrainingPhrases.AddRange(trainingPhrases);
            createIntentRequest.Intent.Messages.AddRange(fulfillmentResponses);

            await intentsClient.CreateIntentAsync(createIntentRequest, cancellationToken);
        }

        public async Task TrainIntentAsync(
            string intentId,
            IReadOnlyList<Intent.Types.TrainingPhrase> trainingPhrases,
            IReadOnlyList<Intent.Types.Message> fulfillmentResponses,
            CancellationToken cancellationToken)
        {
            var getIntentRequest = new GetIntentRequest
            {
                Name = intentId,
                IntentView = IntentView.Full
            };

            var intent = await intentsClient.GetIntentAsync(getIntentRequest, cancellationToken);

            intent.TrainingPhrases.AddRange(trainingPhrases);
            intent.Messages.AddRange(fulfillmentResponses);

            var updateIntentRequest = new UpdateIntentRequest
            {
                Intent = intent
            };

            await intentsClient.UpdateIntentAsync(updateIntentRequest, cancellationToken);
        }

        public async Task ShowAllIntents(SocketUserMessage socketUserMessage)
        {
            var embedBuilder = new EmbedBuilder();

            embedBuilder.WithTitle("All Questions");

            var description = string.Empty;

            foreach (var listIntent in intentsClient.ListIntents(
                new ListIntentsRequest
                {
                    Parent = dialogflowConfiguration.ProjectIdView
                }))
            {
                if (listIntent.IsFallback) continue;

                description += $"{listIntent.DisplayName}{Environment.NewLine}";
            }

            embedBuilder.WithDescription(description);

            await socketUserMessage.Channel.SendMessageAsync(embed: embedBuilder.Build());
        }
    }
}