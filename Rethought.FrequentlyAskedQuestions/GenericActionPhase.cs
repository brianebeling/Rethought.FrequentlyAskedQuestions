using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

namespace Rethought.FrequentlyAskedQuestions
{
    public class GenericActionPhase : IPhase
    {
        private readonly Action<FrequentlyAskedQuestionModel, string> action;
        private readonly DiscordSocketClient discordSocketClient;
        private readonly SocketUserMessage initialSocketUserMessage;
        private readonly InteractiveService interactiveService;
        private readonly string question;

        public GenericActionPhase(string question, SocketUserMessage initialSocketUserMessage,
            DiscordSocketClient discordSocketClient, InteractiveService interactiveService,
            Action<FrequentlyAskedQuestionModel, string> action)
        {
            this.question = question;
            this.initialSocketUserMessage = initialSocketUserMessage;
            this.discordSocketClient = discordSocketClient;
            this.interactiveService = interactiveService;
            this.action = action;
        }

        public async Task<FrequentlyAskedQuestionModel> CompleteAsync(
            FrequentlyAskedQuestionModel frequentlyAskedQuestionModel, CancellationToken cancellationToken)
        {
            await initialSocketUserMessage.Channel.SendMessageAsync(question);

            var message = await interactiveService.NextMessageAsync(
                new SocketCommandContext(discordSocketClient, initialSocketUserMessage), true, true,
                TimeSpan.FromMinutes(3));

            if (message == default(SocketMessage)) return frequentlyAskedQuestionModel;

            if (cancellationToken.IsCancellationRequested) return frequentlyAskedQuestionModel;

            action.Invoke(frequentlyAskedQuestionModel, message.Content);

            return frequentlyAskedQuestionModel;
        }
    }

    public class RepeatingGenericActionPhase : IPhase
    {
        private readonly Action<FrequentlyAskedQuestionModel, string> action;
        private readonly DiscordSocketClient discordSocketClient;
        private readonly SocketUserMessage initialSocketUserMessage;
        private readonly InteractiveService interactiveService;
        private readonly string question;

        public RepeatingGenericActionPhase(string question, SocketUserMessage initialSocketUserMessage,
            DiscordSocketClient discordSocketClient, InteractiveService interactiveService,
            Action<FrequentlyAskedQuestionModel, string> action)
        {
            this.question = question;
            this.initialSocketUserMessage = initialSocketUserMessage;
            this.discordSocketClient = discordSocketClient;
            this.interactiveService = interactiveService;
            this.action = action;
        }

        public async Task<FrequentlyAskedQuestionModel> CompleteAsync(
            FrequentlyAskedQuestionModel frequentlyAskedQuestionModel, CancellationToken cancellationToken)
        {
            await initialSocketUserMessage.Channel.SendMessageAsync(question);

            while (true)
            {
                await initialSocketUserMessage.Channel.SendMessageAsync("Alright, I've got this.");

                var message = await interactiveService.NextMessageAsync(
                    new SocketCommandContext(discordSocketClient,
                        initialSocketUserMessage),
                    true,
                    true,
                    TimeSpan.FromSeconds(45));

                if (message == default(SocketMessage))
                {
                    break;
                }

                if (cancellationToken.IsCancellationRequested) break;

                action.Invoke(frequentlyAskedQuestionModel, message.Content);
            }

            return frequentlyAskedQuestionModel;
        }
    }
}