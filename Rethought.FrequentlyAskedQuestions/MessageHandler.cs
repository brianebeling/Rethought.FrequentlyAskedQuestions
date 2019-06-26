using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Google.Cloud.Dialogflow.V2;

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
}