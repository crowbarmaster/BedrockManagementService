using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MinecraftService.Shared.JsonModels.Service
{
    public class JavaVersionHistoryModel
    {
        public string Version { get; set; }
        public bool IsBeta { get; set; }
        public string DownloadUrl { get; set; }
        public List<PropInfoEntry> PropList { get; set; }
    }
}
