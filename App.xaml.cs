using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Threshold_Miku_Customizer_2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(System.Windows.ExitEventArgs e)
        {
            base.OnExit(e);
            if (G.CachedFileList != null)
            {
                foreach (var tmpFile in G.CachedFileList)
                {
                    if (File.Exists(tmpFile))
                    {
                        try
                        {
                            File.Delete(tmpFile);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }
    }
}
