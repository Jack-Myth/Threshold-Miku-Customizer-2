using System;
using System.Collections.Generic;
using System.Globalization;
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
            string v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            try
            {
                if (System.IO.File.Exists(".\\Customization\\Backup\\" + v))
                    System.IO.Directory.Delete(".\\Customization\\Backup");
            }
            catch (Exception){ }
            mTimer = new DispatcherTimer();
            mTimer.Interval = TimeSpan.FromSeconds(2);
            mTimer.Tick += ShowMainWindow;
            mTimer.Start();
        }

        public void ShowMainWindow(object sender, EventArgs e)
        {
            new MainWindow().Show();
            mTimer.Stop();
            Close();
        }
    }
}
