namespace MinecraftService.Shared.Interfaces {
    public interface IBackupManager {
        bool BackupRunning();
        void InitializeBackup();
        bool PerformBackup(string unused);
        void PerformRollback(string zipFilePath);
        bool SetBackupComplete();
    }
}