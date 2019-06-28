using Discord.Commands;
using Discord.WebSocket;

namespace Rethought.FrequentlyAskedQuestions
{
    public class SocketCommandContextFactory
    {
        private readonly DiscordSocketClient discordSocketClient;

        public SocketCommandContextFactory(DiscordSocketClient discordSocketClient)
        {
            this.discordSocketClient = discordSocketClient;
        }

        public SocketCommandContext Create(SocketUserMessage socketUserMessage)
        {
            return new SocketCommandContext(discordSocketClient, socketUserMessage);
        }
    }
}