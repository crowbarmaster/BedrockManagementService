using BedrockService.Shared.Classes;
using System.Collections.Generic;

namespace BedrockService.Shared.Interfaces
{
    public interface IServerConfiguration : IConfiguration
    {
        string GetServerName();

        string GetFileName();

        void AddStartCommand(string command);

        bool DeleteStartCommand(string command);

        List<StartCmdEntry> GetStartCommands();

        void SetStartCommands(List<StartCmdEntry> newEntries);

        void AddUpdatePlayer(IPlayer player);

        IPlayer GetPlayerByXuid(string xuid);

        List<IPlayer> GetPlayerList();

        void SetPlayerList(List<IPlayer> newList);

        IServerConfiguration GetServerInfo();
    }
}
