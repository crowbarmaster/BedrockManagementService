using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftService.Shared.Classes {
    public class ProgressModel {
        public string Message;
        public double Progress;

        public ProgressModel(string message, double progress) {
            Message = message;
            Progress = progress;
        }

        public ProgressModel UpdateProgress(string message, double progress) {
            Message = message;
            Progress = progress;
            return this;
        }
    }
}
