using System.Collections.Generic;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Shared.Classes.Configurations {
    public static class CommonConfigDefaults {
        public static List<Property> PropList = new List<Property>() {
            new Property(ServerPropertyStrings[ServerPropertyKeys.ServerAutostartEnabled], "true"),
            new Property(ServerPropertyStrings[ServerPropertyKeys.AutoBackupEnabled], "false"),
            new Property(ServerPropertyStrings[ServerPropertyKeys.AutoRestartEnabled], "false"),
            new Property(ServerPropertyStrings[ServerPropertyKeys.BackupCron], "0 1 * * *"),
            new Property(ServerPropertyStrings[ServerPropertyKeys.MaxBackupCount], "25"),
            new Property(ServerPropertyStrings[ServerPropertyKeys.AutoBackupsContainPacks], "false"),
            new Property(ServerPropertyStrings[ServerPropertyKeys.IgnoreInactiveBackups], "true"),
            new Property(ServerPropertyStrings[ServerPropertyKeys.CheckUpdatesEnabled], "true"),
            new Property(ServerPropertyStrings[ServerPropertyKeys.AutoDeployUpdates], "true"),
            new Property(ServerPropertyStrings[ServerPropertyKeys.UpdateCron], "0 2 * * *"),
            new Property(ServerPropertyStrings[ServerPropertyKeys.RestartCron], "0 2 * * *"),
            new Property(ServerPropertyStrings[ServerPropertyKeys.ServerVersion], "None"),
            new Property(ServerPropertyStrings[ServerPropertyKeys.UseBetaVersions], "false"),
            new Property(ServerPropertyStrings[ServerPropertyKeys.UseErrorFilter], "true")
        };
    }
}
