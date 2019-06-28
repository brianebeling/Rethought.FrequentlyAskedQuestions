using System.Collections.Generic;

namespace Rethought.FrequentlyAskedQuestions
{
    public class FrequentlyAskedQuestionModel
    {
        public string DisplayName { get; set; }

        public IList<string> TrainingPhrases { get; set; } = new List<string>();

        public IList<string> Responses { get; set; } = new List<string>();
    }
}