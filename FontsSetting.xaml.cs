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
        List<Button> ButtonList = new List<Button>();
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
            ButtonList.Add(this.Uninstalled);
            ButtonList.Add(this.Installed);
            ButtonList.Add(this.Running);
            ButtonList.Add(this.Updating);
            var installedFontCollection = new System.Drawing.Text.InstalledFontCollection();
            FontMap["<Default>"] = "";
            foreach (var fontfamily in installedFontCollection.Families)
            {
                FontMap[fontfamily.Name] = fontfamily.GetName(System.Globalization.CultureInfo.GetCultureInfo("en-us").LCID);
            }
            foreach (ComboBox i in ComboBoxList)
            {
                i.SelectionChanged += FontCommon_SelectionChanged;
                foreach (var fontItem in FontMap)
                {
                    i.Items.Add(fontItem.Key);
                }
            }
            foreach(Button i in ButtonList)
            {
                i.Click += ColorCommon_Click;
            }

            List<string> fsdKeys = _FontSettingsDic.Keys.ToList();
            foreach(var fsdkey in fsdKeys)
            {
                foreach(var a in ComboBoxList)
                {
                    if (a.Name == fsdkey)
                    {
                        foreach (var fontName in FontMap)
                        {
                            if (fontName.Value== _FontSettingsDic[fsdkey])
                            {
                                a.SelectedItem = fontName.Key;
                                break;
                            }
                        }
                        goto ContinueTag;
                    }
                }
                foreach(var a in ButtonList)
                {
                    if (a.Name==fsdkey)
                    {
                        var DarwingColor = ColorTranslator.FromHtml(_FontSettingsDic[fsdkey]);
                        var MediaColor = System.Windows.Media.Color.FromRgb(DarwingColor.R, DarwingColor.G, DarwingColor.B);
                        a.Background =new SolidColorBrush(MediaColor);
                        break;
                    }
                }
            ContinueTag:
                continue;
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

        private void ColorCommon_Click(object sender, RoutedEventArgs e)
        {
            var ColorPicker = new System.Windows.Forms.ColorDialog();
            if(ColorPicker.ShowDialog()==System.Windows.Forms.DialogResult.OK)
            {
                FontSettingsDic[((Button)sender).Name] = ColorTranslator.ToHtml(ColorPicker.Color);
                var MediaColor = System.Windows.Media.Color.FromRgb(ColorPicker.Color.R, ColorPicker.Color.G, ColorPicker.Color.B);
                ((Button)sender).Background = new SolidColorBrush(MediaColor);
            }
        }
    }
}
