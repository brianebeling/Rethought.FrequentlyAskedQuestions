using System.Threading.Tasks;
using Discord.WebSocket;
using Google.Cloud.Dialogflow.V2;

namespace Rethought.FrequentlyAskedQuestions.Strategies
{
    public interface IUserQuestionReceivedStrategy
    {
        Task ExecuteAsync(SocketUserMessage socketUserMessage, DetectIntentResponse intent);
    }
}