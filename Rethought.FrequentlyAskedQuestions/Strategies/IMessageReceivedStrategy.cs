using System.Threading.Tasks;
using Discord.WebSocket;

namespace Rethought.FrequentlyAskedQuestions
{
    public interface IMessageReceivedStrategy
    {
        Task ExecuteAsync(SocketMessage socketMessage);
    }
}