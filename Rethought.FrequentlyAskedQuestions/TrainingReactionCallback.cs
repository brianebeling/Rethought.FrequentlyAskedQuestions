using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Google.Cloud.Dialogflow.V2;

namespace Rethought.FrequentlyAskedQuestions
{
    public class TrainingReactionCallback : IReactionCallback
    {
        private readonly RestUserMessage botMessage;
        private readonly DetectIntentResponse detectIntentResponse;
        private readonly IntentService intentService;
        private readonly InteractiveService interactiveService;
        private readonly string trainingPhrase;

        public TrainingReactionCallback(string trainingPhrase, DetectIntentResponse detectIntentResponse,
            IntentService intentService, SocketCommandContext context, TimeSpan? timeout,
            ICriterion<SocketReaction> criterion, RunMode runMode, InteractiveService interactiveService,
            RestUserMessage botMessage)
        {
            this.trainingPhrase = trainingPhrase;
            this.detectIntentResponse = detectIntentResponse;
            this.intentService = intentService;
            Context = context;
            Timeout = timeout;
            Criterion = criterion;
            RunMode = runMode;
            this.interactiveService = interactiveService;
            this.botMessage = botMessage;
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            await botMessage.RemoveAllReactionsAsync();

            await intentService.TrainIntentAsync(detectIntentResponse.QueryResult.Intent.Name,
                new List<Intent.Types.TrainingPhrase>
                {
                    new Intent.Types.TrainingPhrase
                        {Parts = {new Intent.Types.TrainingPhrase.Types.Part {Text = trainingPhrase}}}
                }, new List<Intent.Types.Message>(), CancellationToken.None);


            await interactiveService.ReplyAndDeleteAsync(Context, "Thanks! I've learned something.",
                timeout: TimeSpan.FromMinutes(2));

            return true;
        }

        public RunMode RunMode { get; }
        public ICriterion<SocketReaction> Criterion { get; }
        public TimeSpan? Timeout { get; }
        public SocketCommandContext Context { get; }
    }
}