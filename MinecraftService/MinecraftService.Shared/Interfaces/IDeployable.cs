namespace MinecraftService.Shared.Interfaces {
    internal interface IDeployable {
        bool DeployServer(IServerConfiguration serverConfiguration);
    }
}
