using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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
using BlockPenSimWPF.Data;
using BlockPenSimWPF.Shared.Models;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using WinRT;

namespace BlockPenSimWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            #if DEBUG
            serviceCollection.AddBlazorWebViewDeveloperTools();
            #endif
            Resources.Add("services", serviceCollection.BuildServiceProvider());

            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            if (sender is MainWindow)
            {
                MainWindow window = (MainWindow)sender;
                window.Title = "BlockPenSimWPF";

                if (ThemeData.GetCurrentTheme() == Theme.Dark)
                {
                    window.Background = new SolidColorBrush(new Color { R = 33, G = 37, B = 41, A = 255 }); //#212529
                }

                var size = LocalSettings.GetValue<Size>("WindowSize");
                if (size != default)
                {
                    window.Width = size.Width;
                    window.Height = size.Height;
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is MainWindow)
            {
                var size = e.NewSize;
                LocalSettings.SetValue("WindowSize", size);
            }
        }
    }
}
