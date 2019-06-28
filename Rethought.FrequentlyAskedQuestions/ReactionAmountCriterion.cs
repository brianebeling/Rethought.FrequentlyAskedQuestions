using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace Rethought.FrequentlyAskedQuestions
{
    public class ReactionAmountCriterion : ICriterion<SocketReaction>
    {
        public async Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketReaction parameter)
        {
            var message = await sourceContext.Channel.GetMessageAsync(parameter.MessageId) as RestUserMessage;


            var count = message.Reactions.FirstOrDefault(pair => pair.Key.Name == "✅").Value.ReactionCount;

            return count == 2;
        }
    }
}