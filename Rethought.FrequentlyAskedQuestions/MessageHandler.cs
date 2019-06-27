using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.WebSocket;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf.WellKnownTypes;
using MoreLinq;

namespace Rethought.FrequentlyAskedQuestions
{
    public class MessageHandler
    {
        private readonly SessionsClient sessionsClient;
        private readonly DialogflowConfiguration dialogflowConfiguration;
        private readonly InteractiveService interactiveService;

        public MessageHandler(SessionsClient sessionsClient, DialogflowConfiguration dialogflowConfiguration, InteractiveService interactiveService)
        {
            this.sessionsClient = sessionsClient;
            this.dialogflowConfiguration = dialogflowConfiguration;
            this.interactiveService = interactiveService;
        }

        public async Task HandleAsync(SocketMessage socketMessage)
        {
            if (socketMessage.Source != MessageSource.User) return;

            var splitMessageContent = socketMessage.Content.Split(string.Empty);

            if (socketMessage.MentionedRoles.Any(mentionedRole => splitMessageContent.First() == mentionedRole.Mention))
            {
                return;
            }

            var sanitizedMessage = Format.Sanitize(socketMessage.Content);
            
            var query = new QueryInput()
            {
                Text = new TextInput()
                {
                    Text = sanitizedMessage,
                    LanguageCode = "en-us"
                }
            };

            var sessionId = Guid.NewGuid().ToString();

            var response = await sessionsClient.DetectIntentAsync(new SessionName(dialogflowConfiguration.ProjectId, sessionId), query);

            if (response.QueryResult.Intent.IsFallback)
            {
                return;
            }

            if (response.QueryResult.IntentDetectionConfidence <= 0.7)
            {
                return;
            }

            var fulfillmentMessage = response.QueryResult.FulfillmentMessages.RandomSubset(1).FirstOrDefault();

            if (fulfillmentMessage == null || fulfillmentMessage.Equals(default))
            {
                return;
            }

            if (response.QueryResult.IntentDetectionConfidence <= 0.85)
            {
                await socketMessage.Channel.SendMessageAsync($"I'm not quite sure if I understood that correctly." + Environment.NewLine + fulfillmentMessage + Environment.NewLine + Format.Italics("React to the message to let me know if I was correct."));
            }
            else
            {
                await socketMessage.Channel.SendMessageAsync(fulfillmentMessage.ToString());
            }
        }
    }

    public class IntentService
    {
        private readonly IntentsClient intentClient;
        private readonly DialogflowConfiguration dialogflowConfiguration;

        public IntentService(IntentsClient intentClient, DialogflowConfiguration dialogflowConfiguration)
        {
            this.intentClient = intentClient;
            this.dialogflowConfiguration = dialogflowConfiguration;
        }
        public async Task AddIntentAsync(
            string displayName, 
            IReadOnlyList<Intent.Types.TrainingPhrase> trainingPhrases, 
            IReadOnlyList<Intent.Types.Message> fulfillmentResponses,
            CancellationToken cancellationToken)
        {
            var createIntentRequest = new CreateIntentRequest()
            {
                Parent = dialogflowConfiguration.ProjectId,
                Intent = new Intent()
                {
                    DisplayName = displayName
                }
            };

            createIntentRequest.Intent.TrainingPhrases.AddRange(trainingPhrases);
            createIntentRequest.Intent.Messages.AddRange(fulfillmentResponses);
            
            await intentClient.CreateIntentAsync(createIntentRequest, cancellationToken);
        }
    }
}