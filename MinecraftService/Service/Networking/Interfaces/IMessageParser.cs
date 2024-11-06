using MinecraftService.Shared.Classes.Networking;

namespace MinecraftService.Service.Networking.Interfaces
{
    public interface IMessageParser {
        Message ParseMessage(Message message);
    }
}
