using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Unicode;
using System.Text.Encodings.Web;
using System.Security;

namespace File_backup
{
    internal class Program
    {
       
        static void Main(string[] args)
        {

            string Settings_Option;

            Console.Write("Хотите использовать старые или новые настройки? (old/new): ");
            Settings_Option = Console.ReadLine();


            string PathFileSettings = Directory.GetCurrentDirectory()+"\\AppSettings.json";
            SettingsBackup settings = have_setting(PathFileSettings, Settings_Option);

            Console.WriteLine(settings.Source_folder1 + "\t" + settings.Source_folder2 + "\t" + settings.Target_folder +"\n\n");

            Console.WriteLine("Start of copying...");
            CopyDirectory(settings.Source_folder1, CreateDir(settings.Target_folder), true);
            Console.WriteLine("Copying completed!");

            Console.ReadLine();
        }


        public class SettingsBackup
        {
            public string Source_folder1 { get; set; }
            public string Source_folder2 { get; set; }
            public string Target_folder { get; set; }

            public SettingsBackup()
            {

            }
            public SettingsBackup(bool NewCopy)
            {
                if (!NewCopy)
                {
                    Console.Write("Введите путь копируемой папки: ");
                    Source_folder1 = Console.ReadLine();

                    //Console.Write("Введите путь второй копируемой папки: ");
                    //Source_folder2 = Console.ReadLine();

                    Console.Write("Введите путь куда скопировать файлы: ");
                    Target_folder = Console.ReadLine();
                }

            }
        }


        public static SettingsBackup have_setting(string PathFileSettings, string Settings_Option)
        {
            Settings_Option.ToLower();
            if (!File.Exists(PathFileSettings) || Settings_Option == "new")
            {
                if (Settings_Option != "new")
                    Console.WriteLine("Старые настройки не найдены!");

                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.All),
                    WriteIndented = true
                };

                SettingsBackup BackupSettings = new SettingsBackup(false);
                string jsonString = JsonSerializer.Serialize(BackupSettings, options);

                Console.WriteLine(jsonString + "\n\n");
                File.WriteAllText("AppSettings.json", jsonString);
                return BackupSettings;
            }
            else
            {
                string jsonStringData = File.ReadAllText("AppSettings.json");
                SettingsBackup NewSettings = JsonSerializer.Deserialize<SettingsBackup>(jsonStringData);

                return NewSettings;
            }
        }

        static string CreateDir(string Target_Dir)
        {            
            DateTime utcDate = DateTime.UtcNow;
            string DateCopy = Target_Dir + @"\" + utcDate.ToShortDateString();
            string TimeCopy = DateCopy + @"\" + utcDate.Hour + "h" + utcDate.Minute + "m" + utcDate.Second + "s utc";
            DirectoryInfo dirInfo = new DirectoryInfo(Target_Dir);
            DirectoryInfo dirTimeInfo = new DirectoryInfo(DateCopy);

            try
            {
                if (dirInfo.Exists)
                {
                    Directory.CreateDirectory(DateCopy);
                    if (dirTimeInfo.Exists)
                    {
                        Directory.CreateDirectory(TimeCopy);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return TimeCopy;
        }

        static void CopyDirectory(string sourceDir, string Target_Dir, bool recursive)
        {          
            try
            {
                var dir = new DirectoryInfo(sourceDir);

                if (!dir.Exists)
                    throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

                DirectoryInfo[] dirs = dir.GetDirectories();
                Directory.CreateDirectory(Target_Dir);               

                foreach (FileInfo file in dir.GetFiles())
                {                    
                    Console.WriteLine(file.FullName);
                    string targetFilePath = Path.Combine(Target_Dir, file.Name);
                    file.CopyTo(targetFilePath);                    
                }

                if (recursive)
                {
                    foreach (DirectoryInfo subDir in dirs)
                    {
                        string newDestinationDir = Path.Combine(Target_Dir, subDir.Name);
                        CopyDirectory(subDir.FullName, newDestinationDir, true);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
          
        }
    }
}
