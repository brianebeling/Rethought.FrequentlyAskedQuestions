using System.Threading;
using System.Threading.Tasks;

namespace Rethought.FrequentlyAskedQuestions
{
    public interface IPhase
    {
        Task<FrequentlyAskedQuestionModel> CompleteAsync(FrequentlyAskedQuestionModel frequentlyAskedQuestionModel,
            CancellationToken cancellationToken);
    }
}