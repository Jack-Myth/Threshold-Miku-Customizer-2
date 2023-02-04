using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SteamLauncher4ThresholdMiku
{
    internal class Program
    {
        [DllImport("kernel32")]
        static extern bool AllocConsole();

        [DllImport("kernel32")]
        static extern bool FreeConsole();

        static string SteamExe, SteamPath, SkinName;
        static void Main(string[] args)
        {
            RegistryKey SteamInfo = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam", false);
            if (SteamInfo == null)
            {
                AllocConsole();
                Console.WriteLine("No Steam Found! You must open Steam at least once!");
                Console.ReadKey();
                FreeConsole();
                return;
            }
            SteamExe = SteamInfo.GetValue("SteamExe", "") as string;
            if (SteamExe==null || SteamExe == "")
            {
                AllocConsole();
                Console.WriteLine("Can't found SteamExe! You must open Steam at least once!");
                Console.ReadKey();
                FreeConsole();
                return;
            }
            SteamPath = SteamInfo.GetValue("SteamPath", "") as string;
            if (SteamPath == null || SteamPath == "")
            {
                AllocConsole();
                Console.WriteLine("Can't found SteamPath! You must open Steam at least once!");
                Console.ReadKey();
                FreeConsole();
                return;
            }
            SkinName = SteamInfo.GetValue("SkinV5", "") as string;
            if (SkinName == null || SkinName == "")
            {
                StartSteam("", args);
                return;
            }
            SteamInfo.Close();
            RegistryKey AutoStartInfo = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if(AutoStartInfo!=null)
            {
                string SteamAutoStart = AutoStartInfo.GetValue("Steam") as string;
                if (SteamAutoStart != null)
                {
                    AutoStartInfo.SetValue("Steam", 
                        string.Format("\"{0}\" -silent", System.Reflection.Assembly.GetExecutingAssembly().Location));
                }
                AutoStartInfo.Close();
            }
            string sourceCSS = SteamPath + "/skins/" + SkinName + "/resource/webkit.css";
            if(!File.Exists(sourceCSS))
            {
                StartSteam("-noverifyfiles", args);
                return;
            }
            string destCSS = SteamPath + "/steamui/skin.css";
            string html = SteamPath + "/steamui/index.html";
            File.Copy(sourceCSS, destCSS, true);
            string htmlContent = File.ReadAllText(html);
            if (htmlContent.IndexOf("/skin.css") < 0)
            {
                int HeadIndex = htmlContent.IndexOf("</head>");
                if (HeadIndex > 0)
                {
                    htmlContent = htmlContent.Insert(HeadIndex, "<link href=\"/skin.css\" rel=\"stylesheet\">");
                    File.WriteAllText(html, htmlContent);
                }
            }
            StartSteam("-noverifyfiles",args);
        }

        static void StartSteam(string InPassArgs,string[] args)
        {
            string PassArgs = InPassArgs;
            for (int i = 0; i < args.Length; i++)
            {
                PassArgs += " " + args[i];
            }
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.WorkingDirectory = SteamPath;
            processStartInfo.Arguments = PassArgs;
            processStartInfo.FileName = SteamExe;
            Process.Start(processStartInfo);
        }
    }
}
