using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftService.Shared.Interfaces {
    internal interface IDeployable {
        bool DeployServer(IServerConfiguration serverConfiguration);
    }
}
