using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Shapes;

namespace Threshold_Miku_Customizer_2
{
    /// <summary>
    /// Interaction logic for FontsSetting.xaml
    /// </summary>
    public partial class FontsSetting : Window
    {
        List<ComboBox> ComboBoxList=new List<ComboBox>();
        Dictionary<string, string> FontSettingsDic;
        Dictionary<string, string> FontMap=new Dictionary<string, string>();
        public FontsSetting(ref Dictionary<string, string> _FontSettingsDic)
        {
            FontSettingsDic = _FontSettingsDic;
            InitializeComponent();
            ComboBoxList.Add(this.BaseFont);
            ComboBoxList.Add(this.Light);
            ComboBoxList.Add(this.SemiLight);
            ComboBoxList.Add(this.SemiBold);
            ComboBoxList.Add(this.Bold);
            ComboBoxList.Add(this.STUIGlobal);
            var installedFontCollection = new System.Drawing.Text.InstalledFontCollection();
            foreach (var fontfamily in installedFontCollection.Families)
            {
                FontMap[fontfamily.Name] = fontfamily.GetName(System.Globalization.CultureInfo.GetCultureInfo("en-us").LCID);
            }
            foreach (ComboBox i in ComboBoxList)
            {
                i.SelectionChanged += FontCommon_SelectionChanged;
                i.Items.Add("");
                foreach (var fontfamily in installedFontCollection.Families)
                {
                    i.Items.Add(fontfamily.Name);
                }
            }
            foreach (var i in FontSettingsDic)
            {
                foreach(var a in ComboBoxList)
                {
                    if (a.Name == i.Key)
                    {
                        a.SelectedItem = i.Value;
                        break;
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void FontCommon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cbox = (ComboBox)sender;
            if (cbox==null)
                return;
            FontSettingsDic[cbox.Name] = FontMap[cbox.SelectedItem.ToString()];
        }
    }
}
