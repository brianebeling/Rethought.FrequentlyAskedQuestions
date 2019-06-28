using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Google.Cloud.Dialogflow.V2;

namespace Rethought.FrequentlyAskedQuestions
{
    public class UserQuestionReceivedStrategy : IMessageReceivedStrategy
    {
        private readonly DialogflowConfiguration dialogflowConfiguration;
        private readonly DiscordSocketClient discordSocketClient;
        private readonly SessionsClient sessionsClient;
        private readonly IList<IUserQuestionReceivedStrategy> userQuestionReceivedStrategies;

        public UserQuestionReceivedStrategy(
            DiscordSocketClient discordSocketClient,
            IList<IUserQuestionReceivedStrategy> userQuestionReceivedStrategies,
            SessionsClient sessionsClient,
            DialogflowConfiguration dialogflowConfiguration)
        {
            this.discordSocketClient = discordSocketClient;
            this.userQuestionReceivedStrategies = userQuestionReceivedStrategies;
            this.sessionsClient = sessionsClient;
            this.dialogflowConfiguration = dialogflowConfiguration;
        }

        public async Task ExecuteAsync(SocketMessage socketMessage)
        {
            if (socketMessage.Source != MessageSource.User) return;

            if (socketMessage is SocketUserMessage socketUserMessage)
            {
                //if (!socketMessage.Content.EndsWith('?')) return;

                if (socketUserMessage.MentionedUsers.Count > 0
                    && socketUserMessage.MentionedUsers.Any(user => user.Id != discordSocketClient.CurrentUser.Id))
                    return;

                var sanitizedMessage = Format.Sanitize(socketMessage.Content);

                var query = new QueryInput
                {
                    Text = new TextInput
                    {
                        Text = sanitizedMessage,
                        LanguageCode = "en-us"
                    }
                };

                var sessionId = Guid.NewGuid().ToString();

                var intent =
                    await sessionsClient.DetectIntentAsync(
                        new SessionName(dialogflowConfiguration.ProjectId, sessionId),
                        query);

                foreach (var userQuestionReceivedStrategy in userQuestionReceivedStrategies)
                    await userQuestionReceivedStrategy.ExecuteAsync(socketUserMessage, intent);
            }
        }
    }
}