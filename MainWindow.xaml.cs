﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        //Special Image
        const string MainBG = "MainBG";

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
            { "SystemInfo","SystemInfo"},
            { "UseOffline","UseOffline"}
        };

        private Dictionary<string, string> TGAImageReplaceList = new Dictionary<string, string>();

        public delegate void OnReplacingFile(string NewFile, string FileToBeReplaced);
        public MainWindow()
        {
            InitializeComponent();
            if(!System.IO.Directory.Exists(".\\Customization"))
            {
                MessageBox.Show("Can't find Customization Folder, Please put the Customizer into Threshold Miku folder."
                    ,"Error",MessageBoxButton.OK,MessageBoxImage.Stop);
                Environment.Exit(0);
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
            if (!System.IO.Directory.Exists(".\\Customization\\WebPage without URL"))
                ((ComboBoxItem)this.WebPageStyle.Items[1]).IsEnabled = false;
            if (!System.IO.Directory.Exists(".\\Customization\\Origin WebPage"))
                ((ComboBoxItem)this.WebPageStyle.Items[2]).IsEnabled = false;
            if (System.IO.File.Exists(".\\Customization\\Backup\\WebPageStyle\\.CustomizerCfg"))
            {
                string SelectIndex = System.IO.File.ReadAllText(".\\Customization\\Backup\\WebPageStyle\\.CustomizerCfg");
                int SelectIndexInt = 0;
                int.TryParse(SelectIndex, out SelectIndexInt);
                this.WebPageStyle.SelectedIndex = SelectIndexInt;
            }
            foreach(string TGAImage in TGAImageList.Keys)
                this.ImgSelector.Items.Add(TGAImage);
        }

        private void ImgSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string CurTGAName = (string)this.ImgSelector.SelectedItem;
            var PreviewBitmap= new BitmapImage(new Uri(System.IO.Path.GetFullPath(string.Format(".\\Previews\\{0}.jpg", TGAImageList[CurTGAName]))));
            //this.PreviewImg.Height = PreviewImg.Width * PreviewBitmap.Height / PreviewBitmap.Width;
            this.PreviewImg.Source = PreviewBitmap;
            if (TGAImageReplaceList.ContainsKey(CurTGAName))
                this.ReplacedByLabel.Content = string.Format("Will be replaced by:{0}", System.IO.Path.GetFileName(TGAImageReplaceList[CurTGAName]));
            else if (System.IO.File.Exists(string.Format(".\\graphics\\JackMyth\\{0}.tmc2.bak", CurTGAName)))
                this.ReplacedByLabel.Content = "This background has been replaced";
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
                string TGAPartPath= string.Format(".\\graphics\\JackMyth\\{0}", TGAReplaceItem.Key);
                if(!System.IO.File.Exists(string.Format("{0}.tmc2.bak",TGAPartPath)))
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
            switch(WebPageStyle.SelectedIndex)
            {
                case 0:
                    if (System.IO.Directory.Exists(".\\Customization\\Backup\\WebPageStyle"))
                    {
                        File.Delete(".\\Customization\\Backup\\WebPageStyle\\.CustomizerCfg");
                        CopyDirectory(".\\Customization\\Backup\\WebPageStyle", ".\\");
                        Directory.Delete(".\\Customization\\Backup\\WebPageStyle", true);
                    }
                    break;
                case 1:
                    BackupReplaceingFiles(".\\Customization\\WebPage without URL", ".\\Customization\\Backup\\WebPageStyle");
                    File.WriteAllText(".\\Customization\\Backup\\WebPageStyle\\.CustomizerCfg", "1");
                    break;
                case 2:
                    BackupReplaceingFiles(".\\Customization\\Origin WebPage", ".\\Customization\\Backup\\WebPageStyle");
                    File.WriteAllText(".\\Customization\\Backup\\WebPageStyle\\.CustomizerCfg", "1");
                    break;
            }

            //Special Image
            if (TGAImageReplaceList.Keys.Contains(MainBG))
            {
                //SteamUI
                string preSteamUIBG = ".\\steamui\\skins\\Threshold Miku\\images\\MainBG";
                if (!File.Exists(preSteamUIBG + ".tmc2.bak"))
                    File.Copy(preSteamUIBG + ".jpg", preSteamUIBG + ".tmc2.bak");
                File.Copy(TGAImageReplaceList[MainBG], preSteamUIBG + ".jpg", true);

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
                        MessageBox.Show("MainBG Is Too Big, WebPage Background may not take effect.",
                            "Image Too Big", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    string Base64Img = "";
                    byte[] ImgBytes = File.ReadAllBytes(TGAImageReplaceList[MainBG]);
                    Base64Img = "background:url(data:image/jpeg;base64," + Convert.ToBase64String(ImgBytes) + ");";
                    ReplaceByMark(".\\resource\\webkit.css", "Background", Base64Img);
                }
                catch (Exception) { }
            }

            //Call Install.cmd
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
            psi.FileName = ".\\Install.cmd";
            psi.Arguments = "CalledByCustomizer";
            if (this.CreateShortcut.IsChecked != true)
                psi.Arguments += " WithoutShortcut";
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(psi);
            p.WaitForExit();
            MessageBox.Show("Apply Succeed! Restart Steam to take effect");
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

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
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

            //Special Image
            //SteamUI
            string preSteamUIBG = ".\\steamui\\skins\\Threshold Miku\\images\\MainBG";
            if (File.Exists(preSteamUIBG + ".tmc2.bak"))
            {
                File.Delete(preSteamUIBG + ".jpg");
                File.Move(preSteamUIBG + ".tmc2.bak", preSteamUIBG + ".jpg");
            }
            TGAImageReplaceList.Clear();
            this.CollapsedSideBar.IsChecked = false;
            this.WebPageStyle.SelectedIndex = 0;
            ApplyButton_Click(null, null);
        }
    }
}
