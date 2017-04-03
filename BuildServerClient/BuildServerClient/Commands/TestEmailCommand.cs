using BuildServerUtils;
using System.Collections.Generic;

namespace BuildServerClient
{
    [Command(CommandStrings.TestEmail)]
    public class TestEmailCommand : IClientCommand
    {
        public string Description
        {
            get { return "Send a test email to check the email configurations are correct"; }
        }

        public void Execute(BaseClient client, List<string> parameters)
        {
            string emailAddress = parameters.Count == 0 ? ClientSettings.Email : parameters[0];

            client.Get(
                CommandStrings.TestEmail,
                new KeyValuePair<string, string>(ParameterStrings.EmailAddress, emailAddress));
        }
    }
}
