using Microsoft.SqlServer.Server;
using Newtonsoft.Json.Linq;
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
            RenderOptions.SetBitmapScalingMode(this.PreviewImg, BitmapScalingMode.Fant);
            ToolTipService.SetIsEnabled(this.PreviewImg, false);
        }

        private void UpdateReplaceLabel()
        {
            if(this.ImgSelector.SelectedItem==null)
            {
                return;
            }
            string CurTGAName = (string)this.ImgSelector.SelectedItem;
            this.ReplacedByLabel.Cursor = Cursors.Arrow;
            this.ReplacedByLabel.Foreground = Brushes.Black;
            this.ReplacedByLabel.TextDecorations = null;
            if (G.TGAImageReplaceList.ContainsKey(CurTGAName))
            {
                this.ReplacedByLabel.Text = string.Format(Application.Current.FindResource("ReplacedBy").ToString()
                    , System.IO.Path.GetFileName(G.TGAImageReplaceList[CurTGAName]));
                this.ReplacedByLabel.Cursor = Cursors.Hand;
                this.ReplacedByLabel.TextDecorations = TextDecorations.Underline;
                this.ReplacedByLabel.Foreground = Brushes.Blue;
            }
            else if (G.ImageResList.ContainsKey(CurTGAName))
            {
                this.ReplacedByLabel.Text = string.Format(Application.Current.FindResource("SuggestRes").ToString(),
                    G.ImageResList[CurTGAName]);
            }
            else
            {
                this.ReplacedByLabel.Text = "";
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
            var PreviewBitmap= new BitmapImage(new Uri(System.IO.Path.GetFullPath(string.Format(".\\Previews\\{0}.jpg", G.TGAImageList[CurTGAName]))));
            //this.PreviewImg.Height = PreviewImg.Width * PreviewBitmap.Height / PreviewBitmap.Width;
            this.PreviewImg.Source = PreviewBitmap;
            ToolTipService.SetIsEnabled(this.PreviewImg, true);
            this.PreviewFullImg.Source = PreviewBitmap;
            this.NewBackground.IsEnabled = true;
            UpdateReplaceLabel();
        }

        private void NewBackground_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "JPEG Image|*.jpg";
            if (ofd.ShowDialog()!=true)
                return;
            G.TGAImageReplaceList[(string)ImgSelector.SelectedItem] = ofd.FileName;
            UpdateReplaceLabel();
            G.PendingSave = true;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(".\\Customization\\Backup\\");
            File.Copy(".\\Customization\\.ver", ".\\Customization\\Backup\\.ver", true);
            foreach (IModifier Modifier in G.ModifierList)
            {
                Modifier.Apply();
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
            if(ImgSelector.SelectedItem==null)
            {
                return;
            }
            var TGAItem = (string)ImgSelector.SelectedItem;
            string TGAPartPath = string.Format(".\\graphics\\JackMyth\\{0}", TGAItem);
            if (System.IO.File.Exists(string.Format("{0}.tmc2.bak", TGAPartPath)))
            {
                File.Delete(string.Format("{0}.tga", TGAPartPath));
                System.IO.File.Move(string.Format("{0}.tmc2.bak", TGAPartPath), string.Format("{0}.tga", TGAPartPath));
            }
            if((string)ImgSelector.SelectedItem==G.MainBG&&Directory.Exists(".\\Customization\\Backup\\WebPageStyle"))
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
            UpdateReplaceLabel();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetButton.ContextMenu.IsOpen = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new FontsSetting(ref G.FontSettings).ShowDialog();
            G.PendingSave = true;
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
            G.PendingSave = true;
        }

        bool AskForSave()
        {
            JObject PendingSave = new JObject();
            foreach (IModifier Modifier in G.ModifierList)
            {
                Modifier.Save(ref PendingSave);
            }
            string PendingSaveString = Newtonsoft.Json.JsonConvert.SerializeObject(PendingSave);
            var ofd = new Microsoft.Win32.SaveFileDialog();
            ofd.Filter = "Saved JSON|*.json";
            if (ofd.ShowDialog() != true)
                return false;
            StreamWriter PendingSaveWriter = File.CreateText(ofd.FileName);
            PendingSaveWriter.Write(PendingSaveString);
            PendingSaveWriter.Close();
            G.PendingSave = false;
            return true;
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            AskForSave();
        }

        private void LoadSettings_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "Saved JSON|*.json";
            if (ofd.ShowDialog() != true)
                return;
            string LoadedStr = File.ReadAllText(ofd.FileName);
            var Serializer = new Newtonsoft.Json.JsonSerializer();
            JObject LoadedObj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(LoadedStr);
            if(LoadedObj==null)
            {
                MessageBox.Show(Application.Current.FindResource("LoadSettingsFailed").ToString(), "TMC2", MessageBoxButton.OK, MessageBoxImage.Error); ;
                return;
            }
            foreach (IModifier Modifier in G.ModifierList)
            {
                Modifier.Load(LoadedObj);
            }
            UpdateReplaceLabel();
        }

        private void ReplacedByLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string CurTGAName = (string)this.ImgSelector.SelectedItem;
            if (CurTGAName!=null&&
                G.TGAImageReplaceList.ContainsKey(CurTGAName)&&
                File.Exists(G.TGAImageReplaceList[CurTGAName]))
            {
                string Arg = $"/select,\"{G.TGAImageReplaceList[CurTGAName]}\"";
                System.Diagnostics.Process.Start("explorer.exe", Arg);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(G.PendingSave)
            {
                var Result = MessageBox.Show(Application.Current.FindResource("SaveBeforeExit").ToString(), "TMC2", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if((Result==MessageBoxResult.Yes&&!AskForSave())||Result==MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
