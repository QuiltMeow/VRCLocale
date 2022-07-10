using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows;

namespace MakeZIP
{
    public static class Program
    {
        private const string ROOT = "AutoTranslator";
        private const string DIRECTORY = "Translation";
        private const string ZIP_NAME = "Updater.zip";

        private static void trapper(object sender, UnhandledExceptionEventArgs e)
        {
            const int FAIL = 1;
            MessageBox.Show($"程式發生無法處理的例外狀況 : {e.ExceptionObject}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(FAIL);
        }

        public static string getRelativePath(string fromPath, string toPath)
        {
            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            string toScheme = toUri.Scheme;
            if (fromUri.Scheme != toScheme)
            {
                return toPath;
            }
            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toScheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
            return relativePath;
        }

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += trapper;
            Console.Title = "建立壓縮檔";
            try
            {
                string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string pack = Path.Combine(path, DIRECTORY);
                string[] file = Directory.GetFileSystemEntries(pack, "*", SearchOption.AllDirectories);

                Console.WriteLine("正在建立壓縮檔 ...");
                using (FileStream fs = new FileStream(Path.Combine(path, ZIP_NAME), FileMode.Create, FileAccess.Write))
                {
                    using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Create, true))
                    {
                        foreach (string item in file)
                        {
                            if (!Directory.Exists(item))
                            {
                                string relative = getRelativePath($"{path}{Path.DirectorySeparatorChar}", item);
                                string[] split = relative.Split(Path.DirectorySeparatorChar);
                                for (int i = split.Length - 2; i >= 0; --i)
                                {
                                    if (split[i].Equals("zh_TW"))
                                    {
                                        split[i] = "zh";
                                        relative = Path.Combine(split);
                                        break;
                                    }
                                }

                                ZipArchiveEntry entry = zip.CreateEntry($"{ROOT}{Path.DirectorySeparatorChar}{relative}", CompressionLevel.Optimal);
                                using (BinaryWriter bw = new BinaryWriter(entry.Open()))
                                {
                                    bw.Write(File.ReadAllBytes(item));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"發生例外狀況 : {ex.Message}");
            }
            Console.WriteLine("程式結束");

            Console.WriteLine();
            Console.Write("按下任意鍵繼續 ...");
            Console.ReadKey(true);
        }
    }
}