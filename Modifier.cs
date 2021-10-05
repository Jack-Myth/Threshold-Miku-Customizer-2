using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Threshold_Miku_Customizer_2
{
    class G
    {
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        public delegate void OnReplacingFile(string NewFile, string FileToBeReplaced);

        //Special Image
        public const string MainBG = "MainBG";

        public static System.Drawing.Color MainContentBaseColor;

        //Image:Preview
        public static readonly Dictionary<string, string> TGAImageList = new Dictionary<string, string>
        {
            { MainBG,"NewLibrary"},
            { "BackupWizard", "BackupWizard"},
            { "bg_security_wizard","SecurityWizard"},
            { "CDKeyWizard","CDKeyWizard"},
            { "GameProperties","GameProperties"},
            { "InstallAppWizard","InstallAppWizard"},
            { "LoginBG","Login"},
            { "MusicPlayerImg","MusicPlayer"},
            { "OverlayBG","Overlay"},
            { "SettingsDialog","Settings"},
            { "SystemInfo","SystemInfo"}
        };

        public static List<IModifier> ModifierList;

        public static MainWindow MainWindow;

        //Image:Res
        public static Dictionary<string, string> ImageResList = new Dictionary<string, string>();

        public static Dictionary<string, string> TGAImageReplaceList = new Dictionary<string, string>();

        public static Dictionary<string, string> FontSettings = new Dictionary<string, string>();

        public static T RegisterModifier<T>() where T:IModifier,new()
        {
            if (ModifierList==null)
                ModifierList = new List<IModifier>();
            T Modifier = new T();
            ModifierList.Add(Modifier);
            return Modifier;

        }

        public static void replaceFont(string DicID, string DefaultFont)
        {
            string FontID = DicID.ToLower();
            string ReplaceContent = "\n\t" + FontID + "=\"";
            if (FontSettings.ContainsKey(DicID) && FontSettings[DicID] != "")
                ReplaceContent += FontSettings[DicID];
            else
                ReplaceContent += DefaultFont;
            ReplaceContent += "\"\n\t";
            ReplaceByMark(".\\resource\\styles\\steam.styles", DicID, ReplaceContent, "//", "");
        }

        public static void replaceFontColor(string DicID)
        {
            if (!FontSettings.ContainsKey(DicID))
                return;
            string ReplaceContent = string.Format("color: {0};", FontSettings[DicID]);
            ReplaceByMark(".\\resource\\webkit.css", DicID, ReplaceContent);
        }

        public static void CopyDirectory(string srcPath, string destPath, OnReplacingFile onReplacingFileDelegate = null)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)     //判断是否文件夹
                    {
                        if (!Directory.Exists(destPath + "\\" + i.Name))
                        {
                            Directory.CreateDirectory(destPath + "\\" + i.Name);   //目标目录下不存在此文件夹即创建子文件夹
                        }
                        CopyDirectory(i.FullName, destPath + "\\" + i.Name, onReplacingFileDelegate);    //递归调用复制子文件夹
                    }
                    else
                    {
                        if (onReplacingFileDelegate != null && File.Exists(destPath + "\\" + i.Name))
                            onReplacingFileDelegate(i.FullName, destPath + "\\" + i.Name);
                        File.Copy(i.FullName, destPath + "\\" + i.Name, true);      //不是文件夹即复制文件，true表示可以覆盖同名文件
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void BackupReplaceingFiles(string SourceDir, string BcakupToDir)
        {
            if (!System.IO.Directory.Exists(BcakupToDir))
            {
                Directory.CreateDirectory(BcakupToDir);
                CopyDirectory(SourceDir, ".\\", (string NewFile, string FileToBeReplaced) =>
                {
                    //Make a backup
                    string mFileToBeReplaced = System.IO.Path.GetFullPath(FileToBeReplaced);
                    string mRoot = System.IO.Path.GetFullPath(".\\");
                    string Target = BcakupToDir + "\\" + mFileToBeReplaced.Substring(mRoot.Length);
                    Directory.CreateDirectory(Target.Substring(0, Target.LastIndexOf("\\")));
                    File.Copy(FileToBeReplaced, Target, true); //True is not necessary
                });
            }
            else
            {
                CopyDirectory(SourceDir, ".\\");
            }
        }

        public static void ReplaceByMark(string FilePath, string Mark, string NewContent, string PreComment = "/*", string PostComment = "*/")
        {
            string FileContent = File.ReadAllText(FilePath);
            string BeginMark = PreComment + "TMC2:Begin" + Mark + PostComment;
            string EndMark = PreComment + "TMC2:End" + Mark + PostComment;
            int BeginIndex = FileContent.IndexOf(BeginMark);
            if (BeginIndex < 0)
                throw new KeyNotFoundException("Mark:" + Mark + " not found at File:" + FilePath);
            int EndIndex = FileContent.IndexOf(EndMark);
            if (EndIndex < 0)
                throw new OverflowException("Mark:" + Mark + " is not closed at File:" + FilePath);
            File.WriteAllText(FilePath, FileContent.Substring(0, BeginIndex + BeginMark.Length) + NewContent + FileContent.Substring(EndIndex));
        }

        public static string GetContentByMark(string FilePath, string Mark, string PreComment = "/*", string PostComment = "*/")
        {
            string FileContent = File.ReadAllText(FilePath);
            string BeginMark = PreComment + "TMC2:Begin" + Mark + PostComment;
            string EndMark = PreComment + "TMC2:End" + Mark + PostComment;
            int BeginIndex = FileContent.IndexOf(BeginMark);
            if (BeginIndex < 0)
                throw new KeyNotFoundException("Mark:" + Mark + " not found at File:" + FilePath);
            int EndIndex = FileContent.IndexOf(EndMark);
            if (EndIndex < 0)
                throw new OverflowException("Mark:" + Mark + " is not closed at File:" + FilePath);
            return FileContent.Substring(BeginIndex + BeginMark.Length, EndIndex - BeginIndex - BeginMark.Length);
        }
    }

    interface IModifier
    {
        void Apply();
        void Save(ref JObject SavedData);
        void Load(JObject SavedData);
        void Reset();
    }

    public class BackgroundImageModifier : IModifier
    {
        public void Apply()
        {
            //Image
            foreach (KeyValuePair<string, string> TGAReplaceItem in G.TGAImageReplaceList)
            {
                if (TGAReplaceItem.Value == "")
                    continue;
                string TGAPartPath = string.Format(".\\graphics\\JackMyth\\{0}", TGAReplaceItem.Key);
                if (!System.IO.File.Exists(string.Format("{0}.tmc2.bak", TGAPartPath)) && File.Exists(string.Format("{0}.tga", TGAPartPath)))
                    System.IO.File.Copy(string.Format("{0}.tga", TGAPartPath), string.Format("{0}.tmc2.bak", TGAPartPath));
                var m_TGA = new ImageTGA();
                m_TGA.Image = new System.Drawing.Bitmap(TGAReplaceItem.Value);
                System.IO.File.Delete(string.Format("{0}.tga", TGAPartPath));
                m_TGA.SaveImage(string.Format("{0}.tga", TGAPartPath));
            }
            if (System.IO.Directory.Exists(".\\Customization\\Backup\\WebPageStyle"))
            {
                G.CopyDirectory(".\\Customization\\Backup\\WebPageStyle", ".\\");
            }
            else
            {
                Directory.CreateDirectory(".\\Customization\\Backup\\WebPageStyle\\resource");
                File.Copy(".\\resource\\webkit.css", ".\\Customization\\Backup\\WebPageStyle\\resource\\webkit.css");
            }
        }

        public static string GetTempFilePathWithExtension(string extension)
        {
            var path = Path.GetTempPath();
            var fileName = Guid.NewGuid().ToString() + extension;
            return Path.Combine(path, fileName);
        }

        public void Load(JObject SavedData)
        {
            JObject ImagesObj = SavedData["BackgroundImages"] as JObject;
            if(ImagesObj==null)
            {
                return;
            }
            foreach(var KV in ImagesObj)
            {
                string TempImage = GetTempFilePathWithExtension(".jpg");
                FileStream TempImageFile = File.Create(TempImage);
                byte[] ImgData = Convert.FromBase64String(KV.Value.ToString());
                TempImageFile.Write(ImgData,0, ImgData.Length);
                G.TGAImageReplaceList[KV.Key] = TempImage;
            }
        }

        public void Reset()
        {
            //Reset All Images
            //Image
            foreach (string TGAItem in G.TGAImageList.Keys)
            {
                string TGAPartPath = string.Format(".\\graphics\\JackMyth\\{0}", TGAItem);
                if (System.IO.File.Exists(string.Format("{0}.tmc2.bak", TGAPartPath)))
                {
                    File.Delete(string.Format("{0}.tga", TGAPartPath));
                    System.IO.File.Move(string.Format("{0}.tmc2.bak", TGAPartPath), string.Format("{0}.tga", TGAPartPath));
                }
            }
            G.TGAImageReplaceList.Clear();
            G.TGAImageReplaceList.Add(G.MainBG, "");
        }

        public void Save(ref JObject SavedData)
        {
            JObject ImgsObj = new JObject();
            foreach (KeyValuePair<string, string> TGAReplaceItem in G.TGAImageReplaceList)
            {
                if (TGAReplaceItem.Value == "")
                    continue;
                byte[] ImgBytes = File.ReadAllBytes(TGAReplaceItem.Value);
                if (ImgBytes.Length > 0)
                {
                    ImgsObj[TGAReplaceItem.Key] = Convert.ToBase64String(ImgBytes);
                }
            }
            SavedData["BackgroundImages"] = ImgsObj;
        }
    }

    public class SidebarModifier : IModifier
    {
        public void Apply()
        {
            //Collapsed Sidebar
            if (G.MainWindow.CollapsedSideBar.IsChecked == true)
            {
                G.BackupReplaceingFiles(".\\Customization\\Collapsed Sidebar", ".\\Customization\\Backup\\Collapsed Sidebar");
                for (int i = 0; i < 3; i++)
                {
                    G.ReplaceByMark(".\\resource\\webkit.css", "CSide" + i.ToString(),
                        "\r\n\tbackground-position: -48px -31px;\r\n\tbackground-size: calc(100% + 48px) calc(100% + 80px);\r\n\t");
                }
            }
            else
            {
                if (System.IO.Directory.Exists(".\\Customization\\Backup\\Collapsed Sidebar"))
                {
                    G.CopyDirectory(".\\Customization\\Backup\\Collapsed Sidebar", ".\\");
                    Directory.Delete(".\\Customization\\Backup\\Collapsed Sidebar", true);
                }
                for (int i = 0; i < 3; i++)
                {
                    G.ReplaceByMark(".\\resource\\webkit.css", "CSide" + i.ToString(),
                        "\r\n\tbackground-position: -240px -31px;\r\n\tbackground-size: calc(100% + 240px) calc(100% + 80px);\r\n\t");
                }
            }
        }

        public void Load(JObject SavedData)
        {
            if (SavedData["CollapsedSideBar"] != null)
            {
                G.MainWindow.CollapsedSideBar.IsChecked = SavedData["CollapsedSideBar"].ToObject<bool>();
            }
        }

        public void Reset()
        {
            G.MainWindow.CollapsedSideBar.IsChecked = false;
        }

        public void Save(ref JObject SavedData)
        {
            SavedData["CollapsedSideBar"] = G.MainWindow.CollapsedSideBar.IsChecked;
        }
    }

    class WebPageModifier : IModifier
    {
        public void Apply()
        {
            //WebPage Style
            string Base64ImgBak = G.GetContentByMark(".\\resource\\webkit.css", "Background");
            switch (G.MainWindow.WebPageStyle.SelectedIndex)
            {
                case 0:
                    G.ReplaceByMark(".\\resource\\webkit.css", "BackgroundPos",
                        string.Format("background-position: {0}px -81px;", (G.MainWindow.CollapsedSideBar.IsChecked == true ? -48 : -240).ToString()));
                    G.ReplaceByMark(".\\resource\\webkit.css", "BackgroundSize",
                        string.Format("background-size: calc(100vw + {0}px) calc(100vh + 130px);", (G.MainWindow.CollapsedSideBar.IsChecked == true ? 48 : 240).ToString()));
                    break;
                case 1:
                    G.ReplaceByMark(".\\resource\\webkit.css", "BackgroundPos",
                        string.Format("background-position: {0}px -31px;", (G.MainWindow.CollapsedSideBar.IsChecked == true ? -48 : -240).ToString()));
                    G.ReplaceByMark(".\\resource\\webkit.css", "BackgroundSize",
                        string.Format("background-size: calc(100vw + {0}px) calc(100vh + 80px);", (G.MainWindow.CollapsedSideBar.IsChecked == true ? 48 : 240).ToString()));
                    break;
                case 2:
                    G.ReplaceByMark(".\\resource\\webkit.css", "WebPageStyleBG", "\n");
                    G.ReplaceByMark(".\\resource\\webkit.css", "WebPageStyle", "\n");
                    break;
            }
        }

        public void Load(JObject SavedData)
        {
            if (SavedData["WebPageStyle"] != null)
            {
                G.MainWindow.WebPageStyle.SelectedIndex = SavedData["WebPageStyle"].ToObject<int>();
            }
        }

        public void Reset()
        {
            G.MainWindow.WebPageStyle.SelectedIndex = 0;
        }

        public void Save(ref JObject SavedData)
        {
            SavedData["WebPageStyle"] = G.MainWindow.WebPageStyle.SelectedIndex;
        }
    }

    class BlurBrightnessModifier : IModifier
    {
        public void Apply()
        {
            //Blur and Brightness
            G.ReplaceByMark(".\\resource\\webkit.css", "GameListBlur",
                string.Format("filter: blur({0}px);", G.MainWindow.GameListBlur.Value.ToString()));
            G.ReplaceByMark(".\\resource\\webkit.css", "MainContentBlur",
                string.Format("filter: blur({0}px);", G.MainWindow.MainContentBlur.Value.ToString()));
            double Brightness = 1 - G.MainWindow.MainContentBrightness.Value / G.MainWindow.MainContentBrightness.Maximum;
            int StartAlpha = (int)Math.Min((Brightness * 2) * 255, 255.0);
            int EndAlpha = (int)Math.Max((Brightness - 0.5) * 255, 0);
            G.ReplaceByMark(".\\resource\\webkit.css", "MainContentBrightness",
                string.Format("background: radial-gradient(closest-side, {0}{1} 0%, {2}{3} 100%);",
                System.Drawing.ColorTranslator.ToHtml(G.MainContentBaseColor), StartAlpha.ToString("X2"),
                System.Drawing.ColorTranslator.ToHtml(G.MainContentBaseColor), EndAlpha.ToString("X2")));
            //Webpage Brightness
            try  //Maybe no background
            {
                string X16Value = Convert.ToString((int)(255 - G.MainWindow.WebPageBrightness.Value), 16);
                string Cnt = "background:linear-gradient(to right,#00000000 0%,#000000" + X16Value + " 2%);";
                G.ReplaceByMark(".\\resource\\webkit.css", "WebBGBrightness", Cnt);
            }
            catch (Exception) { }
        }

        public void Load(JObject SavedData)
        {
            JObject MyData = SavedData["BlurBrightness"] as JObject;
            if(MyData==null)
            {
                return;
            }
            G.MainWindow.GameListBlur.Value = MyData["GameListBlur"].ToObject<double>();
            G.MainWindow.MainContentBlur.Value = MyData["MainContentBlur"].ToObject<double>();
            G.MainWindow.MainContentBrightness.Value = MyData["MainContentBrightness"].ToObject<double>();
            G.MainWindow.WebPageBrightness.Value = MyData["WebPageBrightness"].ToObject<double>();
            G.MainContentBaseColor = System.Drawing.ColorTranslator.FromHtml(MyData["MainContentBaseColor"].ToString());
            var MediaColor = System.Windows.Media.Color.FromRgb(G.MainContentBaseColor.R,
                 G.MainContentBaseColor.G,
                 G.MainContentBaseColor.B);
            G.MainWindow.MainContentUpating.Background = new System.Windows.Media.SolidColorBrush(MediaColor);
        }

        public void Reset()
        {
            G.MainWindow.GameListBlur.Value = 5;
            G.MainWindow.MainContentBlur.Value = 10;
            G.MainWindow.MainContentBrightness.Value = 90;
            G.MainWindow.WebPageBrightness.Value = 153;
            var MediaColor = System.Windows.Media.Color.FromRgb(G.MainContentBaseColor.R,
                 G.MainContentBaseColor.G,
                 G.MainContentBaseColor.B);
            G.MainWindow.MainContentUpating.Background = new System.Windows.Media.SolidColorBrush(MediaColor);
        }

        public void Save(ref JObject SavedData)
        {
            JObject MyData = new JObject();
            MyData["GameListBlur"] = G.MainWindow.GameListBlur.Value;
            MyData["MainContentBlur"] = G.MainWindow.MainContentBlur.Value;
            MyData["MainContentBrightness"] = G.MainWindow.MainContentBrightness.Value;
            MyData["WebPageBrightness"] = G.MainWindow.WebPageBrightness.Value;
            MyData["MainContentBaseColor"] = System.Drawing.ColorTranslator.ToHtml(G.MainContentBaseColor);
            SavedData["BlurBrightness"] = MyData;
        }
    }

    class ShowLWDModifier : IModifier
    {
        public void Apply()
        {
            //Show LWD
            if (G.MainWindow.ShowLWD.IsChecked == true)
            {
                G.ReplaceByMark(".\\resource\\webkit.css", "LWD",
                    "\r\n\tbox-shadow: 1px 0px 6px 1px #000000;\r\n\tbackground-color: #17191bFF!important;\r\n\t");
            }
            else
            {
                G.ReplaceByMark(".\\resource\\webkit.css", "LWD",
                    "\r\n\tbackground-color: #17191b00!important;\r\n\t");
            }
        }

        public void Load(JObject SavedData)
        {
            if (SavedData["ShowLWD"] != null)
            {
                G.MainWindow.ShowLWD.IsChecked = SavedData["ShowLWD"].ToObject<bool>();
            }
        }

        public void Reset()
        {
            G.MainWindow.ShowLWD.IsChecked = true;
        }

        public void Save(ref JObject SavedData)
        {
            SavedData["ShowLWD"] = G.MainWindow.ShowLWD.IsChecked;
        }
    }

    class SpecialImgModifier : IModifier
    {
        public void Apply()
        {
            //Special Image
            string Base64ImgBak = G.GetContentByMark(".\\resource\\webkit.css", "Background");
            if (G.TGAImageReplaceList.Keys.Contains(G.MainBG))
            {
                if (G.TGAImageReplaceList[G.MainBG] != "")
                {
                    //Webkit Base64
                    try
                    {
                        var fileInfo = new FileInfo(G.TGAImageReplaceList[G.MainBG]);
                        if (fileInfo.Length > (500 * 1024))
                        {
                            MessageBox.Show(Application.Current.FindResource("ImgTooBigWarning").ToString(),
                                "TMC2", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }
                        string Base64Img = "";
                        byte[] ImgBytes = File.ReadAllBytes(G.TGAImageReplaceList[G.MainBG]);
                        Base64Img = "background:url(data:image/jpeg;base64," + Convert.ToBase64String(ImgBytes) + ");";
                        G.ReplaceByMark(".\\resource\\webkit.css", "Background", Base64Img);

                    }
                    catch (Exception) { }
                }
            }
            else
            {
                G.ReplaceByMark(".\\resource\\webkit.css", "Background", Base64ImgBak);
            }
            //MusicPlayerPanel
            {
                var m_TGA = new ImageTGA(".\\graphics\\JackMyth\\MusicPlayerImg.tga", true);
                G.ReplaceByMark(".\\resource\\layout\\musicplayerpanel.layout", "MusicPlayerLayout",
                    String.Format("\r\n\t\t\t\t1=\"image(x1-{0},y1-{1},x1,y1-76,graphics/JackMyth/MusicPlayerImg)\"\r\n\t\t\t\t",
                        m_TGA.Image.Width, m_TGA.Image.Height + 76),
                    "//", "");
            }
        }

        public void Load(JObject SavedData)
        {}

        public void Reset()
        {}

        public void Save(ref JObject SavedData)
        {}
    }

    class FontModifier : IModifier
    {
        public void Apply()
        {
            //Font
            {
                G.replaceFont("BaseFont", "Segoe UI");
                G.replaceFont("Light", "Segoe UI Light");
                G.replaceFont("SemiLight", "Segoe UI Semilight");
                G.replaceFont("SemiBold", "Segoe UI Semibold");
                G.replaceFont("Bold", "Segoe UI Bold");
                if (G.FontSettings.ContainsKey("STUIGlobal") && G.FontSettings["STUIGlobal"] != "")
                    G.ReplaceByMark(".\\resource\\webkit.css", "STUIFontGlobal", String.Format("\n\tfont-family:\"{0}\";\n\t", G.FontSettings["STUIGlobal"]));
                else
                    G.ReplaceByMark(".\\resource\\webkit.css", "STUIFontGlobal", "\n\t");
            }
            //Font Color
            {
                G.replaceFontColor("Uninstalled");
                G.replaceFontColor("Installed");
                G.replaceFontColor("Running");
                G.replaceFontColor("Updating");
                G.replaceFontColor("GameListSectionHeader");
            }
        }

        public void Load(JObject SavedData)
        {
            JObject FontSettings = SavedData["FontSettings"] as JObject;
            if(FontSettings == null)
            {
                return;
            }
            G.FontSettings.Clear();
            foreach (var KV in FontSettings)
            {
                G.FontSettings[KV.Key] = KV.Value.ToString();
            }
        }

        public void Reset()
        {
            //Fonts
            G.FontSettings.Clear();
        }

        public void Save(ref JObject SavedData)
        {
            JObject FontSettings = new JObject();
            foreach (var KV in G.FontSettings)
            {
                FontSettings[KV.Key] = KV.Value;
            }
            SavedData["FontSettings"] = FontSettings;
        }
    }
}
