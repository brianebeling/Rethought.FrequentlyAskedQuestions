namespace Rethought.FrequentlyAskedQuestions
{
    public class PrebuiltIntents
    {
        public PrebuiltIntents(string addIntentName, string showIntentName, string correctIntentName)
        {
            AddIntentName = addIntentName;
            ShowIntentName = showIntentName;
            CorrectIntentName = correctIntentName;
        }

        public string AddIntentName { get; }

        public string ShowIntentName { get; }

        public string CorrectIntentName { get; }

    }
}