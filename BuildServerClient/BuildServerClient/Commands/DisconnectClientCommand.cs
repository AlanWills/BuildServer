using BuildServerUtils;

namespace BuildServerClient
{
    [Command(CommandStrings.DisconnectClient)]
    public class DisconnectClientCommand : IClientCommand
    {
        public void Execute(BaseClient client)
        {
            client.ServerComms.Send(CommandStrings.DisconnectClient);
        }
    }
}
