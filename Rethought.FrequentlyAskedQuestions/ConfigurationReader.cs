using Microsoft.Extensions.Configuration;

namespace Rethought.FrequentlyAskedQuestions
{
    public class ConfigurationReader
    {
        private readonly IConfigurationRoot configurationRoot;

        public ConfigurationReader(IConfigurationRoot configurationRoot)
        {
            this.configurationRoot = configurationRoot;
        }

        public DiscordAuthentication GetDiscordAuthentication()
        {
            var discordAuthentication = new DiscordAuthentication();
            configurationRoot.GetSection("Authentication").GetSection("Discord").Bind(discordAuthentication);

            return discordAuthentication;
        }

        public DialogflowConfiguration GetDialogflowConfiguration()
        {
            var dialogflowConfiguration = new DialogflowConfiguration();
            configurationRoot.GetSection("Authentication").GetSection("Dialogflow").Bind(dialogflowConfiguration);
            configurationRoot.GetSection("Dialogflow").Bind(dialogflowConfiguration);

            return dialogflowConfiguration;
        }
    }
}