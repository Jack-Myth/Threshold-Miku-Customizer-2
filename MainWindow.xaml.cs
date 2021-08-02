using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Threshold_Miku_Customizer_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
  
    public partial class MainWindow : Window
    {
        [DllImport("kernel32")] 
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        //Special Image
        const string MainBG = "MainBG";

        System.Drawing.Color MainContentBaseColor;

        //Image:Preview
        private static readonly Dictionary<string, string> TGAImageList = new Dictionary<string, string>
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

        //Image:Res
        private static Dictionary<string, string> ImageResList = new Dictionary<string, string>();

        private Dictionary<string, string> TGAImageReplaceList = new Dictionary<string, string>();

        private Dictionary<string, string> FontSettings = new Dictionary<string, string>();

        public delegate void OnReplacingFile(string NewFile, string FileToBeReplaced);
        public MainWindow()
        {
            InitializeComponent();
            if(!System.IO.Directory.Exists(".\\Customization"))
            {
                MessageBox.Show(Application.Current.FindResource("NotSkinFolderWarning").ToString()
                    , "Error",MessageBoxButton.OK,MessageBoxImage.Stop);
                Environment.Exit(0);
            }

            {
                StringBuilder ColorOutput=new StringBuilder();
                ColorOutput.Length = 256;
                GetPrivateProfileString("Config",
                    "MainContentBaseColor",
                    "#000000",
                    ColorOutput,
                    ColorOutput.Length,
                    ".\\Customization\\TMC2.ini");
                MainContentBaseColor = System.Drawing.ColorTranslator.FromHtml(ColorOutput.ToString());
                var MediaColor = System.Windows.Media.Color.FromRgb(MainContentBaseColor.R,
                 MainContentBaseColor.G,
                 MainContentBaseColor.B);
                this.MainContentUpating.Background = new SolidColorBrush(MediaColor);
            }
            if(!System.IO.Directory.Exists(".\\Customization\\Collapsed Sidebar"))
            {
                this.CollapsedSideBar.IsEnabled = false;
            }
            else
            {
                if (System.IO.Directory.Exists(".\\Customization\\Backup\\Collapsed Sidebar"))
                    this.CollapsedSideBar.IsChecked = true;
            }
            if (System.IO.File.Exists(".\\Customization\\Backup\\WebPageStyle\\.CustomizerCfg"))
            {
                string SelectIndex = System.IO.File.ReadAllText(".\\Customization\\Backup\\WebPageStyle\\.CustomizerCfg");
                int SelectIndexInt = 0;
                int.TryParse(SelectIndex, out SelectIndexInt);
                this.WebPageStyle.SelectedIndex = SelectIndexInt;
            }
            foreach (string TGAImage in TGAImageList.Keys)
            {
                this.ImgSelector.Items.Add(TGAImage);
                StringBuilder sres = new StringBuilder(255);
                GetPrivateProfileString("SuggestRes", TGAImage, "", sres, 255, ".\\Previews\\Info.ini");
                if (sres.ToString() != "")
                    ImageResList.Add(TGAImage, sres.ToString());
            }
        }

        private void ImgSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string CurTGAName = (string)this.ImgSelector.SelectedItem;
            if (!File.Exists(string.Format(".\\Previews\\{0}.jpg", TGAImageList[CurTGAName])))
            {
                this.PreviewImg.Source = new BitmapImage();
                return;
            }
            var PreviewBitmap= new BitmapImage(new Uri(System.IO.Path.GetFullPath(string.Format(".\\Previews\\{0}.jpg", TGAImageList[CurTGAName]))));
            //this.PreviewImg.Height = PreviewImg.Width * PreviewBitmap.Height / PreviewBitmap.Width;
            this.PreviewImg.Source = PreviewBitmap;
            if (TGAImageReplaceList.ContainsKey(CurTGAName))
                this.ReplacedByLabel.Content = string.Format(Application.Current.FindResource("ReplacedBy").ToString()
                    , System.IO.Path.GetFileName(TGAImageReplaceList[CurTGAName]));
            /*else if (System.IO.File.Exists(string.Format(".\\graphics\\JackMyth\\{0}.tmc2.bak", CurTGAName)))
                this.ReplacedByLabel.Content = Application.Current.FindResource("BGReplaced").ToString();*/
            else if (ImageResList.ContainsKey(CurTGAName))
                this.ReplacedByLabel.Content = string.Format(Application.Current.FindResource("SuggestRes").ToString(),
                    ImageResList[CurTGAName]);
            else
                this.ReplacedByLabel.Content = "";
            this.NewBackground.IsEnabled = true;
        }

        private void NewBackground_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "Supported Image|*.jpg;*.png";
            if (ofd.ShowDialog()!=true)
                return;
            TGAImageReplaceList[(string)ImgSelector.SelectedItem] = ofd.FileName;
            this.ReplacedByLabel.Content = string.Format("ReplacedBy:{0}", System.IO.Path.GetFileName(ofd.FileName));
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            //Image
            foreach(KeyValuePair<string,string> TGAReplaceItem in TGAImageReplaceList)
            {
                if (TGAReplaceItem.Value == "")
                    continue;
                string TGAPartPath= string.Format(".\\graphics\\JackMyth\\{0}", TGAReplaceItem.Key);
                if(!System.IO.File.Exists(string.Format("{0}.tmc2.bak",TGAPartPath))&&File.Exists(string.Format("{0}.tga", TGAPartPath)))
                    System.IO.File.Copy(string.Format("{0}.tga", TGAPartPath), string.Format("{0}.tmc2.bak", TGAPartPath));
                var m_TGA = new ImageTGA();
                m_TGA.Image = new System.Drawing.Bitmap(TGAReplaceItem.Value);
                System.IO.File.Delete(string.Format("{0}.tga", TGAPartPath));
                m_TGA.SaveImage(string.Format("{0}.tga", TGAPartPath));
            }

            //Collapsed Sidebar
            if (CollapsedSideBar.IsChecked == true)
            {
                BackupReplaceingFiles(".\\Customization\\Collapsed Sidebar", ".\\Customization\\Backup\\Collapsed Sidebar");
            }
            else
            {
                if (System.IO.Directory.Exists(".\\Customization\\Backup\\Collapsed Sidebar"))
                {
                    CopyDirectory(".\\Customization\\Backup\\Collapsed Sidebar", ".\\");
                    Directory.Delete(".\\Customization\\Backup\\Collapsed Sidebar",true);
                }
            }

            //WebPage Style
            string Base64ImgBak = GetContentByMark(".\\resource\\webkit.css", "Background");
            if (System.IO.Directory.Exists(".\\Customization\\Backup\\WebPageStyle"))
            {
                File.Delete(".\\Customization\\Backup\\WebPageStyle\\.CustomizerCfg");
                CopyDirectory(".\\Customization\\Backup\\WebPageStyle", ".\\");
                Directory.Delete(".\\Customization\\Backup\\WebPageStyle", true);
            }
            Directory.CreateDirectory(".\\Customization\\Backup\\WebPageStyle\\resource");
            File.Copy(".\\resource\\webkit.css", ".\\Customization\\Backup\\WebPageStyle\\resource\\webkit.css");
            switch (WebPageStyle.SelectedIndex)
            {
                case 0:
                    ReplaceByMark(".\\resource\\webkit.css", "BackgroundPos",
                        string.Format("background-position: {0}px -81px;", (CollapsedSideBar.IsChecked == true ? -48 : -240).ToString()));
                    ReplaceByMark(".\\resource\\webkit.css", "BackgroundSize",
                        string.Format("background-size: calc(100vw + {0}px) calc(100vh + 130px);", (CollapsedSideBar.IsChecked == true ? 48 : 240).ToString()));
                    break;
                case 1:
                    ReplaceByMark(".\\resource\\webkit.css", "BackgroundPos", 
                        string.Format("background-position: {0}px -31px;",(CollapsedSideBar.IsChecked == true ? -48 : -240).ToString()));
                    ReplaceByMark(".\\resource\\webkit.css", "BackgroundSize",
                        string.Format("background-size: calc(100vw + {0}px) calc(100vh + 80px);", (CollapsedSideBar.IsChecked == true ? 48 : 240).ToString()));
                    File.WriteAllText(".\\Customization\\Backup\\WebPageStyle\\.CustomizerCfg", "1");
                    break;
                case 2:
                    ReplaceByMark(".\\resource\\webkit.css", "WebPageStyleBG", "\n");
                    ReplaceByMark(".\\resource\\webkit.css", "WebPageStyle", "\n");
                    File.WriteAllText(".\\Customization\\Backup\\WebPageStyle\\.CustomizerCfg", "2");
                    break;
            }

            {
                //Blur and Brightness
                ReplaceByMark(".\\resource\\webkit.css", "GameListBlur",
                    string.Format("filter: blur({0}px);", this.GameListBlur.Value.ToString()));
                ReplaceByMark(".\\resource\\webkit.css", "MainContentBlur",
                    string.Format("filter: blur({0}px);", this.MainContentBlur.Value.ToString()));
                double Brightness = 1 - this.MainContentBrightness.Value / this.MainContentBrightness.Maximum;
                int StartAlpha = (int)Math.Min((Brightness * 2)*255,255.0);
                int EndAlpha = (int)Math.Max((Brightness - 0.5) * 255, 0);
                ReplaceByMark(".\\resource\\webkit.css", "MainContentBrightness",
                    string.Format("background: radial-gradient(closest-side, {0}{1} 0%, {2}{3} 100%);",
                    System.Drawing.ColorTranslator.ToHtml(MainContentBaseColor),StartAlpha.ToString("X2"),
                    System.Drawing.ColorTranslator.ToHtml(MainContentBaseColor), EndAlpha.ToString("X2")));
            }
            //Show LWD
            if(this.ShowLWD.IsChecked==true)
            {
                ReplaceByMark(".\\resource\\webkit.css", "LWD",
                    "\r\n\tbox-shadow: 1px 0px 6px 1px #000000;\r\n\tbackground-color: #17191bFF!important;\r\n\t");
            }
            else
            {
                ReplaceByMark(".\\resource\\webkit.css", "LWD", 
                    "\r\n\tbackground-color: #17191b00!important;\r\n\t");
            }

            if(this.CollapsedSideBar.IsChecked==true)
            {
                for(int i=0;i<3;i++)
                {
                    ReplaceByMark(".\\resource\\webkit.css", "CSide"+i.ToString(),
                        "\r\n\tbackground-position: -48px -31px;\r\n\tbackground-size: calc(100% + 48px) calc(100% + 80px);\r\n\t");
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    ReplaceByMark(".\\resource\\webkit.css", "CSide" + i.ToString(),
                        "\r\n\tbackground-position: -240px -31px;\r\n\tbackground-size: calc(100% + 240px) calc(100% + 80px);\r\n\t");
                }
            }

            //Special Image
            if (TGAImageReplaceList.Keys.Contains(MainBG))
            {
                if (TGAImageReplaceList[MainBG] != "")
                {
                    //Webkit Base64
                    //Backup First
                    if (!Directory.Exists(".\\Customization\\Backup\\WebPageStyle"))
                    {
                        Directory.CreateDirectory(".\\Customization\\Backup\\WebPageStyle\\resource");
                        File.Copy(".\\resource\\webkit.css", ".\\Customization\\Backup\\WebPageStyle\\resource\\webkit.css");
                        File.WriteAllText(".\\Customization\\Backup\\WebPageStyle\\.CustomizerCfg", WebPageStyle.SelectedIndex.ToString());
                    }
                    try
                    {
                        var fileInfo = new FileInfo(TGAImageReplaceList[MainBG]);
                        if (fileInfo.Length > (500 * 1024))
                        {
                            MessageBox.Show(Application.Current.FindResource("ImgTooBigWarning").ToString(),
                                "TMC2", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }
                        string Base64Img = "";
                        byte[] ImgBytes = File.ReadAllBytes(TGAImageReplaceList[MainBG]);
                        Base64Img = "background:url(data:image/jpeg;base64," + Convert.ToBase64String(ImgBytes) + ");";
                        ReplaceByMark(".\\resource\\webkit.css", "Background", Base64Img);

                    }
                    catch (Exception) { }
                }
            }
            else
            {
                ReplaceByMark(".\\resource\\webkit.css", "Background", Base64ImgBak);
            }
            //MusicPlayerPanel
            {
                var m_TGA = new ImageTGA(".\\graphics\\JackMyth\\MusicPlayerImg.tga",true);
                ReplaceByMark(".\\resource\\layout\\musicplayerpanel.layout", "MusicPlayerLayout",
                    String.Format("\r\n\t\t\t\t1=\"image(x1-{0},y1-{1},x1,y1-76,graphics/JackMyth/MusicPlayerImg)\"\r\n\t\t\t\t",
                        m_TGA.Image.Width, m_TGA.Image.Height+76),
                    "//", "");
            }

            //Font
            {
                replaceFont("BaseFont", "Segoe UI");
                replaceFont("Light", "Segoe UI Light");
                replaceFont("SemiLight", "Segoe UI Semilight");
                replaceFont("SemiBold", "Segoe UI Semibold");
                replaceFont("Bold", "Segoe UI Bold");
                if (FontSettings.ContainsKey("STUIGlobal")&& FontSettings["STUIGlobal"]!="")
                    ReplaceByMark(".\\resource\\webkit.css", "STUIFontGlobal", String.Format("\n\tfont-family:\"{0}\";\n\t", FontSettings["STUIGlobal"]));
                else
                    ReplaceByMark(".\\resource\\webkit.css", "STUIFontGlobal", "\n\t");
            }
            //Font Color
            {
                replaceFontColor("Uninstalled");
                replaceFontColor("Installed");
                replaceFontColor("Running");
                replaceFontColor("Updating");
                replaceFontColor("GameListSectionHeader");
            }

            //Webpage Brightness
            try  //Maybe no background
            {
                string X16Value = Convert.ToString((int)(255-this.WebPageBrightness.Value), 16);
                string Cnt = "background:linear-gradient(to right,#00000000 0%,#000000" + X16Value + " 2%);";
                ReplaceByMark(".\\resource\\webkit.css", "WebBGBrightness", Cnt);
            }
            catch (Exception) { }

            if (Directory.Exists(".\\Customization\\Backup"))
            {
                string v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                File.Create(".\\Customization\\Backup\\"+ v).Close();
            }
            MessageBox.Show(Application.Current.FindResource("ApplySucceed").ToString());
        }


        private void replaceFont(string DicID,string DefaultFont)
        {
            string FontID = DicID.ToLower();
            string ReplaceContent = "\n\t" + FontID + "=\"";
            if (FontSettings.ContainsKey(DicID) && FontSettings[DicID] !="")
                ReplaceContent += FontSettings[DicID];
            else
                ReplaceContent += DefaultFont;
            ReplaceContent += "\"\n\t";
            ReplaceByMark(".\\resource\\styles\\steam.styles", DicID, ReplaceContent, "//", "");
        }

        private void replaceFontColor(string DicID)
        {
            if (!FontSettings.ContainsKey(DicID))
                return;
            string ReplaceContent = string.Format("color: {0};", FontSettings[DicID]);
            ReplaceByMark(".\\resource\\webkit.css", DicID, ReplaceContent);
        }

        public static void CopyDirectory(string srcPath, string destPath, OnReplacingFile onReplacingFileDelegate=null)
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
                        if (onReplacingFileDelegate!=null&&File.Exists(destPath + "\\" + i.Name))
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

        private static void BackupReplaceingFiles(string SourceDir,string BcakupToDir)
        {
            if (!System.IO.Directory.Exists(BcakupToDir))
            {
                Directory.CreateDirectory(BcakupToDir);
                CopyDirectory(SourceDir, ".\\", (string NewFile, string FileToBeReplaced) =>
                  {
                      //Make a backup
                      string mFileToBeReplaced = System.IO.Path.GetFullPath(FileToBeReplaced);
                      string mRoot = System.IO.Path.GetFullPath(".\\");
                      string Target = BcakupToDir+ "\\" + mFileToBeReplaced.Substring(mRoot.Length);
                      Directory.CreateDirectory(Target.Substring(0,Target.LastIndexOf("\\")));
                      File.Copy(FileToBeReplaced, Target,true); //True is not necessary
                  });
            }
            else
            {
                CopyDirectory(SourceDir, ".\\");
            }
        }

        private static void ReplaceByMark(string FilePath,string Mark,string NewContent,string PreComment="/*",string PostComment="*/")
        {
            string FileContent = File.ReadAllText(FilePath);
            string BeginMark = PreComment + "TMC2:Begin" + Mark + PostComment;
            string EndMark = PreComment + "TMC2:End" + Mark + PostComment;
            int BeginIndex = FileContent.IndexOf(BeginMark);
            if (BeginIndex<0)
                throw new KeyNotFoundException("Mark:" + Mark + " not found at File:" + FilePath);
            int EndIndex = FileContent.IndexOf(EndMark);
            if (EndIndex < 0)
                throw new OverflowException("Mark:" + Mark + " is not closed at File:" + FilePath);
            File.WriteAllText(FilePath, FileContent.Substring(0, BeginIndex + BeginMark.Length) + NewContent + FileContent.Substring(EndIndex));
        }

        private static string GetContentByMark(string FilePath,string Mark, string PreComment = "/*", string PostComment = "*/")
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

        void ResetAll()
        {
            if (MessageBox.Show(Application.Current.FindResource("ResetToDefault").ToString(), "TMC2", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                return;
            //Reset All Images
            //Image
            foreach (string TGAItem in TGAImageList.Keys)
            {
                string TGAPartPath = string.Format(".\\graphics\\JackMyth\\{0}", TGAItem);
                if (System.IO.File.Exists(string.Format("{0}.tmc2.bak", TGAPartPath)))
                {
                    File.Delete(string.Format("{0}.tga", TGAPartPath));
                    System.IO.File.Move(string.Format("{0}.tmc2.bak", TGAPartPath), string.Format("{0}.tga", TGAPartPath));
                }
            }

            //Fonts
            FontSettings.Clear();

            TGAImageReplaceList.Clear();
            TGAImageReplaceList.Add(MainBG, "");
            this.CollapsedSideBar.IsChecked = false;
            this.WebPageStyle.SelectedIndex = 0;
            this.GameListBlur.Value = 5;
            this.MainContentBlur.Value = 10;
            this.MainContentBrightness.Value = 90;
            this.ShowLWD.IsChecked = true;
            this.WebPageBrightness.Value = 153;

            ApplyButton_Click(null, null);
            TGAImageReplaceList.Clear();
        }

        private void MenuItemResetAll_Click(object sender, EventArgs e)
        {
            ResetAll();
        }

        private void MenuItemCurrentPicture_Click(object sender, EventArgs e)
        {
            var TGAItem = (string)ImgSelector.SelectedItem;
            string TGAPartPath = string.Format(".\\graphics\\JackMyth\\{0}", TGAItem);
            if (System.IO.File.Exists(string.Format("{0}.tmc2.bak", TGAPartPath)))
            {
                File.Delete(string.Format("{0}.tga", TGAPartPath));
                System.IO.File.Move(string.Format("{0}.tmc2.bak", TGAPartPath), string.Format("{0}.tga", TGAPartPath));
            }
            if((string)ImgSelector.SelectedItem==MainBG&&Directory.Exists(".\\Customization\\Backup\\WebPageStyle"))
                CopyDirectory(".\\Customization\\Backup\\WebPageStyle", ".\\");

            //MusicPlayerPanel
            {
                var m_TGA = new ImageTGA(".\\graphics\\JackMyth\\MusicPlayerImg.tga", true);
                ReplaceByMark(".\\resource\\layout\\musicplayerpanel.layout", "MusicPlayerLayout",
                    String.Format("\r\n\t\t\t\t1=\"image(x1-{0},y1-{1},x1,y1-76,graphics/JackMyth/MusicPlayerImg)\"\r\n\t\t\t\t",
                        m_TGA.Image.Width, m_TGA.Image.Height + 76),
                    "//", "");
            }

            TGAImageReplaceList.Remove((string)ImgSelector.SelectedItem);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetButton.ContextMenu.IsOpen = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new FontsSetting(ref FontSettings).ShowDialog();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Jack-Myth/Threshold-Miku/");
        }

        private void MainContentUpating_Click(object sender, RoutedEventArgs e)
        {
            var ColorPicker = new System.Windows.Forms.ColorDialog();
            if (ColorPicker.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MainContentBaseColor = System.Drawing.Color.FromArgb(255, ColorPicker.Color);
            }
            var MediaColor = System.Windows.Media.Color.FromRgb(MainContentBaseColor.R,
                 MainContentBaseColor.G,
                 MainContentBaseColor.B);
            this.MainContentUpating.Background = new SolidColorBrush(MediaColor);
        }
    }
}
