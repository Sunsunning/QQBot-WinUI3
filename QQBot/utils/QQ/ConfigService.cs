using System.IO;
using static PluginDLL.Class1;

namespace QQBotCodePlugin.QQBot.utils.QQ
{
    public class ConfigService : IConfigService
    {
        private readonly string DirectoryPath;

        public ConfigService(string DirectoryPath)
        {
            this.DirectoryPath = DirectoryPath;
        }

        public void CreateConfig(string folder, string FileName)
        {
            if (folder == null)
            {
                return;
            }
            if (FileName == null)
            {
                return;
            }
            string folderPath = Path.Combine(DirectoryPath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string filePath = Path.Combine(folderPath, FileName);
            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }
        }

        public void DeleteConfig(string folder, string FileName)
        {
            if (folder == null)
            {
                return;
            }
            if (FileName == null)
            {
                return;
            }
            string folderPath = Path.Combine(DirectoryPath, folder);
            if (!Directory.Exists(folderPath))
            {
                return;
            }
            string filePath = Path.Combine(folderPath, FileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public string getCurrentDirectory()
        {
            return DirectoryPath;
        }
    }
}
