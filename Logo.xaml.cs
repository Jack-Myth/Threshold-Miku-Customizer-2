using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Threshold_Miku_Customizer_2
{
    /// <summary>
    /// Interaction logic for Logo.xaml
    /// </summary>
    public partial class Logo : Window
    {
        DispatcherTimer  mTimer;
        MainWindow CurMainWindow;
        public Logo()
        {
            InitializeComponent();

            if (!Environment.GetCommandLineArgs().Contains("-nolocalization"))
            {
                List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
                foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
                    dictionaryList.Add(dictionary);
                string requestedCulture = @"Resources\lang_en-us.xaml";
                if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "zh")
                    requestedCulture = @"Resources\lang_zh-cn.xaml";
                ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedCulture));
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }
            try
            {
                string v = File.ReadAllText(".\\Customization\\.ver");            
                if (System.IO.File.Exists(".\\Customization\\Backup\\.ver"))
                {
                    string Ver = File.ReadAllText(".\\Customization\\Backup\\.ver");
                    if(Ver!=v)
                    {
                        MessageBox.Show(Application.Current.FindResource("DoNotOverrideSkin").ToString(), "TMC2", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.Shutdown();
                    }

                }
            }
            catch (Exception){ }
            CurMainWindow = new MainWindow();
            G.RegisterModifier<BackgroundImageModifier>();
            G.RegisterModifier<SidebarModifier>();
            G.RegisterModifier<WebPageModifier>();
            G.RegisterModifier<BlurBrightnessModifier>();
            G.RegisterModifier<ShowLWDModifier>();
            G.RegisterModifier<SpecialImgModifier>();
            G.RegisterModifier<FontModifier>();

            mTimer = new DispatcherTimer();
            mTimer.Interval = TimeSpan.FromSeconds(1);
            mTimer.Tick += ShowMainWindow;
            mTimer.Start();
        }

        public void ShowMainWindow(object sender, EventArgs e)
        {
            mTimer.Stop();
            CurMainWindow.Show();
            Close();
        }
    }
}
