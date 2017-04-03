using BuildServerUtils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServer
{
    [Command(CommandStrings.TestEmail)]
    public class TestEmailCommand : IServerCommand
    {
        public string Execute(BaseServer baseServer, NameValueCollection arguments)
        {
            Server server = baseServer as Server;

            string[] emails = arguments.GetValues(CommandStrings.EmailAddress);
            if (emails == null)
            {
                return "Specify an email using email=[email]";
            }

            string email = emails.Length > 0 ? emails[0] : "";

            User user = new User(email, "N");
            user.Message(new StringBuilder("Test Email"), "Test Email", true);

            return "Email sent to " + email;
        }
    }
}