using System;
using System.Collections.Generic;
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

namespace Allegro_PCIE_Lane_Parser
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        UserProjectSettings userSettings = new UserProjectSettings();

        public class TextBoxValue
        {
            public string? textValue { get; set; }
        }

        public SettingsWindow()
        {
            InitializeComponent();
            text_toolsLocation.DataContext      = new TextBoxValue() { textValue = userSettings.cadenceToolLocation };
            text_pcieNetIdentifier.DataContext  = new TextBoxValue() { textValue = userSettings.pcieNetIdentifier };
            text_upiNetIdentifier.DataContext   = new TextBoxValue() { textValue = userSettings.upiNetIdentifier };
            text_clockNetIdentifier.DataContext = new TextBoxValue() { textValue = userSettings.clockNetIdentifier };
            text_usbNetIdentifier.DataContext   = new TextBoxValue() { textValue = userSettings.usbNetIdentifier };
            text_positiveDiffPair.DataContext   = new TextBoxValue() { textValue = userSettings.positiveDiffPairIdentifier };
            text_negativeDiffPair.DataContext   = new TextBoxValue() { textValue = userSettings.negativeDiffPairIdentifier };

            savedStatus.Visibility = Visibility.Hidden;
        }

        private void tools_btnOpenFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_saveSettings_Click(object sender, RoutedEventArgs e)
        {
            bool validSave = false;
            savedStatus.Visibility = Visibility.Hidden;

            foreach (var item in settings_StackPanel.Children.OfType<TextBox>())
            {
                validSave = (item.Text != string.Empty) ? true : false;
                if (!validSave) { break; }
            }

            if (validSave)
            {
                userSettings.cadenceToolLocation = text_toolsLocation.Text;
                userSettings.pcieNetIdentifier = text_pcieNetIdentifier.Text.ToUpper();
                userSettings.upiNetIdentifier = text_upiNetIdentifier.Text.ToUpper();
                userSettings.clockNetIdentifier = text_clockNetIdentifier.Text.ToUpper();
                userSettings.usbNetIdentifier = text_usbNetIdentifier.Text.ToUpper();
                userSettings.positiveDiffPairIdentifier = text_positiveDiffPair.Text.ToUpper();
                userSettings.negativeDiffPairIdentifier = text_negativeDiffPair.Text.ToUpper();

                savedStatus.Content = "Settings have been successfully saved";
                savedStatus.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x27, 0xB7, 0x00));
                //Brushes.LawnGreen         FF27B700;
                savedStatus.Visibility = Visibility.Visible;
                userSettings.Save();
            }
            else
            {
                savedStatus.Content = "Save was not successful. All fields must be populated";
                savedStatus.Foreground = Brushes.Red;
                savedStatus.Visibility = Visibility.Visible;
            }
        }
    }
}
