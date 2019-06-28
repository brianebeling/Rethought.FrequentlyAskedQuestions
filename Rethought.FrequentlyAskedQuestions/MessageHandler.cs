using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Rethought.FrequentlyAskedQuestions
{
    public class MessageHandler
    {
        private readonly IList<IMessageReceivedStrategy> messageReceivedStrategies;

        public MessageHandler(IList<IMessageReceivedStrategy> messageReceivedStrategies)
        {
            this.messageReceivedStrategies = messageReceivedStrategies;
        }

        public Task HandleAsync(SocketMessage socketMessage)
        {
            foreach (var messageReceivedStrategy in messageReceivedStrategies)
            {
#pragma warning disable 4014
                Task.Run(() => messageReceivedStrategy.ExecuteAsync(socketMessage));
#pragma warning restore 4014
            }

            return Task.CompletedTask;
        }
    }
}