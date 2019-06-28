using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rethought.FrequentlyAskedQuestions
{
    public class Conversation
    {
        private readonly IList<IPhase> phases;

        public Conversation(IList<IPhase> phases)
        {
            this.phases = phases;
        }

        public async Task<FrequentlyAskedQuestionModel> StartAsync(CancellationToken cancellationToken)
        {
            var frequentlyAskedQuestionModel = new FrequentlyAskedQuestionModel();

            foreach (var phase in phases)
                frequentlyAskedQuestionModel =
                    await phase.CompleteAsync(frequentlyAskedQuestionModel, cancellationToken);

            return frequentlyAskedQuestionModel;
        }
    }
}