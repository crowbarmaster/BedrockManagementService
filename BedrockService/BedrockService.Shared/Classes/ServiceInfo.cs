using BedrockService.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BedrockService.Shared.Classes {
    public class ServiceInfo {
        public string serverVersion { get; set; }
        public List<LogEntry> serviceLog = new List<LogEntry>();
        public List<IServerConfiguration> ServerList = new List<IServerConfiguration>();
        public List<Property> globals = new List<Property>();
        public List<Property> _defaultServerProps = new List<Property>();
    }
}