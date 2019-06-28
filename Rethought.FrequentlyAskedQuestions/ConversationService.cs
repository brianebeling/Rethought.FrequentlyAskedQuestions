using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.WebSocket;
using Google.Cloud.Dialogflow.V2;

namespace Rethought.FrequentlyAskedQuestions
{
    public class ConversationService
    {
        private readonly DiscordSocketClient discordSocketClient;
        private readonly IntentService intentService;
        private readonly InteractiveService interactiveService;

        public ConversationService(DiscordSocketClient discordSocketClient, InteractiveService interactiveService,
            IntentService intentService)
        {
            this.discordSocketClient = discordSocketClient;
            this.interactiveService = interactiveService;
            this.intentService = intentService;
        }

        public IDictionary<ulong, Conversation> Conversations { get; } = new Dictionary<ulong, Conversation>();

        public async Task StartAdditionConversationAsync(SocketUserMessage initialSocketUserMessage, CancellationToken cancellationToken)
        {
            try
            {
                var conversation = new Conversation(new List<IPhase>
                {
                    new GenericActionPhase("What is the question?", initialSocketUserMessage, discordSocketClient,
                        interactiveService, (model, s) =>
                        {
                            model.DisplayName = s;
                            model.TrainingPhrases.Add(s);
                        }),
                    new GenericActionPhase("Please phrase the question differently than before",
                        initialSocketUserMessage,
                        discordSocketClient, interactiveService, (model, s) => model.TrainingPhrases.Add(s)),
                    new GenericActionPhase(
                        "And for good measure just another one that is different than the ones before",
                        initialSocketUserMessage, discordSocketClient, interactiveService,
                        (model, s) => model.TrainingPhrases.Add(s)),
                    new GenericActionPhase("Last task, what would you like the answer to be?", initialSocketUserMessage,
                        discordSocketClient, interactiveService, (model, s) => model.Responses.Add(s))
                });

                Conversations.Add(initialSocketUserMessage.Author.Id, conversation);

                var frequentlyAskedQuestionModel = await conversation.StartAsync(cancellationToken);

                await intentService.AddIntentAsync(
                    frequentlyAskedQuestionModel.DisplayName,
                    frequentlyAskedQuestionModel
                        .TrainingPhrases.Select(
                            s =>
                            {
                                var trainingPhrase = new Intent.Types.TrainingPhrase();
                                trainingPhrase.Parts.Add(new Intent.Types.TrainingPhrase.Types.Part
                                {
                                    Text = s
                                });

                                return trainingPhrase;
                            }).ToImmutableList(),
                    frequentlyAskedQuestionModel.Responses.Select(s =>
                    {
                        var message = new Intent.Types.Message
                        {
                            Text = new Intent.Types.Message.Types.Text
                            {
                                Text_ =
                                {
                                    s
                                }
                            }
                        };

                        return message;
                    }).ToImmutableList()
                    , cancellationToken);

                Conversations.Remove(initialSocketUserMessage.Author.Id);

                await initialSocketUserMessage.Channel.SendMessageAsync(
                    "Nice, good job, Gotta catch them all!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task StartCorrectionConversationAsync(SocketUserMessage initialSocketUserMessage, CancellationToken cancellationToken)
        {
            try
            {
                await intentService.ShowAllIntents(initialSocketUserMessage);

                var conversation = new Conversation(new List<IPhase>
                {
                    new GenericActionPhase("Awaiting your question..", initialSocketUserMessage, discordSocketClient,
                        interactiveService, (model, s) =>
                        {
                            model.DisplayName = s;
                        }),
                    new RepeatingGenericActionPhase("Alright! You can add as many phrases as you would like. One message equals one phrase. You don't need to do anything when you are done, please wait at least 45 seconds after the last correction before writing a message again. The AI might take a few minutes to update.",
                        initialSocketUserMessage,
                        discordSocketClient, interactiveService, (model, s) => model.TrainingPhrases.Add(s)),
                });

                Conversations.Add(initialSocketUserMessage.Author.Id, conversation);

                var frequentlyAskedQuestionModel = await conversation.StartAsync(cancellationToken);

                await intentService.AddIntentAsync(
                    frequentlyAskedQuestionModel.DisplayName,
                    frequentlyAskedQuestionModel
                        .TrainingPhrases.Select(
                            s =>
                            {
                                var trainingPhrase = new Intent.Types.TrainingPhrase();
                                trainingPhrase.Parts.Add(new Intent.Types.TrainingPhrase.Types.Part
                                {
                                    Text = s
                                });

                                return trainingPhrase;
                            }).ToImmutableList(),
                    frequentlyAskedQuestionModel.Responses.Select(s =>
                    {
                        var message = new Intent.Types.Message
                        {
                            Text = new Intent.Types.Message.Types.Text
                            {
                                Text_ =
                                {
                                    s
                                }
                            }
                        };

                        return message;
                    }).ToImmutableList()
                    , cancellationToken);

                Conversations.Remove(initialSocketUserMessage.Author.Id);

                await initialSocketUserMessage.Channel.SendMessageAsync(
                    "Nice, good job, Gotta catch them all!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}