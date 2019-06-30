using System.Threading.Tasks;
using Discord.WebSocket;

namespace Rethought.FrequentlyAskedQuestions.Strategies
{
    public interface IMessageReceivedStrategy
    {
        Task ExecuteAsync(SocketMessage socketMessage);
    }
}