using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Service.Utilities
{
    public static class FileUtils
    {
        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        }

        public static void DeleteFilesRecursively(DirectoryInfo source, bool removeSourceFolder)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                DeleteFilesRecursively(dir, removeSourceFolder);
            foreach (FileInfo file in source.GetFiles())
                file.Delete();
            foreach (DirectoryInfo emptyDir in source.GetDirectories())
                emptyDir.Delete(true);
            if (removeSourceFolder)
                source.Delete(true);
        }
    }
}
