using BuildServerUtils;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Web.Script.Serialization;

namespace BuildServer
{
    [Command(CommandStrings.TestSlack)]
    public class TestSlackCommand : IServerCommand
    {
        public string Execute(BaseServer baseServer, NameValueCollection arguments)
        {
            Server server = baseServer as Server;

            if (string.IsNullOrEmpty(BuildServerSettings.SlackURL))
            {
                return "No Slack URL configured in Server Settings.xml file";
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string content = serializer.Serialize(new { text = "Test" });

            HttpClient client = new HttpClient();
            StringContent contentPost = new StringContent(content, Encoding.UTF8, "application/json");
            client.PostAsync(BuildServerSettings.SlackURL, contentPost);
            
            return "Notification sent to " + BuildServerSettings.SlackURL;
        }
    }
}