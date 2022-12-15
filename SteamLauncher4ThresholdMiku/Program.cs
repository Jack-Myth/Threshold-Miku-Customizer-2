using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace SteamLauncher4ThresholdMiku
{
    internal class Program
    {
        static string SteamExe, SteamPath, SkinName;
        static void Main(string[] args)
        {
            
            RegistryKey SteamInfo = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam", false);
            if (SteamInfo == null)
            {
                MessageBox.Show("You must open Steam at least once!", "No Steam Found!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SteamExe = SteamInfo.GetValue("SteamExe", "") as string;
            if (SteamExe==null || SteamExe == "")
            {
                MessageBox.Show("You must open Steam at least once!", "Can't found SteamExe!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SteamPath = SteamInfo.GetValue("SteamPath", "") as string;
            if (SteamPath == null || SteamPath == "")
            {
                MessageBox.Show("You must open Steam at least once!", "Can't found SteamPath!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SkinName = SteamInfo.GetValue("SkinV5", "") as string;
            if (SkinName == null || SkinName == "")
            {
                StartSteam("", args);
                return;
            }
            SteamInfo.Close();
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
