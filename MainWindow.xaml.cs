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

        public delegate void OnReplacingFile(string NewFile, string FileToBeReplaced);
        public MainWindow()
        {
            InitializeComponent();
            G.MainWindow = this;
            if (!System.IO.Directory.Exists(".\\Customization"))
            {
                MessageBox.Show(Application.Current.FindResource("NotSkinFolderWarning").ToString()
                    , "Error",MessageBoxButton.OK,MessageBoxImage.Stop);
                Environment.Exit(0);
            }

            {
                StringBuilder ColorOutput=new StringBuilder();
                ColorOutput.Length = 256;
                G.GetPrivateProfileString("Config",
                    "MainContentBaseColor",
                    "#000000",
                    ColorOutput,
                    ColorOutput.Length,
                    ".\\Customization\\TMC2.ini");
                G.MainContentBaseColor = System.Drawing.ColorTranslator.FromHtml(ColorOutput.ToString());
                var MediaColor = System.Windows.Media.Color.FromRgb(G.MainContentBaseColor.R,
                 G.MainContentBaseColor.G,
                 G.MainContentBaseColor.B);
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
            foreach (string TGAImage in G.TGAImageList.Keys)
            {
                this.ImgSelector.Items.Add(TGAImage);
                StringBuilder sres = new StringBuilder(255);
                G.GetPrivateProfileString("SuggestRes", TGAImage, "", sres, 255, ".\\Previews\\Info.ini");
                if (sres.ToString() != "")
                    G.ImageResList.Add(TGAImage, sres.ToString());
            }
        }

        private void ImgSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string CurTGAName = (string)this.ImgSelector.SelectedItem;
            if (!File.Exists(string.Format(".\\Previews\\{0}.jpg", G.TGAImageList[CurTGAName])))
            {
                this.PreviewImg.Source = new BitmapImage();
                return;
            }
            var PreviewBitmap= new BitmapImage(new Uri(System.IO.Path.GetFullPath(string.Format(".\\Previews\\{0}.jpg", TGAImageList[CurTGAName]))));
            //this.PreviewImg.Height = PreviewImg.Width * PreviewBitmap.Height / PreviewBitmap.Width;
            this.PreviewImg.Source = PreviewBitmap;
            if (G.TGAImageReplaceList.ContainsKey(CurTGAName))
                this.ReplacedByLabel.Content = string.Format(Application.Current.FindResource("ReplacedBy").ToString()
                    , System.IO.Path.GetFileName(G.TGAImageReplaceList[CurTGAName]));
            /*else if (System.IO.File.Exists(string.Format(".\\graphics\\JackMyth\\{0}.tmc2.bak", CurTGAName)))
                this.ReplacedByLabel.Content = Application.Current.FindResource("BGReplaced").ToString();*/
            else if (G.ImageResList.ContainsKey(CurTGAName))
                this.ReplacedByLabel.Content = string.Format(Application.Current.FindResource("SuggestRes").ToString(),
                    G.ImageResList[CurTGAName]);
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
            G.TGAImageReplaceList[(string)ImgSelector.SelectedItem] = ofd.FileName;
            this.ReplacedByLabel.Content = string.Format("ReplacedBy:{0}", System.IO.Path.GetFileName(ofd.FileName));
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(IModifier Modifier in G.ModifierList)
            {
                Modifier.Apply();
            }
            if (Directory.Exists(".\\Customization\\Backup"))
            {
                string v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                StreamWriter VerFile = File.CreateText(".\\Customization\\Backup\\.ver");
                VerFile.Write(v);
                VerFile.Close();
            }
            MessageBox.Show(Application.Current.FindResource("ApplySucceed").ToString());
        }

        void ResetAll()
        {
            if (MessageBox.Show(Application.Current.FindResource("ResetToDefault").ToString(), "TMC2", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                return;

            foreach (IModifier Modifier in G.ModifierList)
            {
                Modifier.Reset();
            }

            ApplyButton_Click(null, null);
            G.TGAImageReplaceList.Clear();
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
                G.CopyDirectory(".\\Customization\\Backup\\WebPageStyle", ".\\");

            //MusicPlayerPanel
            {
                var m_TGA = new ImageTGA(".\\graphics\\JackMyth\\MusicPlayerImg.tga", true);
                G.ReplaceByMark(".\\resource\\layout\\musicplayerpanel.layout", "MusicPlayerLayout",
                    String.Format("\r\n\t\t\t\t1=\"image(x1-{0},y1-{1},x1,y1-76,graphics/JackMyth/MusicPlayerImg)\"\r\n\t\t\t\t",
                        m_TGA.Image.Width, m_TGA.Image.Height + 76),
                    "//", "");
            }

            G.TGAImageReplaceList.Remove((string)ImgSelector.SelectedItem);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetButton.ContextMenu.IsOpen = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new FontsSetting(ref G.FontSettings).ShowDialog();
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
                G.MainContentBaseColor = System.Drawing.Color.FromArgb(255, ColorPicker.Color);
            }
            var MediaColor = System.Windows.Media.Color.FromRgb(G.MainContentBaseColor.R,
                 G.MainContentBaseColor.G,
                 G.MainContentBaseColor.B);
            this.MainContentUpating.Background = new SolidColorBrush(MediaColor);
        }
    }
}
