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
            this.configurationRoot.GetSection("Authentication").GetSection("Discord").Bind(discordAuthentication);

            return discordAuthentication;
        }

        public DialogflowConfiguration GetDialogflowConfiguration()
        {
            var dialogflowConfiguration = new DialogflowConfiguration();
            this.configurationRoot.GetSection("Authentication").GetSection("Dialogflow").Bind(dialogflowConfiguration);
            this.configurationRoot.GetSection("Dialogflow").Bind(dialogflowConfiguration);

            return dialogflowConfiguration;

        }
    }
}