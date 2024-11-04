namespace MinecraftService.Shared.Classes.Service.Core
{
    public class ProgressModel
    {
        public string Message;
        public double Progress;

        public ProgressModel(string message, double progress)
        {
            Message = message;
            Progress = progress;
        }

        public ProgressModel UpdateProgress(string message, double progress)
        {
            Message = message;
            Progress = progress;
            return this;
        }
    }
}
