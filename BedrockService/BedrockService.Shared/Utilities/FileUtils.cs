using BedrockService.Shared.MincraftJson;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BedrockService.Shared.Utilities
{
    public class FileUtils
    {
        readonly string _servicePath;
        public FileUtils(string servicePath)
        {
            _servicePath = servicePath;
        }
        public void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        }

        public void DeleteFilesRecursively(DirectoryInfo source, bool removeSourceFolder)
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

        public void ClearTempDir()
        {
            DirectoryInfo tempDirectory = new DirectoryInfo($@"{_servicePath}\Temp");
            if (!tempDirectory.Exists)
                tempDirectory.Create();
            foreach (FileInfo file in tempDirectory.GetFiles("*", SearchOption.AllDirectories))
                file.Delete();
            foreach (DirectoryInfo directory in tempDirectory.GetDirectories())
                directory.Delete(true);
        }

        public void DeleteFilelist(string[] fileList, string serverPath)
        {
            foreach (string file in fileList)
                try
                {
                    File.Delete($@"{serverPath}\{file}");
                }
                catch { }
            List<string> exesInPath = Directory.EnumerateFiles(serverPath, "*.exe", SearchOption.AllDirectories).ToList();
            foreach (string exe in exesInPath)
                File.Delete(exe);
            foreach (string dir in Directory.GetDirectories(serverPath))
                if (Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Count() == 0)
                    Directory.Delete(dir, true);
        }

        public void WriteStringToFile(string path, string content) => File.WriteAllText(path, content);

        public void WriteStringArrayToFile(string path, string[] content) => File.WriteAllLines(path, content);

        public void UpdateJArrayFile(string path, JArray content, System.Type type)
        {
            try
            {
                string currentFileContents = null;
                JArray jArray = new JArray();
                if (File.Exists(path))
                {
                    currentFileContents = File.ReadAllText(path);
                    jArray = JArray.Parse(currentFileContents);
                }
                if(type == typeof(WorldPacksJsonModel))
                {
                    foreach (JToken jToken in content)
                    {
                        bool doesContainToken = false;
                        foreach (JToken currentToken in jArray)
                        {
                            if (currentToken.ToObject<WorldPacksJsonModel>().pack_id == jToken.ToObject<WorldPacksJsonModel>().pack_id)
                            {
                                doesContainToken = true;
                            }
                        }
                        if (!doesContainToken)
                        {
                            jArray.Add(jToken);
                        }
                    }
                }
                File.WriteAllText(path, jArray.ToString());
            }
            catch(System.Exception)
            {
                return;
            }
        }
    }
}
