namespace BedrockService.Service.Server.HostInfoClasses
{

    public class StartCmdEntry
    {
        public string Command = "help 1";

        public StartCmdEntry(string entry)
        {
            Command = entry;
        }
    }
}