using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace VRCTC_Console_Installer
{
    public static class Program
    {
        private const string MELON_LOADER_ZIP = "MelonLoader.zip";
        private const string AUTO_TRANSLATOR_ZIP = "XUnity.AutoTranslator.zip";
        private const string VRCTC_ZIP = "VRCTC.zip";

        private static readonly string separator = new string(' ', 5);
        private static readonly string line = new string('-', 60);

        private static void writePrefix(string content)
        {
            Console.WriteLine($"{separator}{content}");
        }

        private static void writeTitle()
        {
            writePrefix(line);
            writePrefix($"| {new string(' ', 15)}VRCLocale 繁體中文化{new string(' ', 21)} |");
            writePrefix($"| {separator}注意 : 安裝前請先關閉 VRChat，以免造成存取被拒{separator} |");
            writePrefix(line);
        }

        private static void writeStepFinish()
        {
            Console.WriteLine("完成");
        }

        private static void writeFinish()
        {
            Console.WriteLine();
            writePrefix(line);
            writePrefix($"| {new string(' ', 15)}VRCLocale 繁體中文化安裝完畢{new string(' ', 13)} |");
            writePrefix(line);
        }

        private static void writeFail()
        {
            Console.WriteLine();
            writePrefix(line);
            writePrefix($"| {new string(' ', 15)}安裝失敗，請重新安裝{new string(' ', 21)} |");
            writePrefix(line);
        }

        private static void pause()
        {
            Console.WriteLine();
            Console.Write("按下任意鍵繼續 ...");
            Console.ReadKey(true);
        }

        private static void trapper(object sender, UnhandledExceptionEventArgs e)
        {
            const int FAIL = 1;
            MessageBox.Show($"程式發生無法處理的例外狀況 : {e.ExceptionObject}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(FAIL);
        }

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += trapper;

            Console.Title = "VRCLocale 繁體中文化安裝工具";
            writeTitle();
            pause();
            Console.Clear();

            writeTitle();
            Console.WriteLine();
            Console.WriteLine("[初始化] 搜尋 VRChat 安裝位置");

            try
            {
                InstallHelper helper = new InstallHelper();
                if (string.IsNullOrWhiteSpace(helper.VRChatPath))
                {
                    string search = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "VRChat.exe");
                    if (!File.Exists(search))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("找不到 VRChat 安裝目錄");
                        Console.WriteLine("您可以嘗試將這個檔案放在 VRChat 目錄下");

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("如何找到 VRChat 安裝目錄");
                        Console.WriteLine("開啟 Steam -> 收藏庫 -> 右鍵 VRChat -> 管理 -> 瀏覽本機檔案");

                        Console.ForegroundColor = ConsoleColor.Gray;
                        writeFail();
                        pause();
                        return;
                    }
                    helper.VRChatPath = search;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"已找到 VRChat 安裝位置 : {helper.VRChatPath}");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;

                Console.Write("正在移除先前版本 ... ");
                helper.removeOldVersion();
                writeStepFinish();

                Console.Write("正在下載並解壓 Melon Loader ... ");
                helper.downloadTempFile(InstallHelper.MELON_LOADER_URL, MELON_LOADER_ZIP);
                helper.extractZIPTemp(MELON_LOADER_ZIP);
                writeStepFinish();

                Console.Write("正在下載並解壓 X Unity 自動翻譯插件 ... ");
                helper.downloadTempFile(InstallHelper.getLatestAutoTranslatorURL(), AUTO_TRANSLATOR_ZIP);
                helper.extractZIPTemp(AUTO_TRANSLATOR_ZIP);
                writeStepFinish();

                Console.Write("正在下載並解壓繁體中文化文本 ... ");
                helper.downloadTempFile(InstallHelper.TC_URL, VRCTC_ZIP);
                helper.extractZIPTemp(VRCTC_ZIP);
                writeStepFinish();

                Console.Write("正在移除無用檔案 ... ");
                helper.removeUselessTempFile();
                writeStepFinish();

                Console.Write("正在清理壓縮檔案 ... ");
                helper.deleteTempFile(MELON_LOADER_ZIP);
                helper.deleteTempFile(AUTO_TRANSLATOR_ZIP);
                helper.deleteTempFile(VRCTC_ZIP);
                writeStepFinish();

                Console.Write("正在安裝所有檔案 ... ");
                helper.moveTempDirectory();
                writeStepFinish();

                writeFinish();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"發生例外狀況 : {ex.Message}");

                Console.ForegroundColor = ConsoleColor.Gray;
                writeFail();
            }
            pause();
        }
    }
}