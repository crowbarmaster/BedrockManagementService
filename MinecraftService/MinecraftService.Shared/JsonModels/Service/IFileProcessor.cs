using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftService.Shared.JsonModels.Service {
    public interface IFileProcessor {
        void Process(byte[] fileBytes);
    }
}
