using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Google.Cloud.Dialogflow.V2;

namespace Rethought.FrequentlyAskedQuestions
{
    public class AddNewIntentStrategy : IUserQuestionReceivedStrategy
    {
        private readonly ConversationService conversationService;
        private readonly PrebuiltIntents prebuiltIntents;

        public AddNewIntentStrategy(PrebuiltIntents prebuiltIntents, ConversationService conversationService)
        {
            this.prebuiltIntents = prebuiltIntents;
            this.conversationService = conversationService;
        }

        public async Task ExecuteAsync(SocketUserMessage socketUserMessage, DetectIntentResponse intent)
        {
            if (intent.QueryResult.Intent.Name != prebuiltIntents.AddIntentName) return;

            if (conversationService.Conversations.ContainsKey(socketUserMessage.Author.Id)) return;

            if (socketUserMessage.Channel is SocketGuildChannel socketGuildChannel)
            {
                var guildUser = socketGuildChannel.GetUser(socketUserMessage.Author.Id);

                if (!guildUser.GetPermissions(socketGuildChannel).ManageMessages)
                {
                    return;
                }
            }

            await conversationService.StartAdditionConversationAsync(socketUserMessage, CancellationToken.None);
        }
    }

    public class CorrectIntentStrategy : IUserQuestionReceivedStrategy
    {
        private readonly ConversationService conversationService;
        private readonly PrebuiltIntents prebuiltIntents;

        public CorrectIntentStrategy(PrebuiltIntents prebuiltIntents, ConversationService conversationService)
        {
            this.prebuiltIntents = prebuiltIntents;
            this.conversationService = conversationService;
        }

        public async Task ExecuteAsync(SocketUserMessage socketUserMessage, DetectIntentResponse intent)
        {
            if (intent.QueryResult.Intent.Name != prebuiltIntents.CorrectIntentName) return;

            if (conversationService.Conversations.ContainsKey(socketUserMessage.Author.Id)) return;

            if (socketUserMessage.Channel is SocketGuildChannel socketGuildChannel)
            {
                var guildUser = socketGuildChannel.GetUser(socketUserMessage.Author.Id);

                if (!guildUser.GetPermissions(socketGuildChannel).ManageMessages)
                {
                    return;
                }
            }

            await conversationService.StartCorrectionConversationAsync(socketUserMessage, CancellationToken.None);
        }
    }

    
}