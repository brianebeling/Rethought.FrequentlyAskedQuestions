using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Google.Cloud.Dialogflow.V2;
using MoreLinq;

namespace Rethought.FrequentlyAskedQuestions
{
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

            var fulfillmentMessage = response.QueryResult.FulfillmentMessages.RandomSubset(1);

            if (!fulfillmentMessage.Any())
            {
                return;
            }

            if (response.QueryResult.IntentDetectionConfidence <= 0.85)
            {
                await socketMessage.Channel.SendMessageAsync($"I'm not quite sure if I understood that correctly." + Environment.NewLine + fulfillmentMessage + Environment.NewLine + Format.Italics("React to the message to let me know if I was correct."));
            }

            await socketMessage.Channel.SendMessageAsync();
        }
    }
}