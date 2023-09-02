using System;

namespace MinecraftService.Shared.SerializeModels {
    public class LogEntry {
        public DateTime TimeStamp { get; set; }
        public string Text { get; set; }


        public LogEntry(string entryText) {
            Text = entryText;
            TimeStamp = DateTime.Now;
        }
    }
}
