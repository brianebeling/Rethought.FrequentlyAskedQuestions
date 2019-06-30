using System.Threading.Tasks;
using Discord.WebSocket;
using Google.Cloud.Dialogflow.V2;

namespace Rethought.FrequentlyAskedQuestions.Strategies
{
    public class ShowIntentsStrategy : IUserQuestionReceivedStrategy
    {
        private readonly ConversationService conversationService;
        private readonly IntentService intentService;
        private readonly PrebuiltIntents prebuiltIntents;

        public ShowIntentsStrategy(PrebuiltIntents prebuiltIntents, ConversationService conversationService,
            IntentService intentService)
        {
            this.prebuiltIntents = prebuiltIntents;
            this.conversationService = conversationService;
            this.intentService = intentService;
        }

        public async Task ExecuteAsync(SocketUserMessage socketUserMessage, DetectIntentResponse intent)
        {
            if (intent.QueryResult.Intent.Name != prebuiltIntents.ShowIntentName) return;

            if (conversationService.Conversations.ContainsKey(socketUserMessage.Author.Id)) return;

            await intentService.ShowAllIntents(socketUserMessage);
        }
    }
}