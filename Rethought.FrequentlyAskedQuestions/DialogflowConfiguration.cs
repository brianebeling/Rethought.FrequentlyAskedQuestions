﻿namespace Rethought.FrequentlyAskedQuestions
{
    public class DialogflowConfiguration
    {
        public string AuthFileName { get; set; }

        public string ProjectId { get; set; }

        public string ProjectIdView => $"projects/{ProjectId}/agent";
    }
}